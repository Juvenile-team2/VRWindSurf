using UnityEngine;

public class WindSurfBoardController : MonoBehaviour
{
    public Rigidbody rb;
    public Transform sail; // 帆のTransform
    public float movementFactor = 5f; // 帆に基づく移動係数

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 帆の回転角度を取得して、サーフボードの動きに影響を与える
        float sailAngle = sail.localRotation.eulerAngles.y;
        Vector3 forwardDirection = Quaternion.Euler(0, sailAngle, 0) * Vector3.forward;

        // サーフボードの向きを帆の角度に応じて調整
        rb.AddForce(forwardDirection * movementFactor * Time.deltaTime);
    }
}
