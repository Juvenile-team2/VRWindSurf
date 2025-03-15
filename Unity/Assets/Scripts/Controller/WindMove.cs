using UnityEngine;
public class WindMove : MonoBehaviour
{
    // x�������ɉ����镗�̗�
    [SerializeField]
    private float windX = 0f;
    // y�������ɉ����镗�̗�
    [SerializeField]
    private float windY = 0f;
    // z�������ɉ����镗�̗�
    [SerializeField]
    private float windZ = 0f;
    /// <summary>
    /// Is Trigger�Ƀ`�F�b�N���������R���C�_�[�͈͓̔��ɓ����Ă���ԂɌJ��Ԃ����s�����֐�
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        // �������������rigidbody�R���|�[�l���g���擾
        Rigidbody otherRigidbody = other.gameObject.GetComponent<Rigidbody>();
        // otherRigidbody��null�ł͂Ȃ��ꍇ�i�����GameObject��rigidbody�����Ă���ꍇ�j
        if (otherRigidbody != null)
        {
            // �����rigidbody�ɗ͂�������
            otherRigidbody.AddForce(windX, windY, windZ, ForceMode.Acceleration);
        }
    }
}