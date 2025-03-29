using UnityEngine;

public class SplashEffect : MonoBehaviour
{
    public ParticleSystem splashParticle;  // �����Ԃ��̃p�[�e�B�N��
    public Rigidbody BoardRigidbody;

    private float minSpeedThreshold = 1.0f; // �����Ԃ��𔭐�������ŏ����x

    void Update()
    {
        // ���x���`�F�b�N
        float BoardSpeed = BoardRigidbody.linearVelocity.magnitude;

        // ���x���������l�𒴂�����p�[�e�B�N����L���ɂ���
        if (BoardSpeed > minSpeedThreshold)
        {
            if (!splashParticle.isPlaying)
            {
                splashParticle.Play();
            }
        }
        else
        {
            if (splashParticle.isPlaying)
            {
                splashParticle.Stop();
            }
        }
    }
}

