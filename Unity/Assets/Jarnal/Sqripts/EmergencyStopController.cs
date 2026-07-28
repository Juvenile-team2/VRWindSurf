using UnityEngine;

/// <summary>
/// 実験中にQキーを1秒間長押しすると緊急停止する。
/// アクチュエータへ即座に停止コマンドを送り、ExperimentManagerでトライアルを中断して待機シーンへ戻す。
/// 条件は進めないため、待機シーンで再開すると同じ条件からやり直せる。
/// </summary>
public class EmergencyStopController : MonoBehaviour
{
    [Tooltip("長押しで緊急停止をトリガーするキー")]
    public KeyCode stopKey = KeyCode.Q;

    [Tooltip("長押しが必要な秒数")]
    public float holdDuration = 1f;

    [Tooltip("未設定ならシーンから自動検索する")]
    public ExperimentManager experimentManager;

    private float heldTime = 0f;
    private bool triggered = false;

    void Start()
    {
        if (experimentManager == null)
            experimentManager = FindObjectOfType<ExperimentManager>();
    }

    void Update()
    {
        if (triggered) return;

        if (Input.GetKey(stopKey))
        {
            // Time.timeScaleの影響を受けないよう実時間で計測する
            heldTime += Time.unscaledDeltaTime;
            if (heldTime >= holdDuration)
            {
                triggered = true;
                Trigger();
            }
        }
        else
        {
            heldTime = 0f;
        }
    }

    void Trigger()
    {
        Debug.LogWarning("EmergencyStopController: 緊急停止が発動しました。");

        if (experimentManager == null)
        {
            Debug.LogError("EmergencyStopController: ExperimentManager が見つかりません。");
            return;
        }

        experimentManager.EmergencyAbort();
    }
}
