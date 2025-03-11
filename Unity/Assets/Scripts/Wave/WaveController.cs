using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour
{
    private Waves waves;
    public float bigWaveHeight = 5f;
    public float bigWaveRadius = 10f;
    public float waveCooldown = 2f;
    private bool canCreateWave = true;

    // �g�̃J�E���g��ǉ�
    private int waveCount = 0;

    [System.Obsolete]
    void Start()
    {
        waves = GetComponent<Waves>();
        if (waves == null)
        {
            waves = FindObjectOfType<Waves>();
        }
        Debug.Log("WaveController initialized - Ready to create waves!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canCreateWave)
        {
            Vector2 waveCenter = new Vector2(waves.Dimension / 2f, waves.Dimension / 2f);
            StartCoroutine(CreateWaveWithCooldown(waveCenter));
        }
    }

    private IEnumerator CreateWaveWithCooldown(Vector2 center)
    {
        canCreateWave = false;
        waveCount++;

        // �g�̐������̃��O�o��
        Debug.Log($"Wave #{waveCount} created!");
        Debug.Log($"Wave Height: {bigWaveHeight} units");
        Debug.Log($"Wave Position: X={center.x}, Y={center.y}");

        float[] originalHeights = new float[waves.Octaves.Length];
        for (int i = 0; i < waves.Octaves.Length; i++)
        {
            originalHeights[i] = waves.Octaves[i].height;
        }

        // �g�̍�����ύX���̃��O
        Debug.Log($"Modifying wave heights - Original heights:");
        for (int i = 0; i < waves.Octaves.Length; i++)
        {
            Debug.Log($"Octave {i}: {waves.Octaves[i].height} -> {bigWaveHeight}");
            var octave = waves.Octaves[i];
            octave.height = bigWaveHeight;
            waves.Octaves[i] = octave;
        }

        yield return new WaitForSeconds(0.5f);

        float lerpTime = 0f;
        float duration = 1f;

        while (lerpTime < duration)
        {
            lerpTime += Time.deltaTime;
            float t = lerpTime / duration;

            for (int i = 0; i < waves.Octaves.Length; i++)
            {
                var octave = waves.Octaves[i];
                octave.height = Mathf.Lerp(bigWaveHeight, originalHeights[i], t);
                waves.Octaves[i] = octave;
            }

            yield return null;
        }

        // �g�����܂������̃��O
        Debug.Log($"Wave #{waveCount} settled back to normal");

        yield return new WaitForSeconds(waveCooldown);

        canCreateWave = true;
        Debug.Log("Ready for next wave!");
    }

    // �f�o�b�O�p�̌��݂̔g�̍������擾���郁�\�b�h
    private float GetCurrentMaxWaveHeight()
    {
        float maxHeight = 0f;
        foreach (var octave in waves.Octaves)
        {
            if (octave.height > maxHeight)
                maxHeight = octave.height;
        }
        return maxHeight;
    }

    // �g�̏�Ԃ��R���\�[���ɏo�͂��郁�\�b�h
    public void PrintWaveStatus()
    {
        Debug.Log($"Current Wave Status:");
        Debug.Log($"Total Waves Created: {waveCount}");
        Debug.Log($"Can Create New Wave: {canCreateWave}");
        Debug.Log($"Current Max Wave Height: {GetCurrentMaxWaveHeight()}");
    }
}