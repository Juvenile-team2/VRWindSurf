using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class TimeController : MonoBehaviour
{
    [SerializeField] private int countDownTime = 60;
    [SerializeField] private int resultTime = 5;
    [SerializeField] private GameObject countDownPanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text countDownText;
    [SerializeField] private TMP_Text descriptionText;
    private bool isCleared = false;
    // クリア時間
    private float elapsedTime = 0f; 

    void Start()
    {
        elapsedTime = countDownTime;
        StartCoroutine(CountDown());
    }


    public void SetCleared()
    {
        isCleared = true;
    }

    IEnumerator CountDown()
    {
        while (countDownTime > 0 && !isCleared)
        {
            yield return new WaitForSeconds(1f);
            countDownTime--;
            countDownText.text = countDownTime.ToString();
        }

        // 結果表示パート
        countDownText.text = "";
        countDownPanel.SetActive(false);
        resultPanel.SetActive(true);
        if (isCleared)
        {
            countDownText.text = "";
            string timeStr = FormatTime(elapsedTime - countDownTime);
            descriptionText.text = "おめでとう！ゴールに到達しました！\nクリアタイム: " + timeStr;
        }
        else
        {
            countDownText.text = "";
            descriptionText.text = "ナイスチャレンジ！\n最後までプレイしてくれてありがとう！";
        }

        yield return new WaitForSeconds(resultTime);
        SceneManager.LoadScene("WaitingScene");
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{00:00}:{1:00}", minutes, seconds);
    }
}
