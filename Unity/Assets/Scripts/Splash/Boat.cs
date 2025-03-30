using UnityEngine;

public class Boat : MonoBehaviour
{
    public Waves waterMesh;              // �g�̃X�N���v�g�ւ̎Q��
    public ParticleSystem splashEffect;  // �����Ԃ��̃p�[�e�B�N���V�X�e��

    private float previousHeight;        // �O�t���[���ł̍���
    private float currentHeight;         // ���݂̍���
    private bool wasDecreasing = false;  // �������������Ă������ǂ���

    [Header("Splash Settings")]
    public float minVelocityForSplash = 0.1f;  // �X�v���b�V������������ŏ����x
    public float splashCooldown = 0.5f;        // �X�v���b�V���̍ŏ��Ԋu�i�b�j
    private float lastSplashTime;              // �Ō�ɃX�v���b�V����������������

    void Start()
    {
        if (waterMesh == null)
        {
            waterMesh = FindObjectOfType<Waves>();
        }

        // �����������擾
        previousHeight = GetWaterHeight();
        currentHeight = previousHeight;
    }

    void Update()
    {
        previousHeight = currentHeight;
        currentHeight = GetWaterHeight();

        // �{�[�g�̈ʒu��g�̍����ɍ��킹��
        Vector3 position = transform.position;
        position.y = currentHeight;
        transform.position = position;

        // �g�̌X���ɍ��킹�ă{�[�g����]
        UpdateBoatRotation();

        // �ɏ��l�̌��o�ƃX�v���b�V���̐���
        CheckForSplash();
    }

    private float GetWaterHeight()
    {
        return waterMesh.GetHeight(transform.position);
    }

    private void UpdateBoatRotation()
    {
        // �g�̖@���Ɋ�Â��ă{�[�g����]������
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // �O��̌X��
        float frontHeight = waterMesh.GetHeight(transform.position + forward);
        float backHeight = waterMesh.GetHeight(transform.position - forward);
        float pitchAngle = Mathf.Atan2(frontHeight - backHeight, 2f) * Mathf.Rad2Deg;

        // ���E�̌X��
        float rightHeight = waterMesh.GetHeight(transform.position + right);
        float leftHeight = waterMesh.GetHeight(transform.position - right);
        float rollAngle = Mathf.Atan2(rightHeight - leftHeight, 2f) * Mathf.Rad2Deg;

        // ��]��K�p
        transform.rotation = Quaternion.Euler(-pitchAngle, transform.eulerAngles.y, -rollAngle);
    }

    private void CheckForSplash()
    {
        float velocity = (currentHeight - previousHeight) / Time.deltaTime;
        bool isDecreasing = velocity < 0;

        // �ɏ��l�̌��o�i���~����㏸�ɕς��u�ԁj
        if (wasDecreasing && !isDecreasing)
        {
            // ���x�`�F�b�N�ƃN�[���_�E���`�F�b�N
            if (Mathf.Abs(velocity) > minVelocityForSplash &&
                Time.time - lastSplashTime > splashCooldown)
            {
                CreateSplash();
                lastSplashTime = Time.time;
            }
        }

        wasDecreasing = isDecreasing;
    }

    private void CreateSplash()
    {
        if (splashEffect != null)
        {
            // �p�[�e�B�N���̈ʒu���{�[�g�̒�ʂɍ��킹��
            Vector3 splashPosition = transform.position;
            splashPosition.y = currentHeight;

            // �p�[�e�B�N�����Đ�
            splashEffect.transform.position = splashPosition;
            splashEffect.Play();
        }
    }
}