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
    [SerializeField] private float rotationSpeed = 5f; // �A�j���[�V�����̑����𐧌�

    private float currentAngle = 0f; // ���݂̊p�x��ێ�

    private void Update()
    {
        float targetAngle = 0f; // �ڕW�Ƃ���p�x

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
                    // �E���y���������d�����������ɌX����i���̊p�x�j
                    targetAngle = -Mathf.Clamp01(diff / maxSensorValue) * maxSailAngle;
                }
                else if (sensorValueR > sensorValueL)
                {
                    // �����y�����E���d���������E�ɌX����i���̊p�x�j
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

        // ���݂̊p�x����ڕW�̊p�x�֊��炩�ɕ��
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // ��Ԃ��ꂽ�p�x��K�p
        sailTransform.localRotation = Quaternion.Euler(-90f, 0f, currentAngle);
    }
}