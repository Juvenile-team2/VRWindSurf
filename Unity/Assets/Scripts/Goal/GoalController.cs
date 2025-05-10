using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private ParticleSystem goalParticle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゴールに到達しました！");

            if (goalParticle != null)
            {
                // パーティクルの設定を確認・調整
                var emission = goalParticle.emission;
                emission.enabled = true; // Emissionを有効化
                emission.rateOverTime = 100; // 1秒間に生成するパーティクル数

                var main = goalParticle.main;
                main.duration = 2f; // パーティクルの継続時間
                main.startLifetime = 2f; // 各パーティクルの寿命
                main.startSize = 1f; // パーティクルのサイズ
                main.startSpeed = 5f; // パーティクルの速度

                // パーティクルを再生
                goalParticle.transform.position = transform.position;
                goalParticle.Clear(); // 既存のパーティクルをクリア
                goalParticle.Play();

                Debug.Log("パーティクル設定後:");
                Debug.Log("- Emission enabled: " + emission.enabled);
                Debug.Log("- Rate over time: " + emission.rateOverTime.constant);
                Debug.Log("- Start size: " + main.startSize.constant);
            }
        }
    }
}