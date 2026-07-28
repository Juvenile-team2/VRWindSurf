using UnityEngine;

public class WavesFormulaHeightJarnal : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int Dimension = 250;

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

    // 波の座標基準となるTransform。未設定ならワールド座標をそのまま使う
    [Tooltip("波メッシュのTransform。未設定時はワールド座標基準で計算する。")]
    public Transform waveOrigin;

    private float startTime;
    private float startFixedTime;
    private float currentOverallAmplitudeMultiplier = 1f;

    void Start()
    {
        startTime = Time.time;
        startFixedTime = Time.fixedTime;
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        currentOverallAmplitudeMultiplier = Mathf.Lerp(1f, maxAmplitudeMultiplier,
            Mathf.Clamp01(growthDuration > 0f ? elapsed / growthDuration : 1f));
    }

    public float GetHeight(Vector3 position)
    {
        bool scaleZSmallToLarge = difficultySettings != null && difficultySettings.rotationFlag;
        float elapsedFixedTime = Time.fixedTime - startFixedTime;

        float x, z, worldY;
        if (waveOrigin != null)
        {
            Vector3 localPos = waveOrigin.InverseTransformPoint(position);
            x = localPos.x;
            z = localPos.z;
            float localY = WaveCalculator.CalcLocalHeight(
                x, z, Dimension,
                baseAmplitude, baseFrequency, baseSpeed,
                noiseScale, noiseSpeed, noiseAmplitude, noiseDirection,
                currentOverallAmplitudeMultiplier, elapsedFixedTime, scaleZSmallToLarge);
            worldY = waveOrigin.TransformPoint(new Vector3(0, localY, 0)).y;
        }
        else
        {
            // waveOrigin 未設定: ワールドXZをそのまま使い、Yをワールド高さとして返す
            x = position.x;
            z = position.z;
            worldY = WaveCalculator.CalcLocalHeight(
                x, z, Dimension,
                baseAmplitude, baseFrequency, baseSpeed,
                noiseScale, noiseSpeed, noiseAmplitude, noiseDirection,
                currentOverallAmplitudeMultiplier, elapsedFixedTime, scaleZSmallToLarge);
        }

        return worldY;
    }
}
