using UnityEngine;

public static class WaveCalculator
{
    /// <summary>
    /// ローカル座標(x, z)における波の高さ(ローカルY)を返す
    /// </summary>
    public static float CalcLocalHeight(
        float x, float z,
        int dimension,
        float baseAmplitude,
        float baseFrequency,
        float baseSpeed,
        float noiseScale,
        float noiseSpeed,
        float noiseAmplitude,
        Vector2 noiseDirection,
        float amplitudeMultiplier,
        float fixedTime,
        bool scaleZSmallToLarge)
    {
        float zScale = 0f;
        if (dimension > 0)
            zScale = scaleZSmallToLarge
                ? 1.0f - Mathf.Clamp01(z / dimension)
                : Mathf.Clamp01(z / dimension);

        float timeOffset      = fixedTime * baseSpeed;
        float noiseTimeOffset = fixedTime * noiseSpeed;

        float baseWave = baseAmplitude * amplitudeMultiplier
            * Mathf.Sin((x * baseFrequency + timeOffset) * Mathf.PI / dimension);

        float noiseX = (x + noiseTimeOffset * noiseDirection.x) * noiseScale / dimension + 0.5f;
        float noiseZ = (z + noiseTimeOffset * noiseDirection.y) * noiseScale / dimension + 0.5f;
        float noise  = (Mathf.PerlinNoise(noiseX, noiseZ) * 2f - 1f)
            * noiseAmplitude * amplitudeMultiplier;

        return baseWave + noise;
    }
}
