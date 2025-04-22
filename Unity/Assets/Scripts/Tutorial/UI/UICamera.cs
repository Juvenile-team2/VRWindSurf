using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UICamera : MonoBehaviour
{
    void Start()
    {
        // UI�J�����̎擾
        Camera camera = GetComponent<Camera>();

        // Clear Flags�̐ݒ�
        camera.clearFlags = CameraClearFlags.Nothing;  // �����N���A���Ȃ�

        // UI�J�����̐[�x�𑼂̃J�������傫���ݒ�
        camera.depth = 1;

        // URP�ŗL�̐ݒ�
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Overlay;
    }
}