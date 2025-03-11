using UnityEngine;
using System.Collections;

public class WaveCreate : MonoBehaviour
{
    private Waves waves;
    public float bigWaveHeight = 5f;      // �g�̍���
    public float waveSpeed = 2f;          // �g�̐i�s���x
    public float waveWidth = 3f;          // �g�̕�
    public float wavePersistence = 0.5f;   // �g�̎�������
    private bool canCreateWave = true;
    private int waveCount = 0;

    [Header("Wave Shape")]
    public float leadingEdgesteepness = 1.5f;  // �g�̑O�ʂ̋}�s��
    public float trailingEdgeSteepness = 1f;   // �g�̌�ʂ̋}�s��

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

        float wavePosition = -waveWidth * 2; // ��ʊO����J�n
        float maxPosition = waves.Dimension + waveWidth * 2; // ��ʊO�܂ő�����

        while (wavePosition < maxPosition)
        {
            // ���̒��_��ۑ�
            var verts = waves.GetComponent<MeshFilter>().mesh.vertices;
            var originalVerts = new Vector3[verts.Length];
            System.Array.Copy(verts, originalVerts, verts.Length);

            for (int x = 0; x <= waves.Dimension; x++)
            {
                for (int z = 0; z <= waves.Dimension; z++)
                {
                    int vertexIndex = x * (waves.Dimension + 1) + z;

                    // �g����̋������v�Z
                    float distanceFromWave = z - wavePosition;

                    // ��Ώ̂Ȕg�`���쐬
                    float waveInfluence = 0;
                    if (distanceFromWave < 0)
                    {
                        // �g�̑O�ʁi���}�s�j
                        waveInfluence = Mathf.Exp(-Mathf.Pow(distanceFromWave, 2) / (2 * waveWidth * leadingEdgesteepness));
                    }
                    else
                    {
                        // �g�̌�ʁi���Ȃ��炩�j
                        waveInfluence = Mathf.Exp(-Mathf.Pow(distanceFromWave, 2) / (2 * waveWidth * trailingEdgeSteepness));
                    }

                    // �����̔g�̍����ɐV�����g��ǉ�
                    verts[vertexIndex].y = originalVerts[vertexIndex].y + waveInfluence * bigWaveHeight;
                }
            }

            // ���b�V�����X�V
            waves.GetComponent<MeshFilter>().mesh.vertices = verts;
            waves.GetComponent<MeshFilter>().mesh.RecalculateNormals();

            // �g��i�߂�
            wavePosition += waveSpeed * Time.deltaTime;

            yield return null;
        }

        Debug.Log($"Wave #{waveCount} completed its travel");
        yield return new WaitForSeconds(wavePersistence);
        canCreateWave = true;
    }

    // �g�̍������擾���郆�[�e�B���e�B���\�b�h
    private float GetWaveHeight(float distance, float width, float steepness)
    {
        return Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * width * steepness));
    }
}