using UnityEngine;
using System.Collections;
using TMPro;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class TutrialManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textComponent;
    [SerializeField] 
    private GameObject tutorialPanel;
    [SerializeField]
    private float waveTime = 30f;
    [SerializeField]
    private float sailTime = 30f;
    [SerializeField]
    private float fadeTime = 0.5f;
    [SerializeField]
    private GameObject windZone;

    void Start()
    {
        if (windZone != null)
        {
            windZone.SetActive(false);
        }
        else
        {
            Debug.LogError("WindZone object not assigned!");
        }

        StartCoroutine(SailTimeRun(waveTime));
        StartCoroutine(MoveTimeRun(sailTime));
    }

    IEnumerator SailTimeRun(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("帆操作タイム開始 " + delay + " seconds");

        if (windZone != null)
        {
            windZone.SetActive(true);
        }


        if (textComponent != null)
        {
            yield return StartCoroutine(ChangeTextWithFade("ハンドルで" + "\n帆を左右に動かしましょう！"));
            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(ChangeTextWithFade("帆の向きで" + "\n進む向きが変わります")) ;
        }
        else
        {
            Debug.LogError("TextMeshPro component not found!");
        }
    }

    IEnumerator MoveTimeRun(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("移動タイム開始 " + delay + " seconds");
        yield return StartCoroutine(ChangeTextWithFade("　"));
        tutorialPanel.SetActive(false);
    }

    private IEnumerator ChangeTextWithFade(string newText)
    {
        // フェードアウト
        yield return StartCoroutine(FadeTextRoutine(false));

        // テキスト変更
        textComponent.text = newText;

        // フェードイン
        yield return StartCoroutine(FadeTextRoutine(true));
    }

    private IEnumerator FadeTextRoutine(bool fadeIn)
    {
        float elapsedTime = 0;
        Color startColor = textComponent.color;
        Color endColor = textComponent.color;

        if (fadeIn)
        {
            startColor.a = 0;
            endColor.a = 1;
        }
        else
        {
            startColor.a = 1;
            endColor.a = 0;
        }

        textComponent.color = startColor;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeTime;
            textComponent.color = Color.Lerp(startColor, endColor, normalizedTime);
            yield return null;
        }

        textComponent.color = endColor;
    }
}