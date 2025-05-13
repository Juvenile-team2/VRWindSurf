using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardSpeed : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] private TMP_Text speedText;


    void Update()
    {
        float speed = rb.linearVelocity.magnitude;
        if (speedText != null)
        {
            speedText.text = $"Speed: {4 * (int)speed} km/h";
        }
    }

    public Vector3 GetBoardVelocity()
    {
        return rb != null ? rb.linearVelocity : Vector3.zero;
    }

    public float GetBoardSpeed()
    {
        return rb != null ? rb.linearVelocity.magnitude : 0f;
    }
}
