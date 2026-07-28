using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaveMeshJarnal : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int Dimension = 250;
    public float UVScale = 10f;

    [Header("Wave Settings")]
    public float baseAmplitude = 7f;
    public float baseFrequency = 1f;
    public float baseSpeed = 1f;

    [Header("Noise Settings")]
    public float noiseScale = 60f;
    public float noiseSpeed = 0.5f;
    public float noiseAmplitude = 8f;
    public Vector2 noiseDirection = new Vector2(-1f, 1f);

    [Header("Wave Growth Settings")]
    public float growthDuration = 0f;
    public float maxAmplitudeMultiplier = 1f;

    [Header("External Settings")]
    public DifficultyDataSO difficultySettings;

    private float startTime;
    private float amplitudeMultiplier = 1f;
    private Mesh mesh;

    void Start()
    {
        startTime = Time.time;

        mesh = new Mesh { name = gameObject.name };
        mesh.vertices  = GenerateVerts();
        mesh.triangles = GenerateTris();
        mesh.uv        = GenerateUVs();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void Update()
    {
        amplitudeMultiplier = Mathf.Lerp(1f, maxAmplitudeMultiplier,
            Mathf.Clamp01((Time.time - startTime) / Mathf.Max(growthDuration, 0.001f)));

        UpdateMesh();
    }

    void UpdateMesh()
    {
        bool scaleZSmallToLarge = difficultySettings != null && difficultySettings.rotationFlag;
        float elapsedTime = Time.time - startTime;
        var verts = mesh.vertices;

        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                float y = WaveCalculator.CalcLocalHeight(
                    x, z,
                    Dimension,
                    baseAmplitude, baseFrequency, baseSpeed,
                    noiseScale, noiseSpeed, noiseAmplitude, noiseDirection,
                    amplitudeMultiplier,
                    elapsedTime,
                    scaleZSmallToLarge);

                verts[Index(x, z)] = new Vector3(x, y, z);
            }
        }

        mesh.vertices = verts;
        mesh.RecalculateNormals();
    }

    Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimension + 1) * (Dimension + 1)];
        for (int x = 0; x <= Dimension; x++)
            for (int z = 0; z <= Dimension; z++)
                verts[Index(x, z)] = new Vector3(x, 0, z);
        return verts;
    }

    int[] GenerateTris()
    {
        var tris = new int[Dimension * Dimension * 6];
        for (int x = 0; x < Dimension; x++)
        {
            for (int z = 0; z < Dimension; z++)
            {
                int b = (x * Dimension + z) * 6;
                tris[b + 0] = Index(x,     z);
                tris[b + 1] = Index(x,     z + 1);
                tris[b + 2] = Index(x + 1, z);
                tris[b + 3] = Index(x + 1, z);
                tris[b + 4] = Index(x,     z + 1);
                tris[b + 5] = Index(x + 1, z + 1);
            }
        }
        return tris;
    }

    Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[(Dimension + 1) * (Dimension + 1)];
        for (int x = 0; x <= Dimension; x++)
            for (int z = 0; z <= Dimension; z++)
                uvs[Index(x, z)] = new Vector2((float)x / Dimension * UVScale, (float)z / Dimension * UVScale);
        return uvs;
    }

    int Index(int x, int z) => x * (Dimension + 1) + z;
}
