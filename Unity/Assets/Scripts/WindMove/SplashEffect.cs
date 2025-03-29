using UnityEngine;

public class SplashEffect : MonoBehaviour
{
    public ParticleSystem splashParticle;  // 水しぶきのパーティクル
    public Rigidbody BoardRigidbody;

    private float minSpeedThreshold = 1.0f; // 水しぶきを発生させる最小速度

    void Update()
    {
        // 速度をチェック
        float BoardSpeed = BoardRigidbody.velocity.magnitude;

        // 速度がしきい値を超えたらパーティクルを有効にする
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

