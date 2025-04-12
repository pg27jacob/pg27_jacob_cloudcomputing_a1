using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq; // Add this for ToList()

[Serializable]
public struct HouseData
{
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class HouseManagerState
{
    public List<HouseData> placedHouses;

    public HouseManagerState()
    {
        placedHouses = new List<HouseData>();
    }
}

public class HouseManager : MonoBehaviour
{
    [Header("Prefab Placement")]
    public GameObject placementPrefab;
    public LayerMask collisionCheckLayer;

    [Header("Preview Settings")]
    public float rotationSpeed = 10f;
    public KeyCode deleteModifierKey = KeyCode.LeftShift;

    private GameObject previewObject;
    private Collider2D previewCollider;
    private Camera mainCam;

    private List<GameObject> placedHouseInstances = new List<GameObject>();

    void Start()
    {
        mainCam = Camera.main;

        if (placementPrefab != null)
        {
            previewObject = Instantiate(placementPrefab);
            previewObject.name = "PlacementPreview";
            previewObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            previewCollider = previewObject.GetComponent<Collider2D>();
            if (previewCollider != null)
            {
                previewCollider.enabled = false;
            }
        }
    }

    void Update()
    {
        if (previewObject == null) return;

        Vector2 mousePosition = GetMouseWorldPosition();
        previewObject.transform.position = mousePosition;

        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseScroll) > 0.01f)
        {
            previewObject.transform.Rotate(Vector3.forward, -mouseScroll * rotationSpeed, Space.Self);
        }

        bool canPlace = true;
        if (previewCollider != null)
        {
            previewCollider.enabled = true;

            Collider2D[] hitColliders = new Collider2D[5];
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(collisionCheckLayer);
            int hits = previewCollider.Overlap(filter, hitColliders);

            if (hits > 0)
            {
                canPlace = false;
            }

            previewCollider.enabled = false;
        }

        SetPreviewColor(canPlace ? Color.white : Color.red);

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(deleteModifierKey))
            {
                DeletePrefabUnderMouse();
            }
            else
            {
                if (canPlace)
                {
                    PlacePrefab(mousePosition, previewObject.transform.rotation);
                }
            }
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        return mainCam.ScreenToWorldPoint(mousePos);
    }

    private void SetPreviewColor(Color color)
    {
        SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
        }
    }

    public void PlacePrefab(Vector2 position, Quaternion rotation)
    {
        GameObject placedObj = Instantiate(placementPrefab, position, rotation);
        placedObj.name = "PlacedPrefab_" + Time.time;
        placedHouseInstances.Add(placedObj); // Keep track of placed houses
        GameManager.Instance.CountNonWhitePixels();
        Debug.Log($"House placed at World Position: {position}, Rotation: {rotation.eulerAngles}");
    }

    private void DeletePrefabUnderMouse()
    {
        Vector2 mousePos = GetMouseWorldPosition();

        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, collisionCheckLayer);
        if (hit.collider != null)
        {
            GameObject toDelete = hit.collider.gameObject;
            if (toDelete != previewObject && placedHouseInstances.Contains(toDelete))
            {
                placedHouseInstances.Remove(toDelete);
                Destroy(toDelete);
                GameManager.Instance.CountNonWhitePixels();
            }
        }
    }

    // Public method to get the current state of placed houses for saving
    public HouseManagerState GetSaveData()
    {
        HouseManagerState state = new HouseManagerState();
        state.placedHouses = placedHouseInstances.Select(house => new HouseData { position = house.transform.position, rotation = house.transform.rotation }).ToList();
        return state;
    }

    // Public method to load the placed houses
    public void LoadSaveData(HouseManagerState state)
    {
        // Clear any existing placed houses
        foreach (var house in placedHouseInstances)
        {
            Destroy(house);
        }
        placedHouseInstances.Clear();

        if (state != null && state.placedHouses != null)
        {
            foreach (var houseData in state.placedHouses)
            {
                GameObject placedObj = Instantiate(placementPrefab, houseData.position, houseData.rotation);
                placedObj.name = "LoadedPrefab_" + Time.time;
                placedHouseInstances.Add(placedObj);
            }
            Debug.Log($"Loaded {placedHouseInstances.Count} houses.");
        }
        else
        {
            Debug.Log("No house data to load.");
        }
    }
}