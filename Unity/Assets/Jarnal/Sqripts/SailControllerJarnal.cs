using UnityEngine;

public class SailControllerJarnal : MonoBehaviour
{
    public WeightControllerJarnal weightController;
    public Transform sailTransform;

    [SerializeField] private float maxSailAngle = 45f;
    [SerializeField] private float maxSensorValue = 6000f;
    [SerializeField] private float rotationSpeed = 5f;

    private float currentAngle = 0f;

    private void Update()
    {
        float targetAngle = 0f;

        if (sailTransform != null && weightController != null)
        {
            float sensorValue = weightController.GetLatestValue();
            float normalized = Mathf.Clamp(sensorValue / maxSensorValue, -1, 1);
            targetAngle = normalized * maxSailAngle;
        }

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);
        sailTransform.localRotation = Quaternion.Euler(-90f, 0f, currentAngle);
    }
}
