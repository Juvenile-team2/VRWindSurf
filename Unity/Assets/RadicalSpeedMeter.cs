using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RadicalSpeedMeter : MonoBehaviour
{
    [SerializeField]
    private Image radicalSpeedImage;
    [SerializeField]
    private TMP_Text radicalSpeedNumber;
    [SerializeField]
    private BoardSpeed boardSpeed;

    [SerializeField]
    private float maxWindSpeed = 40f;

    void Update()
    {
        float speed = 4*boardSpeed.GetBoardSpeed();
        float normalized = Mathf.Clamp01(speed / maxWindSpeed);

        if (radicalSpeedNumber != null)
        {
            radicalSpeedNumber.text = $"{(int)speed}";


            // êFïœçX
            if (normalized < 0.33f)
            {
                radicalSpeedImage.color = Color.green;
            }
            else if (normalized < 0.66f)
            {
                radicalSpeedImage.color = Color.yellow;
            }
            else
            {
                radicalSpeedImage.color = Color.red;
            }
            radicalSpeedImage.fillAmount = normalized * 0.75f;
        }
    }
}
