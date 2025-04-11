using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BlobGenerator : MonoBehaviour
{
    [Header("Blob Generation Settings")]
    public int seed = 0;

    public int vertexCount = 8;

    public float baseRadius = 1f;

    public float radiusVariance = 0.5f;

    public float verticalOffset = 0f;

    public Material material;

    private Mesh mesh;

    private void Awake()
    {
        GenerateBlob(seed);
    }

    [ContextMenu("Regenerate Blob")]
    public void GenerateBlob(int newSeed)
    {
        seed = newSeed;
        if (mesh == null)
        {
            mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = mesh;
        }
        else
        {
            mesh.Clear();
        }

        Random.InitState(seed);

        List<Vector3> vertices2D = new List<Vector3>();
        float angleStep = 360f / vertexCount;
        for (int i = 0; i < vertexCount; i++)
        {
            float angleDeg = i * angleStep;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float currentRadius = baseRadius + Random.Range(-radiusVariance, radiusVariance);

            float x = Mathf.Cos(angleRad) * currentRadius;
            float y = Mathf.Sin(angleRad) * currentRadius + verticalOffset;

            vertices2D.Add(new Vector3(x, y, 0f));
        }

        int[] triangles = Triangulate(vertices2D);

        mesh.SetVertices(vertices2D);
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<Renderer>().material = material;

    }

    private int[] Triangulate(List<Vector3> vertices2D)
    {
        List<int> indices = new List<int>();

        for (int i = 1; i < vertices2D.Count - 1; i++)
        {
            indices.Add(0);
            indices.Add(i);
            indices.Add(i + 1);
        }

        return indices.ToArray();
    }
}
