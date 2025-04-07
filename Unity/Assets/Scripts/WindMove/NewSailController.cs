using UnityEngine;

public class NewSailController : MonoBehaviour
{
    public WeightController weightController;
    public Transform sailTransform;
    
    //[SerializeField] private float minSailAngle = -45f;
    [SerializeField] private float maxSailAngle = 45f;

    [SerializeField] private float maxSensorValue = 6000f;



    private void Update()
    {

        if (sailTransform != null && weightController != null)
        {
            float sensorValue = weightController.GetLatestValue(maxSensorValue);

            float normalized = Mathf.Clamp01(sensorValue / maxSensorValue);
            float sailAngle = (normalized - 0.5f) * 2f * maxSailAngle;
            sailTransform.localRotation = Quaternion.Euler(-90f, 0f, sailAngle);
        }
    }
}
