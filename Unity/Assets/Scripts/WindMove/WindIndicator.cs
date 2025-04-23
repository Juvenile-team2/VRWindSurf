using UnityEngine;

public class WindIndicator : MonoBehaviour
{
    [SerializeField] private Transform arrow; // 風矢
    [SerializeField] private WindMove windMove; // WindMove.cs
    [SerializeField] private Transform surfboard; // ボードのTransform

    private void Update()
    {
        UpdateWindArrowDirection();
    }

    private void UpdateWindArrowDirection()
    {
        if (windMove == null || surfboard == null || arrow == null) return;

        // WindMoveから風向きを取得
        Vector3 worldWindDirection = windMove.GetWindDirection();

        // サーフボードのローカル座標系での風向を計算
        Vector3 localWindDirection = surfboard.InverseTransformDirection(worldWindDirection);

        // XZ平面での風向角度を計算（サーフボードから見て風がどの方向から吹いてくるか）
        float angle = Mathf.Atan2(localWindDirection.x, localWindDirection.z) * Mathf.Rad2Deg;

        // 矢印を回転（Y軸周り）
        arrow.localRotation = Quaternion.Euler(0, angle, 0);
    }
}