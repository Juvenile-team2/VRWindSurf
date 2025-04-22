using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // ��]���x
    [SerializeField] private float currentRotation = 0f; // ���݂̔��̉�]�p�x

    public GameObject pivot;

    void Update()
    {
        // ���͂��󂯂Ĕ������E�ɓ�����
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // ��]������ݒ�
        currentRotation = Mathf.Clamp(currentRotation, -80f, 80f);
        //currentRotation = Mathf.Clamp(currentRotation, 10f, 170f);

        // ������]������
        transform.localRotation = Quaternion.Euler(-90f,0f,currentRotation);

/*        if (Input.GetKey(KeyCode.LeftArrow)){
            transform.RotateAround(pivot.transform.position, Vector3.up, 20 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(pivot.transform.position, Vector3.up, -20 * Time.deltaTime);
        }*/
    }
}


