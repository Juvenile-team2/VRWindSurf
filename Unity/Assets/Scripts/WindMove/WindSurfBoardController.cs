using UnityEngine;

public class WindSurfBoardController : MonoBehaviour
{
    public Rigidbody rb;
    public Transform sail; // ����Transform
    public float movementFactor = 5f; // ���Ɋ�Â��ړ��W��

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // ���̉�]�p�x���擾���āA�T�[�t�{�[�h�̓����ɉe����^����
        float sailAngle = sail.localRotation.eulerAngles.y;
        Vector3 forwardDirection = Quaternion.Euler(0, sailAngle, 0) * Vector3.forward;

        // �T�[�t�{�[�h�̌����𔿂̊p�x�ɉ����Ē���
        rb.AddForce(forwardDirection * movementFactor * Time.deltaTime);
    }
}
