using UnityEngine;
using UnityEngine.UI;

public class SpeedGage : MonoBehaviour
{
    public Slider BoardSpeedSlider;
    public Image fillImage;
    public BoardSpeed boardSpeed;

    [SerializeField] private float maxWindSpeed = 10f;

    void Update()
    {
        float speed = boardSpeed.GetBoardSpeed();
        float normalized = Mathf.Clamp01(speed / maxWindSpeed);

        // スライダーの値を更新
        BoardSpeedSlider.value = normalized;

        // 色変更
        if (normalized < 0.33f)
        {
            fillImage.color = Color.green;
        }
        else if (normalized < 0.66f)
        {
            fillImage.color = Color.yellow;
        }
        else
        {
            fillImage.color = Color.red;
        }
    }
}

