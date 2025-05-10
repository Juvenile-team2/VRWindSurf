using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private ParticleSystem goalParticle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("�S�[���ɓ��B���܂����I");

            if (goalParticle != null)
            {
                // �p�[�e�B�N���̐ݒ���m�F�E����
                var emission = goalParticle.emission;
                emission.enabled = true; // Emission��L����
                emission.rateOverTime = 100; // 1�b�Ԃɐ�������p�[�e�B�N����

                var main = goalParticle.main;
                main.duration = 2f; // �p�[�e�B�N���̌p������
                main.startLifetime = 2f; // �e�p�[�e�B�N���̎���
                main.startSize = 1f; // �p�[�e�B�N���̃T�C�Y
                main.startSpeed = 5f; // �p�[�e�B�N���̑��x

                // �p�[�e�B�N�����Đ�
                goalParticle.transform.position = transform.position;
                goalParticle.Clear(); // �����̃p�[�e�B�N�����N���A
                goalParticle.Play();

                Debug.Log("�p�[�e�B�N���ݒ��:");
                Debug.Log("- Emission enabled: " + emission.enabled);
                Debug.Log("- Rate over time: " + emission.rateOverTime.constant);
                Debug.Log("- Start size: " + main.startSize.constant);
            }
        }
    }
}