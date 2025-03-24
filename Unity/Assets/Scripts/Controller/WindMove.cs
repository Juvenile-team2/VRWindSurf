using UnityEngine;

public class WindMove : MonoBehaviour
{
    // ���̐���
    [SerializeField]
    private float windX = 0f;
    [SerializeField]
    private float windY = 0f;
    [SerializeField]
    private float windZ = 0f;

    // �ő�g�͌W���ƍR�͌W��
    private float maxLiftCoefficient = 1.2f;
    private float baseDragCoefficient = 0.05f;
    private float dragIncreaseRate = 0.1f;

    private float airDensity = 1.225f;

    private void OnTriggerStay(Collider other)
    {
        // ���iSail�j�ɂԂ������ꍇ
        if (other.CompareTag("Sail"))
        {
            // �e�I�u�W�F�N�g�i�D�j��Rigidbody���擾
            Rigidbody parentRigidbody = other.transform.root.GetComponent<Rigidbody>();

            // ����Transform���擾
            Transform sailTransform = other.transform;

            if (parentRigidbody != null && sailTransform != null)
            {
                // ���̋����ƌ������v�Z
                float windSpeed = CalculateWindSpeed(windX, windY, windZ);
                Vector3 windDirection = CalculateWindDirection(windX, windY, windZ);

                // ���̌������擾
                Vector3 sailDirection = sailTransform.forward;

                // �p�i���Ɣ��̊p�x�j���v�Z
                float angleOfAttack = CalculateAngleOfAttack(windDirection, sailDirection);

                // �g�͌W���ƍR�͌W�����v�Z
                float liftCoefficient = CalculateLiftCoefficient(angleOfAttack);
                //float dragCoefficient = CalculateDragCoefficient(liftCoefficient);

                // �g�͂ƍR�͂��v�Z
                Vector3 liftForce = CalculateLiftForce(windSpeed, windDirection, sailDirection, parentRigidbody, liftCoefficient);
                //Vector3 dragForce = CalculateDragForce(windSpeed, windDirection, parentRigidbody, dragCoefficient);

                // ���i�͂��v�Z
                Vector3 thrustForce = CalculateThrustForce(liftForce, angleOfAttack);

                // �e�I�u�W�F�N�g�i�D�j�ɗ͂�K�p
                parentRigidbody.AddForce(thrustForce, ForceMode.Acceleration);
            }
        }
    }

    // ���̋������v�Z
    float CalculateWindSpeed(float windx, float windy, float windz)
    {
        return Mathf.Sqrt(windx * windx + windy * windy + windz * windz);
    }

    // ���̌������v�Z
    Vector3 CalculateWindDirection(float windx, float windy, float windz)
    {
        float windSpeed = CalculateWindSpeed(windx, windy, windz);
        if (windSpeed == 0) return Vector3.zero;
        return new Vector3(windx / windSpeed, windy / windSpeed, windz / windSpeed);
    }

    // �p���v�Z�i���Ɣ��̊p�x�j
    float CalculateAngleOfAttack(Vector3 windDirection, Vector3 sailDirection)
    {
        return Vector3.Angle(windDirection, sailDirection) * Mathf.Deg2Rad;
    }

    // �g�͌W�����v�Z
    float CalculateLiftCoefficient(float angleOfAttack)
    {
        return maxLiftCoefficient * Mathf.Sin(2 * angleOfAttack);
    }

    // �R�͌W�����v�Z
    float CalculateDragCoefficient(float liftCoefficient)
    {
        return baseDragCoefficient + dragIncreaseRate * liftCoefficient * liftCoefficient;
    }

    // �g�͂��v�Z
    Vector3 CalculateLiftForce(float windSpeed, Vector3 windDirection, Vector3 sailDirection, Rigidbody rigidbody, float liftCoefficient)
    {
        
        Vector3 liftDirection = Vector3.Cross(windDirection, sailDirection).normalized; // ���Ɣ��̊O�ςŗg�͂̕���������

        // Y�����̗g�͂𖳎��iXZ���ʂɌ���j
        liftDirection.y = 0;
        liftDirection = liftDirection.normalized;

        // �g�͂̑傫�����v�Z
        float liftForceMagnitude = 0.5f * windSpeed * windSpeed * liftCoefficient * airDensity * rigidbody.mass;

        // �g�̓x�N�g����Ԃ�
        return liftForceMagnitude * liftDirection;
    }

    // �R�͂��v�Z
    Vector3 CalculateDragForce(float windSpeed, Vector3 windDirection, Rigidbody rigidbody, float dragCoefficient)
    {
        float dragForceMagnitude = 0.5f * windSpeed * windSpeed * dragCoefficient * airDensity * rigidbody.mass;
        Vector3 dragForce = dragForceMagnitude * windDirection; // ���Ɠ�������

        // Y�����̍R�͂𖳎��iXZ���ʂɌ���j
        dragForce.y = 0;

        return dragForce;
    }

    // ���i�͂��v�Z
    Vector3 CalculateThrustForce(Vector3 lift, float angleOfAttack)
    {

        Vector3 thrustForce = lift * Mathf.Sin(angleOfAttack);

        return thrustForce;
    }
}
