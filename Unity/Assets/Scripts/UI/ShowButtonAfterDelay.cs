using UnityEngine;
using UnityEngine.UI;

public class ShowButtonAfterDelay : MonoBehaviour
{
    public float delayTime = 30f; // �\���܂ł̕b��
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.gameObject.SetActive(false); // �ŏ��͔�\��
            Invoke(nameof(ShowButton), delayTime); // delayTime�b��ɕ\��
        }
        else
        {
            Debug.LogError("Button component ��������܂���B");
        }
    }

    void ShowButton()
    {
        button.gameObject.SetActive(true); // �{�^����\��
    }
}
