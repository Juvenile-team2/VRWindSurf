using UnityEngine;
using UnityEngine.UI;

public class ShowButtonAfterDelay : MonoBehaviour
{
    public float delayTime = 30f; // 表示までの秒数
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.gameObject.SetActive(false); // 最初は非表示
            Invoke(nameof(ShowButton), delayTime); // delayTime秒後に表示
        }
        else
        {
            Debug.LogError("Button component が見つかりません。");
        }
    }

    void ShowButton()
    {
        button.gameObject.SetActive(true); // ボタンを表示
    }
}
