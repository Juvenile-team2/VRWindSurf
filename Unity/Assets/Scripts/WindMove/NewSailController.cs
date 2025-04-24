using UnityEngine;

public class NewSailController : MonoBehaviour
{
    public WeightControllerR weightControllerR;
    public WeightControllerL weightControllerL;
    public WeightController weightController;
    public bool twoWeightController = false;
    public Transform sailTransform;

    [SerializeField] private float maxSailAngle = 45f;
    [SerializeField] private float maxSensorValue = 6000f;
    [SerializeField] private float rotationSpeed = 5f; // アニメーションの速さを制御

    private float currentAngle = 0f; // 現在の角度を保持

    private void Update()
    {
        float targetAngle = 0f; // 目標とする角度

        if (twoWeightController)
        {
            if (sailTransform != null && weightControllerR != null && weightControllerL != null)
            {
                float sensorValueR = weightControllerR.GetLatestValue();
                float sensorValueL = weightControllerL.GetLatestValue();

                float minValue = Mathf.Min(sensorValueL, sensorValueR);
                float diff = maxSensorValue - minValue;

                if (sensorValueL > sensorValueR)
                {
                    // 右が軽い＝左が重い→帆を左に傾ける（負の角度）
                    targetAngle = -Mathf.Clamp01(diff / maxSensorValue) * maxSailAngle;
                }
                else if (sensorValueR > sensorValueL)
                {
                    // 左が軽い＝右が重い→帆を右に傾ける（正の角度）
                    targetAngle = Mathf.Clamp01(diff / maxSensorValue) * maxSailAngle;
                }
            }
        }
        else
        {
            if (sailTransform != null && weightController != null)
            {
                float sensorValue = weightController.GetLatestValue();
                float normalized = Mathf.Clamp(sensorValue / maxSensorValue, -1, 1);
                targetAngle = normalized * maxSailAngle;
            }
        }

        // 現在の角度から目標の角度へ滑らかに補間
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // 補間された角度を適用
        sailTransform.localRotation = Quaternion.Euler(-90f, 0f, currentAngle);
    }
}