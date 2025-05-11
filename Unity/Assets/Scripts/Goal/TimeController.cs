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
    private float elapsedTime = 0f;
    private Coroutine countDownCoroutine;

    void Start()
    {
        elapsedTime = countDownTime;
        countDownCoroutine = StartCoroutine(CountDown());
    }

    public void SetCleared()
    {
        isCleared = true;
        Debug.Log("クリアフラグ受け取り");
        if (countDownCoroutine != null)
        {
            StopCoroutine(countDownCoroutine);
        }
        StartCoroutine(ShowResult());
    }

    IEnumerator CountDown()
    {
        while (countDownTime > 0 && !isCleared)
        {
            yield return new WaitForSeconds(1f);
            countDownTime--;
            countDownText.text = countDownTime.ToString();
        }

        if (!isCleared)
        {
            StartCoroutine(ShowResult());
        }
    }

    IEnumerator ShowResult()
    {
        countDownPanel.SetActive(false);
        resultPanel.SetActive(true);

        if (isCleared)
        {
            string timeStr = FormatTime(elapsedTime - countDownTime);
            descriptionText.text = "おめでとう！ゴールに到達しました！\nクリアタイム: " + timeStr;
        }
        else
        {
            descriptionText.text = "ナイスチャレンジ！\n最後までプレイしてくれてありがとう！";
        }

        yield return new WaitForSeconds(resultTime);
        SceneManager.LoadScene("WaitingScene");
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}