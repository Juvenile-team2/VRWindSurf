using UnityEngine;
public class WindMove : MonoBehaviour
{
    // x軸方向に加える風の力
    [SerializeField]
    private float windX = 0f;
    // y軸方向に加える風の力
    [SerializeField]
    private float windY = 0f;
    // z軸方向に加える風の力
    [SerializeField]
    private float windZ = 0f;
    /// <summary>
    /// Is Triggerにチェックが入ったコライダーの範囲内に入っている間に繰り返し実行される関数
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        // 当たった相手のrigidbodyコンポーネントを取得
        Rigidbody otherRigidbody = other.gameObject.GetComponent<Rigidbody>();
        // otherRigidbodyがnullではない場合（相手のGameObjectにrigidbodyがついている場合）
        if (otherRigidbody != null)
        {
            // 相手のrigidbodyに力を加える
            otherRigidbody.AddForce(windX, windY, windZ, ForceMode.Acceleration);
        }
    }
}