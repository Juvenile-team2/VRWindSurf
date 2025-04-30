using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene: MonoBehaviour
{
    private float pressTime = 0f;
    private const float REQUIRED_PRESS_TIME = 3f;

    void Update()
    {
        if (Input.GetKey(KeyCode.R))
        {
            pressTime += Time.deltaTime;

            if (pressTime >= REQUIRED_PRESS_TIME)
            {
                SceneManager.LoadScene("WaitingScene");
            }
        }
        else
        {
            pressTime = 0f;
        }
    }
}
