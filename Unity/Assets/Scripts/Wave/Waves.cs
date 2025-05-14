using UnityEngine;
public class Waves : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int Dimension = 10;
    public float UVScale = 2f;

    [Header("Wave Settings")]
    public float baseAmplitude = 0.5f;
    public float baseFrequency = 1.0f;
    public float baseSpeed = 1.0f;

    [Header("Noise Settings")]
    public float noiseScale = 1.0f;
    public float noiseSpeed = 0.5f;
    public float noiseAmplitude = 0.3f;
    public Vector2 noiseDirection = new Vector2(1f, 1f);

    [Header("Wave Growth Settings")]
    public float growthDuration = 10f;
    public float maxAmplitudeMultiplier = 2f;
    
    [Header("External Settings")]
    public DifficultyDataSO difficultySettings;

    private float startTime;
    private float currentOverallAmplitudeMultiplier = 1f;
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    void Start()
    {
        if (difficultySettings == null)
        {
            Debug.LogWarning("DifficultyDataSO is not assigned to the Waves script. Default Z-scaling behavior will be used (Z-large means larger waves).");
        }

        startTime = Time.time;

        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTries();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter = gameObject.AddComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;
    }

    public float GetHeight(Vector3 position)
    {
        Vector3 localPos = transform.InverseTransformPoint(position);

        float x = localPos.x;
        float z = localPos.z;

        int x1 = Mathf.FloorToInt(x);
        int x2 = x1 + 1;
        int z1 = Mathf.FloorToInt(z);
        int z2 = z1 + 1;

        x1 = Mathf.Clamp(x1, 0, Dimension);
        x2 = Mathf.Clamp(x2, 0, Dimension);
        z1 = Mathf.Clamp(z1, 0, Dimension);
        z2 = Mathf.Clamp(z2, 0, Dimension);

        float tx = x - x1;
        float tz = z - z1;

        float h11 = Mesh.vertices[index(x1, z1)].y;
        float h12 = Mesh.vertices[index(x1, z2)].y;
        float h21 = Mesh.vertices[index(x2, z1)].y;
        float h22 = Mesh.vertices[index(x2, z2)].y;

        float h1 = Mathf.Lerp(h11, h12, tz);
        float h2 = Mathf.Lerp(h21, h22, tz);
        float heightInLocalSpace = Mathf.Lerp(h1, h2, tx);

        return transform.TransformPoint(new Vector3(0, heightInLocalSpace, 0)).y;
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimension + 1) * (Dimension + 1)];
        for (int x = 0; x <= Dimension; x++)
            for (int z = 0; z <= Dimension; z++)
                verts[index(x, z)] = new Vector3(x, 0, z);
        return verts;
    }

    private int[] GenerateTries()
    {
        var tries = new int[Dimension * Dimension * 6];
        for (int x = 0; x < Dimension; x++)
        {
            for (int z = 0; z < Dimension; z++)
            {
                int baseIndex = (x * Dimension + z) * 6;
                tries[baseIndex + 0] = index(x, z);
                tries[baseIndex + 1] = index(x, z + 1);
                tries[baseIndex + 2] = index(x + 1, z);
                tries[baseIndex + 3] = index(x + 1, z);
                tries[baseIndex + 4] = index(x, z + 1);
                tries[baseIndex + 5] = index(x + 1, z + 1);
            }
        }
        return tries;
    }

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[Mesh.vertices.Length];
        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                float u = (float)x / Dimension * UVScale;
                float v = (float)z / Dimension * UVScale;
                uvs[index(x, z)] = new Vector2(u, v);
            }
        }
        return uvs;
    }

    private int index(int x, int z)
    {
        return x * (Dimension + 1) + z;
    }

    void Update()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        var verts = Mesh.vertices;
        float timeOffset = Time.time * baseSpeed;
        float noiseTimeOffset = Time.time * noiseSpeed;

        float timeSinceStart = Time.time - startTime;
        float growthProgress = Mathf.Clamp01(timeSinceStart / growthDuration);
        currentOverallAmplitudeMultiplier = Mathf.Lerp(1f, maxAmplitudeMultiplier, growthProgress);
        bool scaleZSmallToLarge = false; 
        if (difficultySettings != null)
        {
            scaleZSmallToLarge = difficultySettings.rotationFlag; 
        }

        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                float zBasedAmplitudeScale = 0f;
                if (Dimension > 0)
                {
                    if (scaleZSmallToLarge) // true: ZÇ™è¨Ç≥Ç¢ÇŸÇ«îgÇ™ëÂÇ´Ç¢
                    {
                        zBasedAmplitudeScale = 1.0f - (z / (float)Dimension);
                    }
                    else // false: ZÇ™ëÂÇ´Ç¢ÇŸÇ«îgÇ™ëÂÇ´Ç¢
                    {
                        zBasedAmplitudeScale = z / (float)Dimension;
                    }
                }
                else if (Dimension == 0 && z == 0)
                {
                    zBasedAmplitudeScale = scaleZSmallToLarge ? 1.0f : 0.0f;
                }

                float currentBaseAmplitude = baseAmplitude * zBasedAmplitudeScale * currentOverallAmplitudeMultiplier;
                float baseWave = currentBaseAmplitude * Mathf.Sin((x * baseFrequency + timeOffset) * Mathf.PI / Dimension);

                float currentNoiseAmplitude = noiseAmplitude * zBasedAmplitudeScale * currentOverallAmplitudeMultiplier;
                float noiseX = (x + noiseTimeOffset * noiseDirection.x) * noiseScale / Dimension;
                float noiseZ = (z + noiseTimeOffset * noiseDirection.y) * noiseScale / Dimension;
                float noise = (Mathf.PerlinNoise(noiseX, noiseZ) * 2f - 1f) * currentNoiseAmplitude;

                Vector3 originalVertex = new Vector3(x, 0, z);
                verts[index(x, z)] = new Vector3(originalVertex.x, baseWave + noise, originalVertex.z);
            }
        }

        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }
}