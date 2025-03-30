using UnityEngine;

public class WindParticleController : MonoBehaviour
{
    public WindMove windMove;
    public ParticleSystem windParticle;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    void Start()
    {
        windParticle = GetComponent<ParticleSystem>();
        velocityModule = windParticle.velocityOverLifetime;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (windMove == null) return;

        // 風の方向を取得
        Vector3 windDirection = windMove.GetWindDirection();
        float windSpeed = windMove.GetWindSpeed();

        // パーティクルの速度を設定
        velocityModule.x = windDirection.x * windSpeed;
        velocityModule.y = windDirection.y * windSpeed;
        velocityModule.z = windDirection.z * windSpeed;
    }
}
