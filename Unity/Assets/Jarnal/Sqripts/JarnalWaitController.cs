using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Jarnal実験の条件間の待機シーンに置く。
/// Spaceキーで、条件リストが残っていれば次の条件のJarnal実験シーンへ、
/// 全条件が終わっていれば終了シーンへ遷移する。
/// </summary>
public class JarnalWaitController : MonoBehaviour
{
    [Tooltip("ExperimentManagerと同じ条件リストアセットを指定する")]
    public ExperimentConditionListSO conditionList;

    [Tooltip("条件を実行するシーン名")]
    public string experimentSceneName = "Jarnal 1";

    [Tooltip("全条件終了後に遷移するシーン名")]
    public string finishSceneName = "WaitingScene";

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        if (conditionList != null && conditionList.HasCurrent)
        {
            SceneManager.LoadScene(experimentSceneName);
        }
        else
        {
            SceneManager.LoadScene(finishSceneName);
        }
    }
}
