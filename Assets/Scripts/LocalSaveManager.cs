using System;
using System.IO;
using UnityEngine;

public class LocalSaveManager : MonoBehaviour
{
    private string filename = "localSaveNoEnx.json";
    private string saveFolderName = "SaveData";
    private string assetsSavePath; // Path within the Assets folder

    private string FilePath
    {
        get
        {
            return Path.Combine(assetsSavePath, filename);
        }
    }

    private void Awake()
    {
        assetsSavePath = Path.Combine(Application.dataPath, saveFolderName);

        // Create the save directory in Assets if it doesn't exist (Editor only)
#if UNITY_EDITOR
        if (!Directory.Exists(assetsSavePath))
        {
            Directory.CreateDirectory(assetsSavePath);
            Debug.Log($"Created save directory in Assets: {assetsSavePath}");
        }
#endif

        LoadFromLocal();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SaveToLocal();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            LoadFromLocal();
        }
    }

    private void LoadFromLocal()
    {
        if (File.Exists(FilePath))
        {
            try
            {
                string json = File.ReadAllText(FilePath);
                HouseManagerState loadedState = JsonUtility.FromJson<HouseManagerState>(json);
                if (houseManager != null)
                {
                    houseManager.LoadSaveData(loadedState);
                    Debug.Log("House data loaded from " + FilePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load house data: " + e.Message);
            }
        }
        else
        {
            Debug.Log("No house save data found at " + FilePath);
        }
    }

    public void SaveToLocal()
    {
        if (houseManager != null)
        {
            HouseManagerState stateToSave = houseManager.GetSaveData();
            string json = JsonUtility.ToJson(stateToSave);
            File.WriteAllText(FilePath, json);
            Debug.Log("House data saved to " + FilePath);
        }
        else
        {
            Debug.LogError("HouseManager is null. Cannot save house data.");
        }
    }

    private HouseManager houseManager; // Get a reference to the HouseManager in the scene
    private void Start()
    {
        houseManager = FindAnyObjectByType<HouseManager>();
        if (houseManager == null)
        {
            Debug.LogError("HouseManager not found in the scene!");
        }
    }
}