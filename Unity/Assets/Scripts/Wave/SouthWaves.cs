using UnityEngine;

public class SouthWaves : MonoBehaviour
{
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

    private float startTime;
    private float currentAmplitudeMultiplier = 1f;
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    void Start()
    {
        startTime = Time.time;

        // Mesh Setup
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

        // グリッド座標の計算
        int x1 = Mathf.FloorToInt(x);
        int x2 = x1 + 1;
        int z1 = Mathf.FloorToInt(z);
        int z2 = z1 + 1;

        // 範囲内に制限
        x1 = Mathf.Clamp(x1, 0, Dimension);
        x2 = Mathf.Clamp(x2, 0, Dimension);
        z1 = Mathf.Clamp(z1, 0, Dimension);
        z2 = Mathf.Clamp(z2, 0, Dimension);

        // 補間用の重み計算
        float tx = x - x1;
        float tz = z - z1;

        // 4つの頂点の高さを取得
        float h11 = Mesh.vertices[index(x1, z1)].y;
        float h12 = Mesh.vertices[index(x1, z2)].y;
        float h21 = Mesh.vertices[index(x2, z1)].y;
        float h22 = Mesh.vertices[index(x2, z2)].y;

        // バイリニア補間
        float h1 = Mathf.Lerp(h11, h12, tz);
        float h2 = Mathf.Lerp(h21, h22, tz);
        float height = Mathf.Lerp(h1, h2, tx);

        return transform.position.y + height * transform.lossyScale.y;
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
        var tries = new int[Mesh.vertices.Length * 6];

        for (int x = 0; x < Dimension; x++)
        {
            for (int z = 0; z < Dimension; z++)
            {
                tries[index(x, z) * 6 + 0] = index(x, z);
                tries[index(x, z) * 6 + 1] = index(x + 1, z + 1);
                tries[index(x, z) * 6 + 2] = index(x + 1, z);
                tries[index(x, z) * 6 + 3] = index(x, z);
                tries[index(x, z) * 6 + 4] = index(x, z + 1);
                tries[index(x, z) * 6 + 5] = index(x + 1, z + 1);
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
                var vec = new Vector2((x / UVScale) % 2, (z / UVScale) % 2);
                uvs[index(x, z)] = new Vector2(vec.x <= 1 ? vec.x : 2 - vec.x, vec.y <= 1 ? vec.y : 2 - vec.y);
            }
        }

        return uvs;
    }

    private int index(int x, int z)
    {
        return x * (Dimension + 1) + z;
    }

    private int index(float x, float z)
    {
        return index((int)x, (int)z);
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

        // 成長係数の更新
        float timeSinceStart = Time.time - startTime;
        float growthProgress = Mathf.Clamp01(timeSinceStart / growthDuration);
        currentAmplitudeMultiplier = Mathf.Lerp(1f, maxAmplitudeMultiplier, growthProgress);

        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                // z座標に基づく振幅の調整係数を反転
                float amplitudeScale = 1f - (z / (float)Dimension);

                // 基本の正弦波
                float baseWave = (baseAmplitude * amplitudeScale * currentAmplitudeMultiplier) *
                    Mathf.Sin((x * baseFrequency + timeOffset) * Mathf.PI / Dimension);

                // パーリンノイズ
                float noiseX = (x + noiseTimeOffset * noiseDirection.x) * noiseScale / Dimension;
                float noiseZ = (z + noiseTimeOffset * noiseDirection.y) * noiseScale / Dimension;
                float noise = (Mathf.PerlinNoise(noiseX, noiseZ) * 2f - 1f) *
                    noiseAmplitude * amplitudeScale * currentAmplitudeMultiplier;

                // 最終的な高さを設定
                verts[index(x, z)] = new Vector3(x, baseWave + noise, z);
            }
        }

        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }
}