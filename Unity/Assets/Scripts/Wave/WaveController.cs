using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour
{
    private Waves waves;
    public float bigWaveHeight = 5f;
    public float bigWaveRadius = 10f;
    public float waveCooldown = 2f;
    private bool canCreateWave = true;

    // 波のカウントを追加
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

        // 波の生成時のログ出力
        Debug.Log($"Wave #{waveCount} created!");
        Debug.Log($"Wave Height: {bigWaveHeight} units");
        Debug.Log($"Wave Position: X={center.x}, Y={center.y}");

        float[] originalHeights = new float[waves.Octaves.Length];
        for (int i = 0; i < waves.Octaves.Length; i++)
        {
            originalHeights[i] = waves.Octaves[i].height;
        }

        // 波の高さを変更時のログ
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

        // 波が収まった時のログ
        Debug.Log($"Wave #{waveCount} settled back to normal");

        yield return new WaitForSeconds(waveCooldown);

        canCreateWave = true;
        Debug.Log("Ready for next wave!");
    }

    // デバッグ用の現在の波の高さを取得するメソッド
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

    // 波の状態をコンソールに出力するメソッド
    public void PrintWaveStatus()
    {
        Debug.Log($"Current Wave Status:");
        Debug.Log($"Total Waves Created: {waveCount}");
        Debug.Log($"Can Create New Wave: {canCreateWave}");
        Debug.Log($"Current Max Wave Height: {GetCurrentMaxWaveHeight()}");
    }
}