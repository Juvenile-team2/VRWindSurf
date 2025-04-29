using Oculus.Interaction.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChangeCountDown : MonoBehaviour
{

    [SerializeField]
    private TMP_Text countdownText;

    [SerializeField] 
    private TMP_Text descriptionText;

    [SerializeField]
    private int countdownTime = 10;

    [SerializeField]
    private string nextSceneName;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            descriptionText.text = "あと10秒でサーフィン開始！" + "\n足元揺れるよ！気を付けて";

            StartCoroutine(StartCountdown());
        }
    }

    private IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);

        for (int i = countdownTime; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSeconds(1.0f);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
