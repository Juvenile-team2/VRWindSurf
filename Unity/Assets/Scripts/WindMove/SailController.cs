using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // ��]���x
    private float currentRotation = 90f; // ���݂̔��̉�]�p�x

    public GameObject pivot;

    void Update()
    {
        // ���͂��󂯂Ĕ������E�ɓ�����
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // ��]������ݒ�
        currentRotation = Mathf.Clamp(currentRotation, 10f, 170f);

        // ������]������
        transform.localRotation = Quaternion.Euler(0f, currentRotation, 0f);

/*        if (Input.GetKey(KeyCode.LeftArrow)){
            transform.RotateAround(pivot.transform.position, Vector3.up, 20 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(pivot.transform.position, Vector3.up, -20 * Time.deltaTime);
        }*/
    }
}


