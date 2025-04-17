using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TelemetryManager : MonoBehaviour
{
    string serverURL = "http://localhost:3000/telemetry";

    private string filename = "localTelemetryData.json";
    private string saveFolderName = "SaveData";
    private string assetsSavePath; // Path within the Assets folder
    private string localFilePath; // The actual path used for saving

    private string FilePath
    {
        get
        {
            return Path.Combine(assetsSavePath, filename);
        }
    }

    public static TelemetryManager Instance;

    private Queue<Dictionary<string, object>> eventQueue;
    private bool isSending = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            eventQueue = new Queue<Dictionary<string, object>>();

            // Define the path within the Assets folder
            assetsSavePath = Path.Combine(Application.dataPath, saveFolderName);

            // Determine the actual local file path based on the build environment
#if UNITY_EDITOR
            localFilePath = FilePath; // Save to Assets/SaveData in the Editor
            Debug.Log($"Saving telemetry to: {localFilePath} (Editor)");
#else
            localFilePath = Path.Combine(Application.persistentDataPath, saveFolderName, filename); // Save to persistentDataPath in builds
            // Ensure the directory exists in persistentDataPath
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
            Debug.Log($"Saving telemetry to: {localFilePath} (Build)");
#endif

            LoadLocalData(); // Load any existing local data on startup
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (parameters == null)
        {
            parameters = new Dictionary<string, object>();
        }

        parameters["eventName"] = eventName;
        parameters["sessionId"] = System.Guid.NewGuid().ToString();
        parameters["deviceTime"] = System.DateTime.UtcNow.ToString("o");

        eventQueue.Enqueue(parameters);
        SaveEventLocally(parameters);

        if (!isSending) StartCoroutine(SendEvents());
    }

    private void SaveEventLocally(Dictionary<string, object> eventData)
    {
        string json = JsonUtility.ToJson(new SerializationWrapper(eventData)) + ",";
        try
        {
            // Ensure the directory exists before trying to save
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));
            File.AppendAllText(localFilePath, json);
            Debug.Log("Telemetry event saved locally.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save telemetry event locally: {e.Message}");
        }
    }

    private void LoadLocalData()
    {
        if (File.Exists(localFilePath))
        {
            try
            {
                string fileContent = File.ReadAllText(localFilePath);
                if (fileContent.EndsWith(","))
                {
                    fileContent = fileContent.Substring(0, fileContent.Length - 1);
                }
                fileContent = "[" + fileContent + "]";

                if (!string.IsNullOrEmpty(fileContent) && fileContent != "[]")
                {
                    SerializationWrapper[] loadedEventsWrapper = JsonHelper.FromJson<SerializationWrapper>(fileContent);
                    if (loadedEventsWrapper != null)
                    {
                        foreach (var wrapper in loadedEventsWrapper)
                        {
                            Dictionary<string, object> loadedEvent = new Dictionary<string, object>();
                            for (int i = 0; i < wrapper.keys.Count; i++)
                            {
                                loadedEvent[wrapper.keys[i]] = wrapper.values[i];
                            }
                            eventQueue.Enqueue(loadedEvent);
                        }
                        Debug.Log($"Loaded {eventQueue.Count} telemetry events from local storage.");
                        if (!isSending && eventQueue.Count > 0)
                        {
                            StartCoroutine(SendEvents());
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load telemetry data from local storage: {e.Message}");
            }
        }
    }

    private IEnumerator SendEvents()
    {
        isSending = true;

        while (eventQueue.Count > 0)
        {
            Dictionary<string, object> currentEvent = eventQueue.Dequeue();
            string payload = JsonUtility.ToJson(new SerializationWrapper(currentEvent));

            using (UnityWebRequest request = new UnityWebRequest(serverURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                // TODO: Add bearer token --- "bearer asldasjghag"

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error sending telemetry: {request.error}");
                    eventQueue.Enqueue(currentEvent);
                    break;
                }
                else
                {
                    Debug.Log("Telemetry Sent: " + payload);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        isSending = false;
    }

    [System.Serializable]
    private class SerializationWrapper
    {
        public List<string> keys = new List<string>();
        public List<string> values = new List<string>();

        public SerializationWrapper(Dictionary<string, object> parameters)
        {
            foreach (var kvp in parameters)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value.ToString());
            }
        }
    }

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>("{ \"Items\": " + json + "}");
            return wrapper.Items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}