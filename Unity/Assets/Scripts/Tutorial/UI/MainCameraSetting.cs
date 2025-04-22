using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainCameraSetting : MonoBehaviour
{
    [SerializeField] private Camera uiCamera; // UI�J�����̎Q��

    void Start()
    {
        // ���C���J�����̎擾
        Camera camera = GetComponent<Camera>();

        // URP�ŗL�̐ݒ�
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Base;

        // �X�^�b�N�J�����Ƃ���UI�J������ǉ�
        cameraData.cameraStack.Add(uiCamera);
    }
}