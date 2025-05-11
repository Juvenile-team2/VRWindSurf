using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private ParticleSystem goalParticle;
    [SerializeField] private TimeController timeController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("�S�[���ɓ��B���܂����I");

            // TimeController�̃N���A����
            if (timeController != null)
            {
                timeController.SetCleared();
            }
            else
            {
                Debug.LogWarning("TimeController���ݒ肳��Ă��܂���");
            }

            if (goalParticle != null)
            {
                var emission = goalParticle.emission;
                var main = goalParticle.main;

                // �p�[�e�B�N���Đ�
                goalParticle.transform.position = transform.position;
                goalParticle.Clear();
                goalParticle.Play();

                Debug.Log("�p�[�e�B�N���ݒ��:");
                Debug.Log("- Emission enabled: " + emission.enabled);
                Debug.Log("- Rate over time: " + emission.rateOverTime.constant);
                Debug.Log("- Start size: " + main.startSize.constant);
            }
        }
    }
}