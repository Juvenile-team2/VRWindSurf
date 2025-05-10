using UnityEngine;

public class GoalController : MonoBehaviour
{
    [SerializeField] private ParticleSystem goalParticle;
    [SerializeField] private TimeController timeController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ゴールに到達しました！");

            // TimeControllerのクリア処理
            if (timeController != null)
            {
                timeController.SetCleared();
            }
            else
            {
                Debug.LogWarning("TimeControllerが設定されていません");
            }

            if (goalParticle != null)
            {
                var emission = goalParticle.emission;
                var main = goalParticle.main;

                // パーティクル再生
                goalParticle.transform.position = transform.position;
                goalParticle.Clear();
                goalParticle.Play();

                Debug.Log("パーティクル設定後:");
                Debug.Log("- Emission enabled: " + emission.enabled);
                Debug.Log("- Rate over time: " + emission.rateOverTime.constant);
                Debug.Log("- Start size: " + main.startSize.constant);
            }
        }
    }
}