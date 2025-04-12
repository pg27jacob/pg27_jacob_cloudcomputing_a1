using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CloudSaveNoEnc : MonoBehaviour
{
    public LocalSaveManager localSaveManager;
    private HouseManager houseManager;

    string saveGameServerUrl = "http://localhost:3000/savegame";
    string loadGameServerUrl = "http://localhost:3000/loadgame";

    private void Start()
    {
        houseManager = FindAnyObjectByType<HouseManager>();
        if (houseManager == null)
        {
            Debug.LogError("HouseManager not found in the scene!");
        }
        if (localSaveManager == null)
        {
            Debug.LogError("LocalSaveManager not assigned in the Inspector!");
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            SendHousePlacementToCloudServer();
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            LoadHousePlacementFromCloudServer();
        }
    }

    private void SendHousePlacementToCloudServer()
    {
        if (houseManager == null)
        {
            Debug.LogError("HouseManager is null. Cannot send cloud save data.");
            return;
        }

        HouseManagerState localData = houseManager.GetSaveData();
        string json = JsonUtility.ToJson(localData);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        StartCoroutine(PostToServer(saveGameServerUrl, bodyRaw, "application/json", (success, response) =>
        {
            if (success)
            {
                Debug.Log($"House placement data sent to server and saved in cloudsaves.json. Response: {response}");
            }
            else
            {
                Debug.LogError($"Failed to send house placement data to server: {response}");
            }
        }));
    }

    private void LoadHousePlacementFromCloudServer()
    {
        StartCoroutine(GetFromServer(loadGameServerUrl, (success, response) =>
        {
            if (success)
            {
                Debug.Log($"House placement data received from cloudsaves.json: {response}");
                try
                {
                    HouseManagerState serverData = JsonUtility.FromJson<HouseManagerState>(response);
                    if (houseManager != null)
                    {
                        houseManager.LoadSaveData(serverData);
                        Debug.Log("House placement data loaded from server.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing server response: {e.Message}");
                    Debug.Log($"Server Response: {response}");
                }
            }
            else
            {
                Debug.LogError($"Failed to retrieve house placement data from server: {response}");
            }
        }));
    }

    private IEnumerator PostToServer(string url, byte[] bodyRaw, string contentType, Action<bool, string> callback)
    {
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", contentType);

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(false, www.error);
            }
            else
            {
                callback?.Invoke(true, www.downloadHandler.text);
            }
        }
    }

    private IEnumerator GetFromServer(string url, Action<bool, string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                callback?.Invoke(false, www.error);
            }
            else
            {
                callback?.Invoke(true, www.downloadHandler.text);
            }
        }
    }
}