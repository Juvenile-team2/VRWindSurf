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
    public  DifficultyDataSO rotationSettings;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (rotationSettings != null)
            {
                rotationSettings.rotationFlag = false;
            }
            else
            {
                Debug.LogWarning("Scriptable�I�u�W�F�N�g������܂���");
            }

            descriptionText.text = "����10�b�ŃT�[�t�B���J�n�I" + "\n�����h����I�C��t����";
            StartCoroutine(StartCountdown());
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (rotationSettings != null)
            {
                rotationSettings.rotationFlag =true;
            }
            else
            { 
                Debug.LogWarning("Scriptable�I�u�W�F�N�g������܂���"); 
            }

            descriptionText.text = "����10�b�ŃT�[�t�B���J�n�I" + "\n�����h����I�C��t����";
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

        SceneManager.LoadScene("SurfScene 1");
    }
}
