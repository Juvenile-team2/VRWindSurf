using UnityEngine;
using System.Collections;

public class WaveCreate : MonoBehaviour
{
    private Waves waves;
    public float bigWaveHeight = 5f;      // ”g‚Ì‚‚³
    public float waveSpeed = 2f;          // ”g‚Ìis‘¬“x
    public float waveWidth = 3f;          // ”g‚Ì•
    public float wavePersistence = 0.5f;   // ”g‚Ì‘±ŠÔ
    private bool canCreateWave = true;
    private int waveCount = 0;

    [Header("Wave Shape")]
    public float leadingEdgesteepness = 1.5f;  // ”g‚Ì‘O–Ê‚Ì‹}s‚³
    public float trailingEdgeSteepness = 1f;   // ”g‚ÌŒã–Ê‚Ì‹}s‚³

    [System.Obsolete]
    void Start()
    {
        waves = GetComponent<Waves>();
        if (waves == null)
        {
            waves = FindObjectOfType<Waves>();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canCreateWave)
        {
            StartCoroutine(CreateSmoothTravelingWave());
        }
    }

    private IEnumerator CreateSmoothTravelingWave()
    {
        canCreateWave = false;
        waveCount++;
        Debug.Log($"Smooth Traveling Wave #{waveCount} created!");

        float wavePosition = -waveWidth * 2; // ‰æ–ÊŠO‚©‚çŠJn
        float maxPosition = waves.Dimension + waveWidth * 2; // ‰æ–ÊŠO‚Ü‚Å‘±‚¯‚é

        while (wavePosition < maxPosition)
        {
            // Œ³‚Ì’¸“_‚ğ•Û‘¶
            var verts = waves.GetComponent<MeshFilter>().mesh.vertices;
            var originalVerts = new Vector3[verts.Length];
            System.Array.Copy(verts, originalVerts, verts.Length);

            for (int x = 0; x <= waves.Dimension; x++)
            {
                for (int z = 0; z <= waves.Dimension; z++)
                {
                    int vertexIndex = x * (waves.Dimension + 1) + z;

                    // ”g‚©‚ç‚Ì‹——£‚ğŒvZ
                    float distanceFromWave = z - wavePosition;

                    // ”ñ‘ÎÌ‚È”gŒ`‚ğì¬
                    float waveInfluence = 0;
                    if (distanceFromWave < 0)
                    {
                        // ”g‚Ì‘O–Êi‚æ‚è‹}sj
                        waveInfluence = Mathf.Exp(-Mathf.Pow(distanceFromWave, 2) / (2 * waveWidth * leadingEdgesteepness));
                    }
                    else
                    {
                        // ”g‚ÌŒã–Êi‚æ‚è‚È‚¾‚ç‚©j
                        waveInfluence = Mathf.Exp(-Mathf.Pow(distanceFromWave, 2) / (2 * waveWidth * trailingEdgeSteepness));
                    }

                    // Šù‘¶‚Ì”g‚Ì‚‚³‚ÉV‚µ‚¢”g‚ğ’Ç‰Á
                    verts[vertexIndex].y = originalVerts[vertexIndex].y + waveInfluence * bigWaveHeight;
                }
            }

            // ƒƒbƒVƒ…‚ğXV
            waves.GetComponent<MeshFilter>().mesh.vertices = verts;
            waves.GetComponent<MeshFilter>().mesh.RecalculateNormals();

            // ”g‚ği‚ß‚é
            wavePosition += waveSpeed * Time.deltaTime;

            yield return null;
        }

        Debug.Log($"Wave #{waveCount} completed its travel");
        yield return new WaitForSeconds(wavePersistence);
        canCreateWave = true;
    }

    // ”g‚Ì‚‚³‚ğæ“¾‚·‚éƒ†[ƒeƒBƒŠƒeƒBƒƒ\ƒbƒh
    private float GetWaveHeight(float distance, float width, float steepness)
    {
        return Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * width * steepness));
    }
}