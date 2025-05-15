using Unity.VisualScripting;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(MeshFilter))]
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

    [Header("Audio Settings")]
    public AudioClip waveSound;
    public Transform cubeTransform;

    private AudioSource audioSource; 
    private float startTime;
    private float currentOverallAmplitudeMultiplier = 1f;
    protected MeshFilter MeshFilter;
    protected Mesh Mesh;

    private float minWaveY = float.MaxValue;
    private float maxWaveY = float.MinValue;

    void Start()
    {
        if (difficultySettings == null)
        {
            Debug.LogWarning("DifficultyDataSO is not assigned to the Waves script. Default Z-scaling behavior will be used (Z-large means larger waves).");
        }

        startTime = Time.time;

        // ���b�V���̏�����
        Mesh = new Mesh();
        Mesh.name = gameObject.name;

        Mesh.vertices = GenerateVerts();
        Mesh.triangles = GenerateTries();
        Mesh.uv = GenerateUVs();
        Mesh.RecalculateNormals();
        Mesh.RecalculateBounds();

        MeshFilter = GetComponent<MeshFilter>();
        MeshFilter.mesh = Mesh;

        // AudioSource�̃Z�b�g�A�b�v
        audioSource = GetComponent<AudioSource>();
        if (waveSound != null)
        {
            audioSource.clip = waveSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Wave sound (AudioClip) is not assigned in the inspector. Audio will not play.");
        }

        if (cubeTransform == null)
        {
            GameObject cubeObject = GameObject.Find("Cube");
            if (cubeObject != null)
            {
                cubeTransform = cubeObject.transform;
            }
            else
            {
                Debug.LogError("GameObject with name 'Cube' not found. Audio volume adjustment based on Cube's position will not work.");
            }
        }
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
                verts[index(x, z)] = new Vector3(x, 0, z); // Y���W��UpdateMesh�Őݒ�
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
        var uvs = new Vector2[Mesh.vertices.Length]; // Mesh.vertices�����������ꂽ��ɌĂяo��
        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                // Dimension��0�̏ꍇ�̃[�����Z������� (UV���s���ɂȂ�\���͂��邪�N���b�V���͖h��)
                float u = (Dimension == 0) ? 0 : (float)x / Dimension * UVScale;
                float v = (Dimension == 0) ? 0 : (float)z / Dimension * UVScale;
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
        UpdateMeshAndTrackWaveHeight();
        AdjustAudioVolumeBasedOnCubePosition();
    }
    // ���b�V���̒��_���X�V���A�����ɔg�̍ŏ�/�ő卂�����v�Z����
    private void UpdateMeshAndTrackWaveHeight()
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

        // ���̃t���[���̔g�̍ŏ��l�ƍő�l�����Z�b�g
        minWaveY = float.MaxValue;
        maxWaveY = float.MinValue;

        for (int x = 0; x <= Dimension; x++)
        {
            for (int z = 0; z <= Dimension; z++)
            {
                float zBasedAmplitudeScale = 0f;
                if (Dimension > 0)
                {
                    if (scaleZSmallToLarge)
                    {
                        zBasedAmplitudeScale = 1.0f - (z / (float)Dimension);
                    }
                    else
                    {
                        zBasedAmplitudeScale = z / (float)Dimension;
                    }
                }
                else if (Dimension == 0 && z == 0) // Dimension��0�̏ꍇ�̓��ꏈ��
                {
                    zBasedAmplitudeScale = scaleZSmallToLarge ? 1.0f : 0.0f;
                }

                float currentBaseAmplitude = baseAmplitude * zBasedAmplitudeScale * currentOverallAmplitudeMultiplier;
                float currentNoiseAmplitude = noiseAmplitude * zBasedAmplitudeScale * currentOverallAmplitudeMultiplier;

                float baseWave = 0f;
                float noise = 0f;

                // Dimension��0�̏ꍇ�̃[�����Z�������
                if (Dimension > 0)
                {
                    baseWave = currentBaseAmplitude * Mathf.Sin((x * baseFrequency + timeOffset) * Mathf.PI / Dimension);
                    float noiseXVal = (x + noiseTimeOffset * noiseDirection.x) * noiseScale / Dimension;
                    float noiseZVal = (z + noiseTimeOffset * noiseDirection.y) * noiseScale / Dimension;
                    noise = (Mathf.PerlinNoise(noiseXVal, noiseZVal) * 2f - 1f) * currentNoiseAmplitude;
                }
                else // Dimension == 0 �̏ꍇ
                {
                    baseWave = currentBaseAmplitude * Mathf.Sin(timeOffset * baseFrequency);
                    noise = 0f;
                }


                Vector3 originalVertex = new Vector3(x, 0, z); 
                float currentVertexY = baseWave + noise;
                verts[index(x, z)] = new Vector3(originalVertex.x, currentVertexY, originalVertex.z);

                if (currentVertexY < minWaveY)
                {
                    minWaveY = currentVertexY;
                }
                if (currentVertexY > maxWaveY)
                {
                    maxWaveY = currentVertexY;
                }
            }
        }

        Mesh.vertices = verts;
        Mesh.RecalculateNormals();
    }

    // Cube�̈ʒu�Ɋ�Â��ăI�[�f�B�I�̉��ʂ𒲐�����
    private void AdjustAudioVolumeBasedOnCubePosition()
    {
        // AudioSource, AudioClip, �܂��� CubeTransform �����p�ł��Ȃ��ꍇ�͉������Ȃ�
        if (audioSource == null || waveSound == null || !audioSource.isPlaying || cubeTransform == null)
        {
            return;
        }
        float cubeLocalY = transform.InverseTransformPoint(cubeTransform.position).y;

        float normalizedHeight;

        // �g�̍����͈̔� (maxWaveY - minWaveY) ���L���� (�[�����Z������邽��)
        if (maxWaveY > minWaveY)
        {
            // Cube�̃��[�J��Y���W���A�g�̃��[�J��Y���W�͈̔� [minWaveY, maxWaveY] �Ő��K��
            normalizedHeight = (cubeLocalY - minWaveY) / (maxWaveY - minWaveY);
        }
        else
        {
            // �g�����R�ȏꍇ (minWaveY == maxWaveY)�A�܂��͔͈͂������ȏꍇ���K�����ꂽ������0.5 (����) �Ƃ���
            // ����ɂ�艹�ʂ�1.0�{ (0.5��1.5�̒���) �ɂȂ�
            normalizedHeight = 0.5f;
        }

        // ���K�����ꂽ�l��0����1�͈̔͂ɃN�����v
        normalizedHeight = Mathf.Clamp01(normalizedHeight);

        // ���ʂ�0.5�{ (�ŏ���) ����1.5�{ (�ő厞) �͈̔͂Ő��`��Ԃ��Đݒ�
        float targetVolume = Mathf.Lerp(0f, 1.0f, normalizedHeight);
        audioSource.volume = targetVolume;
        Debug.Log(normalizedHeight);
    }
}