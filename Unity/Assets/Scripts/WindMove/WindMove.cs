using UnityEngine;
using System.Collections;

public class WindMove : MonoBehaviour
{
    // ????????
    [SerializeField]
    private float windX = 0f;
    [SerializeField]
    private float windY = 0f;
    [SerializeField]
    private float windZ = 0f;

    [SerializeField]
    private float arrowX = 0f;
    [SerializeField]
    private float arrowY = 0f;
    [SerializeField]
    private float arrowZ = 0f;

    // ?????g???W?????R???W??
    [SerializeField]
    private float maxLiftCoefficient = 1.5f;
    private float baseDragCoefficient = 0.05f;
    private float dragIncreaseRate = 0.1f;

    [SerializeField]
    float dragCofficient = 10f;

    [SerializeField]
    private float airDensity = 1.225f;

    //船のrigitbody
    public Rigidbody rb;

    //boardのtransform
    public Transform boardtf;

    //矢印のtransform
    public Transform arrowtf;
    
    [SerializeField]
    private WindMoveController controller;

    private Vector3 wind = Vector3.zero;

    [SerializeField]
    private bool isWeb = true;

    //風の向きが変わる周期
    public float windChangeInterval = 60f;

    public bool windRandomSwitch = true;



    private void Start()
    {
        if (windRandomSwitch)
        {
            // 風の強さと向きを1分ごとに変更するコルーチンを開始
            StartCoroutine(ChangeWindDirection());
        }
        
    }

    private void OnTriggerStay(Collider other)
    {
        // ???iSail?j????????????????
        if (other.CompareTag("Sail"))
        {
            // オブジェクトのRigidbodyを取得
            //Rigidbody parentRigidbody = other.transform.GetComponent<Rigidbody>();

            // ????Transform??????

            Transform sailTransform = other.transform;

            Vector3 sailRight = sailTransform.right;
            Vector3 sailForward = sailTransform.forward;

            //帆の向きの修正
            //Quaternion rotated = Quaternion.Euler(90f, 0f, 0f) * sailTransform.rotation;

            if (rb != null && sailTransform != null)
            {

                float windSpeed;
                Vector3 windDirection;

                if (isWeb)
                {
                    // ?????????????????v?Z
                    wind = controller.GetLatestValue();
                    windSpeed = CalculateWindSpeed(wind.x, wind.y, wind.z);
                    windDirection = CalculateWindDirection(wind.x, wind.y, wind.z);
                }
                else
                {
                    windSpeed = CalculateWindSpeed(windX, windY, windZ);
                    windDirection = CalculateWindDirection(windX, windY, windZ);
                }

                Vector3 sailDirection = sailTransform.up;

                // 帆の回転rotationはzが上向き
                /*if (sailTransform.rotation.z < 0f)
                {
                    sailDirection = sailTransform.up;
                }
                else
                {
                    sailDirection = -sailTransform.up;
                }*/

                //仰角(正負あり)
                float signedAngle = Vector3.SignedAngle(sailDirection, windDirection, Vector3.up);

                //仰角(正負なし)
                float angleOfAttack = CalculateAngleOfAttack(windDirection, sailDirection);

                Vector3 windAffection = CalculateWindAffection(windSpeed, windDirection, angleOfAttack);

                // 揚力と抗力係数を計算
                float liftCoefficient = CalculateLiftCoefficient(angleOfAttack);
                //float dragCoefficient = CalculateDragCoefficient(liftCoefficient);
                
                //揚力の方向を計算
                Vector3 liftDirection = CalculateLiftDirection(windDirection, sailRight, signedAngle);

                // 揚力と抗力を計算
                Vector3 liftForce = CalculateLiftForce(windSpeed, windDirection, liftDirection, rb, liftCoefficient);
                Vector3 dragForce = CalculateDragForce(windSpeed, windDirection, sailDirection, dragCofficient);

                // 推進力を計算
                Vector3 thrustForce = CalculateThrustForce(liftForce, dragForce, windAffection);

                ////ボードの向きを修正
                //Quaternion newRotation = Quaternion.LookRotation(liftdirection * Mathf.Sin(angleOfAttack), Vector3.up);
                //boardtf.rotation = newRotation;

                //推進力の方向表示
                Quaternion lookRotation = Quaternion.LookRotation(thrustForce);

                Quaternion correction = Quaternion.Euler(arrowX, arrowY, arrowZ);

                arrowtf.rotation = lookRotation * correction;

                //矢印がくるってた時用の向きチェック
                //Debug.DrawRay(rb.position, liftDirection * 3f, Color.green);

                // 親オブジェクト（船）に力を適用
                rb.AddForce(thrustForce, ForceMode.Acceleration);

            }
        }
    }

    // 風の強さ
    float CalculateWindSpeed(float windx, float windy, float windz)
    {
        return Mathf.Sqrt(windx * windx + windy * windy + windz * windz);
    }

    // 風向き
    Vector3 CalculateWindDirection(float windx, float windy, float windz)
    {
        float windSpeed = CalculateWindSpeed(windx, windy, windz);
        if (windSpeed == 0) return Vector3.zero;
        return new Vector3(windx / windSpeed, windy / windSpeed, windz / windSpeed);
    }

    //風の影響
    Vector3 CalculateWindAffection(float windSpeed, Vector3 windDirection, float angleOfAttack)
    {
        return windSpeed * windDirection * angleOfAttack;
    }

    // 風と帆の向きの仰角
    float CalculateAngleOfAttack(Vector3 windDirection, Vector3 sailDirection)
    {
        return Vector3.Angle(windDirection, sailDirection) * Mathf.Deg2Rad;
    }

    // 揚力係数
    float CalculateLiftCoefficient(float angleOfAttack)
    {
        return maxLiftCoefficient * Mathf.Sin(2 * angleOfAttack);
    }

    // 抗力係数
    float CalculateDragCoefficient(float liftCoefficient)
    {
        return baseDragCoefficient + dragIncreaseRate * liftCoefficient * liftCoefficient;
    }

    Vector3 CalculateLiftDirection(Vector3 windDirection, Vector3 sailDirection, float signedAngle)
    {

        //揚力は常に風と垂直な方向にある
        Vector3 liftDirection;

        //仰角の正負で揚力の向きを変える
        if (signedAngle > 0)
        {
            liftDirection = Vector3.Cross(windDirection, Vector3.up);
        }
        else
        {
            liftDirection = -Vector3.Cross(windDirection, Vector3.up);
        }

        return liftDirection;
    }

    // 揚力を計算
    Vector3 CalculateLiftForce(float windSpeed, Vector3 windDirection, Vector3 liftDirection, Rigidbody rigidbody, float liftCoefficient)
    {
        //揚力の大きさ
        float liftForceMagnitude = 0.5f * windSpeed * windSpeed * liftCoefficient * airDensity * rigidbody.mass;

        return liftForceMagnitude * liftDirection;
    }

    // 風と反対方向の力
    Vector3 CalculateDragForce(float windSpeed, Vector3 windDirection, Vector3 sailAngle, float dragCofficient)
    {
        Vector3 dragDirection = -windDirection;

        float Angle = Vector3.Angle(sailAngle, windDirection) * Mathf.Deg2Rad;
        //Debug.Log("Angle :" + Angle);

        //風と帆の間の角度が小さくなるほど大きくなる、もう少し大きくしなければいけないかも
        Vector3 dragForce = dragDirection * Mathf.Abs(Mathf.Cos(Angle)) * dragCofficient;

        return dragForce;
    }

    // 推進力を計算
    Vector3 CalculateThrustForce(Vector3 liftForce, Vector3 dragForce, Vector3 windAffection)
    {

        Vector3 thrustForce = liftForce + dragForce + windAffection;

        return thrustForce;
    }

    public Vector3 GetWindDirection()
    {
        return CalculateWindDirection(windX, windY, windZ);
    }

    public float GetWindSpeed()
    {
        return CalculateWindSpeed(windX, windY, windZ);
    }

    private IEnumerator ChangeWindDirection()
    {
        while (true)
        {
            // ランダムに風のX,Z成分を変更
            if (!isWeb)
            {
                windX = GetRandomWindComponent();
                windZ = GetRandomWindComponent();
            }

            // 1分待つ
            yield return new WaitForSeconds(windChangeInterval);
        }
    }

    float GetRandomWindComponent()
    {
        // 0～1のランダム値を生成して、範囲を選択
        if (UnityEngine.Random.value < 0.5f)
        {
            // -5～-1の範囲で値を取得
            return UnityEngine.Random.Range(-5f, -1f);
        }
        else
        {
            // 1～5の範囲で値を取得
            return UnityEngine.Random.Range(1f, 5f);
        }
    }
}
