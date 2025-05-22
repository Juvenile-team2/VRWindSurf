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
        Debug.Log("������^�C���J�n " + delay + " seconds");

        if (windZone != null)
        {
            windZone.SetActive(true);
        }


        if (textComponent != null)
        {
            yield return StartCoroutine(ChangeTextWithFade("�n���h����" + "\n�������E�ɓ������܂��傤�I"));
            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(ChangeTextWithFade("���̌�����" + "\n�i�ތ������ς��܂�")) ;
        }
        else
        {
            Debug.LogError("TextMeshPro component not found!");
        }
    }

    IEnumerator MoveTimeRun(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("�ړ��^�C���J�n " + delay + " seconds");
        yield return StartCoroutine(ChangeTextWithFade("�@"));
        tutorialPanel.SetActive(false);
    }

    private IEnumerator ChangeTextWithFade(string newText)
    {
        // �t�F�[�h�A�E�g
        yield return StartCoroutine(FadeTextRoutine(false));

        // �e�L�X�g�ύX
        textComponent.text = newText;

        // �t�F�[�h�C��
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