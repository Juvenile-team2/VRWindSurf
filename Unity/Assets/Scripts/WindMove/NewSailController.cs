using UnityEngine;

public class NewSailController : MonoBehaviour
{
    public WeightControllerR weightControllerR;
    public WeightControllerL weightControllerL;

    public WeightController weightController;

    public bool twoWeightController = false;

    public Transform sailTransform;
    
    //[SerializeField] private float minSailAngle = -45f;
    [SerializeField] private float maxSailAngle = 45f;

    [SerializeField] private float maxSensorValue = 6000f;



    private void Update()
    {
        if (twoWeightController)
        {
            if (sailTransform != null && weightControllerR != null && weightControllerL != null)
            {
                float sensorValueR = weightControllerR.GetLatestValue();
                float sensorValueL = weightControllerL.GetLatestValue();

                float minValue = Mathf.Min(sensorValueL, sensorValueR);
                float diff = maxSensorValue - minValue;

                float angle = 0f;
                if (sensorValueL > sensorValueR)
                {
                    // 右が軽い＝左が重い→帆を左に傾ける（負の角度）
                    angle = -Mathf.Clamp01(diff / maxSensorValue) * maxSailAngle;
                }
                else if (sensorValueR > sensorValueL)
                {
                    // 左が軽い＝右が重い→帆を右に傾ける（正の角度）
                    angle = Mathf.Clamp01(diff / maxSensorValue) * maxSailAngle;
                }
                else
                {
                    // 同じならまっすぐ
                    angle = 0f;
                }

                //float normalized = Mathf.Clamp01(sensorValue / maxSensorValue);
                //float sailAngle = (normalized - 0.5f) * 2f * maxSailAngle;

                sailTransform.localRotation = Quaternion.Euler(-90f, 0f, angle);
            }
        }
        else
        {
            if(sailTransform != null && weightControllerR != null)
            {
                float sensorValue = weightController.GetLatestValue();

                float normalized = Mathf.Clamp01(sensorValue / maxSensorValue);
                float angle = (normalized - 0.5f) * 2f * maxSailAngle;

                sailTransform.localRotation = Quaternion.Euler(-90f, 0f, angle);
            }
        }
    }
}
