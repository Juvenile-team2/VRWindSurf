using UnityEngine;

// Waves の GetHeight() を数式から直接計算するバージョン。
// メッシュ頂点を参照しないので Update/FixedUpdate のタイミングズレが起きない。
// 傾き(TargetUp)は WaterFloat 側の FloatPoints 全点から PhysicsHelper.GetNormal() で計算される。
// 使い方: Wave オブジェクトの Waves コンポーネントをこれに差し替える。
public class WavesFormulaHeight : Waves
{
    public override float GetHeight(Vector3 position)
    {
        Vector3 localPos = transform.InverseTransformPoint(position);
        float x = localPos.x;
        float z = localPos.z;

        float zBasedAmplitudeScale = 0f;
        if (Dimension > 0)
        {
            bool scaleZSmallToLarge = difficultySettings != null && difficultySettings.rotationFlag;
            zBasedAmplitudeScale = scaleZSmallToLarge
                ? 1.0f - Mathf.Clamp01(z / Dimension)
                : Mathf.Clamp01(z / Dimension);
        }

        float timeOffset      = Time.fixedTime * baseSpeed;
        float noiseTimeOffset = Time.fixedTime * noiseSpeed;

        float baseWave = baseAmplitude * zBasedAmplitudeScale * currentOverallAmplitudeMultiplier
            * Mathf.Sin((x * baseFrequency + timeOffset) * Mathf.PI / Dimension);

        float noiseX = (x + noiseTimeOffset * noiseDirection.x) * noiseScale / Dimension + 0.5f;
        float noiseZ = (z + noiseTimeOffset * noiseDirection.y) * noiseScale / Dimension + 0.5f;
        float noise  = (Mathf.PerlinNoise(noiseX, noiseZ) * 2f - 1f)
            * noiseAmplitude * zBasedAmplitudeScale * currentOverallAmplitudeMultiplier;

        return transform.TransformPoint(new Vector3(0, baseWave + noise, 0)).y;
    }
}
