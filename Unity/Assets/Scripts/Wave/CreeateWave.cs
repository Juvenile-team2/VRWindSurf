using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateWave : MonoBehaviour
{
    // Wave Parameters
    public int Dimension = 10;
    public float UVScale = 2f;
    public float WaveSpeed = 5.0f;        // �����g�̑��x
    public float WaveHeight = 1.5f;       // �����g�̍��� 
    public float WaveDuration = 3.0f;     // �g�̎������ԁi�b�j
    public float WaveWidth = 3.0f;        // �g�̕��i�傫���قǕ��L���g�ɂȂ�j

    // �g�̕�������p
    private Vector2 currentWaveDirection = Vector2.right;  // �����l�͉E����
    private bool isWaveActive = false;
    private float waveStartTime = 0f;
    private Vector2[] waveDirections = new Vector2[]
    {
        new Vector2(1, 0),   // �E����
        new Vector2(0, 1),   // �����
        new Vector2(-1, 0),  // ������
        new Vector2(0, -1)   // ������
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
        // �X�y�[�X�L�[�������ꂽ��g�𔭐�������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerWave();
        }

        UpdateMesh();
    }

    private void TriggerWave()
    {
        // �g�̕�����؂�ւ�
        currentDirectionIndex = (currentDirectionIndex + 1) % waveDirections.Length;
        currentWaveDirection = waveDirections[currentDirectionIndex];

        isWaveActive = true;
        waveStartTime = Time.time;

        Debug.Log("Directional wave triggered: " + currentWaveDirection);
    }

    private void UpdateMesh()
    {
        var verts = Mesh.vertices;

        // �f�t�H���g�ł͕��R�ȃ��b�V���i����0�j
        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                float y = 0f;

                // �X�y�[�X�L�[�Ŕ��������������̂���g��\��
                if (isWaveActive)
                {
                    float elapsedTime = Time.time - waveStartTime;

                    // �g�̎������Ԃ��`�F�b�N
                    if (elapsedTime <= WaveDuration)
                    {
                        // �g�̐i�s�����ɑ΂���ʒu�v�Z
                        float wavePosition = Vector2.Dot(
                            new Vector2(x, z),
                            currentWaveDirection.normalized) - elapsedTime * WaveSpeed;

                        // �����ɐ����Ȉʒu���v�Z�i�g�̕������p�j
                        float perpendicularPosition = Vector2.Dot(
                            new Vector2(x, z),
                            new Vector2(-currentWaveDirection.y, currentWaveDirection.x).normalized);

                        // �g�`�̌v�Z�i�T�C���g�j
                        float waveValue = Mathf.Sin(wavePosition * 2f) * WaveHeight;

                        // �g�̒[�̌����i�K�E�X�Ȑ��ŕ��𐧌��j
                        float widthAttenuation = Mathf.Exp(-(perpendicularPosition * perpendicularPosition) / (2f * WaveWidth * WaveWidth));

                        // ���Ԍo�߂ɂ�錸���i�g�̎������Ԃɉ����ď��X�Ɍ����j
                        float timeAttenuation = 1.0f - (elapsedTime / WaveDuration);

                        // �ŏI�I�Ȕg�̍������v�Z
                        y = waveValue * widthAttenuation * timeAttenuation;
                    }
                    else
                    {
                        // �������Ԃ��߂�����g�𖳌���
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