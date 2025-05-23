using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // 回転速度
    [SerializeField] private float currentRotation = 0f; // 現在の帆の回転角度

    public GameObject pivot;

    void Update()
    {
        // 入力を受けて帆を左右に動かす
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // 回転制限を設定
        currentRotation = Mathf.Clamp(currentRotation, -180f, 180f);
        //currentRotation = Mathf.Clamp(currentRotation, 10f, 170f);

        // 帆を回転させる
        transform.localRotation = Quaternion.Euler(-90f,0f,currentRotation);

/*        if (Input.GetKey(KeyCode.LeftArrow)){
            transform.RotateAround(pivot.transform.position, Vector3.up, 20 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(pivot.transform.position, Vector3.up, -20 * Time.deltaTime);
        }*/
    }
}


