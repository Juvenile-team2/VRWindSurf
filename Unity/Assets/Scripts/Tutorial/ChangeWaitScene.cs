using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeWaitScene : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            SceneManager.LoadScene("SurfScene 1");
        }
    }

}
