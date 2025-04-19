using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainCameraSetting : MonoBehaviour
{
    [SerializeField] private Camera uiCamera; // UIカメラの参照

    void Start()
    {
        // メインカメラの取得
        Camera camera = GetComponent<Camera>();

        // URP固有の設定
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Base;

        // スタックカメラとしてUIカメラを追加
        cameraData.cameraStack.Add(uiCamera);
    }
}