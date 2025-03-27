using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // ��]���x
    private float currentRotation = 0f; // ���݂̔��̉�]�p�x

    void Update()
    {
        // ���͂��󂯂Ĕ������E�ɓ�����
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // ��]������ݒ�
        currentRotation = Mathf.Clamp(currentRotation, -80f, 80f);

        // ������]������
        transform.localRotation = Quaternion.Euler(0f, currentRotation, 0f);
    }
}


