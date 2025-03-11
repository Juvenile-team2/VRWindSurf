using UnityEngine;
using System.Collections;

public class BigWaveCreate : MonoBehaviour
{
    private Waves waves;
    private MeshFilter meshFilter;

    [Header("Wave Settings")]
    public float waveHeight = 5f;
    public float waveSpeed = 2f;
    public float waveWidth = 3f;
    public float leadingSteepness = 1.5f;
    public float trailingSteepness = 1f;

    private bool isWaveActive = false;
    private float wavePosition;
    private Vector3[] originalVertices;
    private bool isInitialized = false;

    void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Wavesコンポーネントの取得
        waves = GetComponent<Waves>();
        if (waves == null)
        {
            waves = FindObjectOfType<Waves>();
            if (waves == null)
            {
                Debug.LogError("Waves component not found!");
                return;
            }
        }

        // MeshFilterの取得または追加
        meshFilter = waves.gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = waves.gameObject.AddComponent<MeshFilter>();
            Debug.Log("Added MeshFilter component");
        }

        // メッシュの初期化
        if (meshFilter.mesh == null)
        {
            meshFilter.mesh = new Mesh();
            Debug.Log("Created new mesh");
        }

        // 元の頂点位置を保存
        try
        {
            originalVertices = meshFilter.mesh.vertices;
            isInitialized = true;
            Debug.Log("Successfully initialized SingleWaveController");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing vertices: {e.Message}");
            isInitialized = false;
        }
    }

    void Update()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Components not properly initialized. Attempting to reinitialize...");
            InitializeComponents();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isWaveActive)
        {
            StartWave();
        }
    }

    public void StartWave()
    {
        if (!isInitialized)
        {
            Debug.LogError("Cannot start wave - system not initialized!");
            return;
        }

        if (!isWaveActive)
        {
            isWaveActive = true;
            wavePosition = -waveWidth * 2;
            StartCoroutine(PropagateWave());
            Debug.Log("Single wave started!");
        }
    }

    private IEnumerator PropagateWave()
    {
        if (!isInitialized || meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("Required components missing for wave propagation!");
            isWaveActive = false;
            yield break;
        }

        Mesh mesh = meshFilter.mesh;
        int dimension = waves.Dimension;

        while (wavePosition < dimension + waveWidth * 2)
        {
            try
            {
                Vector3[] currentVertices = mesh.vertices;
                Vector3[] newVertices = new Vector3[currentVertices.Length];
                System.Array.Copy(originalVertices, newVertices, originalVertices.Length);

                // 波の計算
                for (int x = 0; x <= dimension; x++)
                {
                    for (int z = 0; z <= dimension; z++)
                    {
                        int vertexIndex = x * (dimension + 1) + z;
                        float y = 0f;

                        // 既存のWavesの波を計算
                        foreach (var octave in waves.Octaves)
                        {
                            if (octave.alternate)
                            {
                                float perl = Mathf.PerlinNoise(
                                    (x * octave.scale.x) / dimension,
                                    (z * octave.scale.y) / dimension
                                ) * Mathf.PI * 2f;
                                y += Mathf.Cos(perl + octave.speed.magnitude * Time.time) * octave.height;
                            }
                            else
                            {
                                float perl = Mathf.PerlinNoise(
                                    (x * octave.scale.x + Time.time * octave.speed.x) / dimension,
                                    (z * octave.scale.y + Time.time * octave.speed.y) / dimension
                                ) - 0.5f;
                                y += perl * octave.height;
                            }
                        }

                        // 単一の大きな波を追加
                        float distanceFromWave = z - wavePosition;
                        float waveInfluence;

                        if (distanceFromWave < 0)
                        {
                            waveInfluence = Mathf.Exp(-Mathf.Pow(distanceFromWave, 2) / (2 * waveWidth * leadingSteepness));
                        }
                        else
                        {
                            waveInfluence = Mathf.Exp(-Mathf.Pow(distanceFromWave, 2) / (2 * waveWidth * trailingSteepness));
                        }

                        y += waveInfluence * waveHeight;
                        newVertices[vertexIndex].y = y;
                    }
                }

                mesh.vertices = newVertices;
                mesh.RecalculateNormals();

                wavePosition += waveSpeed * Time.deltaTime;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error during wave propagation: {e.Message}");
                break;
            }
        }

        isWaveActive = false;
        Debug.Log("Single wave completed!");
    }

    void OnDisable()
    {
        // コンポーネントが無効になった時にクリーンアップ
        isWaveActive = false;
        StopAllCoroutines();
    }
}