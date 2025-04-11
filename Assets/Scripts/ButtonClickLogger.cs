using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Linq;


public class ButtonClickLogger : MonoBehaviour
{
    public Button myButton;
    public string housePrefabName = "PlacedPrefab_";


    private void Start()
    {
        if(myButton != null)
        {
            myButton.onClick.AddListener(onButtonClicked); 
        }
    }

    public void onButtonClicked()
    {
        List<string> housePositionsFormatted = new List<string>();

        
        GameObject[] placedHouses = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(obj => obj.name.StartsWith(housePrefabName))
            .ToArray();

        foreach (GameObject house in placedHouses)
        {
            housePositionsFormatted.Add($"({house.transform.position.x:F2},{house.transform.position.y:F2})");
        }

        TelemetryManager.Instance.LogEvent("button_click'd", new Dictionary<string, object>
        {
            { "buttonName", myButton.name },
            { "clickTime", System.DateTime.UtcNow.ToString("o") },
            { "housePositionValue", housePositionsFormatted }
        });
    }
}
