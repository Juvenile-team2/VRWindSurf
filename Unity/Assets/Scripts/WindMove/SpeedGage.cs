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

        // �X���C�_�[�̒l���X�V
        BoardSpeedSlider.value = normalized;

        // �F�ύX
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

