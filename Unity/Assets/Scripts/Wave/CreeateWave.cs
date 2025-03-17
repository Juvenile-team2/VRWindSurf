using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateWave : MonoBehaviour
{
    // Wave Parameters
    public int Dimension = 10;
    public float UVScale = 2f;
    public float WaveSpeed = 5.0f;        // 方向波の速度
    public float WaveHeight = 1.5f;       // 方向波の高さ 
    public float WaveDuration = 3.0f;     // 波の持続時間（秒）
    public float WaveWidth = 3.0f;        // 波の幅（大きいほど幅広い波になる）

    // 波の方向制御用
    private Vector2 currentWaveDirection = Vector2.right;  // 初期値は右方向
    private bool isWaveActive = false;
    private float waveStartTime = 0f;
    private Vector2[] waveDirections = new Vector2[]
    {
        new Vector2(1, 0),   // 右方向
        new Vector2(0, 1),   // 上方向
        new Vector2(-1, 0),  // 左方向
        new Vector2(0, -1)   // 下方向
    };
    private int currentDirectionIndex = 0;

    // Mesh
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    void Start()
    {
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

        // Make sure we have a MeshRenderer
        if (!GetComponent<MeshRenderer>())
        {
            gameObject.AddComponent<MeshRenderer>();
        }
    }

    private Vector3[] GenerateVerts()
    {
        var verts = new Vector3[(Dimension + 1) * (Dimension + 1)];

        // Equally distributed verts
        for (int x = 0; x <= Dimension; x++)
            for (int z = 0; z <= Dimension; z++)
                verts[index(x, z)] = new Vector3(x, 0, z);

        return verts;
    }

    private int[] GenerateTries()
    {
        var tries = new int[Mesh.vertices.Length * 6];

        // Two triangles are one tile
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
        // スペースキーが押されたら波を発生させる
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerWave();
        }

        UpdateMesh();
    }

    private void TriggerWave()
    {
        // 波の方向を切り替え
        currentDirectionIndex = (currentDirectionIndex + 1) % waveDirections.Length;
        currentWaveDirection = waveDirections[currentDirectionIndex];

        isWaveActive = true;
        waveStartTime = Time.time;

        Debug.Log("Directional wave triggered: " + currentWaveDirection);
    }

    private void UpdateMesh()
    {
        var verts = Mesh.vertices;

        // デフォルトでは平坦なメッシュ（高さ0）
        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                float y = 0f;

                // スペースキーで発生した方向性のある波を表現
                if (isWaveActive)
                {
                    float elapsedTime = Time.time - waveStartTime;

                    // 波の持続時間をチェック
                    if (elapsedTime <= WaveDuration)
                    {
                        // 波の進行方向に対する位置計算
                        float wavePosition = Vector2.Dot(
                            new Vector2(x, z),
                            currentWaveDirection.normalized) - elapsedTime * WaveSpeed;

                        // 方向に垂直な位置を計算（波の幅制限用）
                        float perpendicularPosition = Vector2.Dot(
                            new Vector2(x, z),
                            new Vector2(-currentWaveDirection.y, currentWaveDirection.x).normalized);

                        // 波形の計算（サイン波）
                        float waveValue = Mathf.Sin(wavePosition * 2f) * WaveHeight;

                        // 波の端の減衰（ガウス曲線で幅を制限）
                        float widthAttenuation = Mathf.Exp(-(perpendicularPosition * perpendicularPosition) / (2f * WaveWidth * WaveWidth));

                        // 時間経過による減衰（波の持続時間に応じて徐々に減衰）
                        float timeAttenuation = 1.0f - (elapsedTime / WaveDuration);

                        // 最終的な波の高さを計算
                        y = waveValue * widthAttenuation * timeAttenuation;
                    }
                    else
                    {
                        // 持続時間が過ぎたら波を無効化
                        isWaveActive = false;
                    }
                }

                verts[index(x, z)] = new Vector3(x, y, z);
            }
        }

        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }
}