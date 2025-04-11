using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TelemetryManager : MonoBehaviour
{
    string serverURL = "http://localhost:3000/telemetry";

    public static TelemetryManager Instance;

    private Queue<Dictionary<string, object>> eventQueue;

    private bool isSending = false;

    public ButtonClickLogger houseManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            eventQueue = new Queue<Dictionary<string, object>>();
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
            // if its null create a dictionary and avoid errors
            parameters = new Dictionary<string, object>();
        }

        parameters["eventName"] = eventName;
        parameters["sessionId"] = System.Guid.NewGuid().ToString();
        parameters["deviceTime"] = System.DateTime.UtcNow.ToString("o"); // year than month than day than minute, seconds, mili.

        eventQueue.Enqueue(parameters);

        if (!isSending) StartCoroutine(SendEvents());
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

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError($"Error {request.error}");
                    eventQueue.Enqueue(currentEvent);
                    break;
                }
                else
                {
                    Debug.Log("Request Sent: " + payload);
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
}