using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UICamera : MonoBehaviour
{
    void Start()
    {
        // UIカメラの取得
        Camera camera = GetComponent<Camera>();

        // Clear Flagsの設定
        camera.clearFlags = CameraClearFlags.Nothing;  // 何もクリアしない

        // UIカメラの深度を他のカメラより大きく設定
        camera.depth = 1;

        // URP固有の設定
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Overlay;
    }
}