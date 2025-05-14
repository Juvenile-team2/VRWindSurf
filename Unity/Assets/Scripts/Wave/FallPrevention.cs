using UnityEngine;
using UnityEngine.UI;
// using TMPro; // TextMeshPro を使用している場合はこちらを有効に

using System.Collections;
using TMPro;

public class FallPrevention : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField]
    private TMP_Text textComponent;
    // [SerializeField]
    // private Text textComponent;

    [Header("Object Settings")]
    [SerializeField]
    private GameObject windZone;

    [Header("Reset Conditions")]
    [SerializeField]
    private float rotationLimit = 60f;
    [SerializeField]
    private float angularVelocityLimitY = 10f;
    [SerializeField]
    private float requiredHoldTime = 3f;


    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;
    private float keyHoldTime = 0f;
    private bool isResetting = false;
    private Coroutine resetCoroutine = null;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("Rigidbodyコンポーネントが見つかりません。慣性のリセットや角速度によるリセットは行われません。", this.gameObject);
        }
    }

    void Update()
    {
        if (isResetting) return;

        if (rb != null)
        {
            float currentYAngularVelocity = rb.angularVelocity.y;
            if (Mathf.Abs(currentYAngularVelocity) > angularVelocityLimitY)
            {
                Debug.Log($"Y軸周りの角速度 ({currentYAngularVelocity:F2} rad/s) が閾値 ({angularVelocityLimitY} rad/s) を超えました。リセットします。");
                ResetTransformAndInertia(0f);
                return;
            }
        }

        Vector3 currentRotation = transform.rotation.eulerAngles;
        float xRotation = NormalizeAngle(currentRotation.x);
        float zRotation = NormalizeAngle(currentRotation.z);

        if (Mathf.Abs(xRotation) > rotationLimit || Mathf.Abs(zRotation) > rotationLimit)
        {
            Debug.Log($"回転角度 ({xRotation:F1}, {currentRotation.y:F1}, {zRotation:F1}) が制限値 ({rotationLimit}) を超えました。リセットします。");
            ResetTransformAndInertia(0f);
            return;
        }

        HandlePKeyInput();
    }

    private void HandlePKeyInput()
    {
        if (Input.GetKey(KeyCode.P))
        {
            keyHoldTime += Time.deltaTime;
            UpdateHoldTimeText();

            if (keyHoldTime >= requiredHoldTime)
            {
                ResetTransformAndInertia(3f);
                keyHoldTime = 0f;
            }
        }
        else
        {
            if (keyHoldTime > 0)
            {
                keyHoldTime = 0f;
                ClearText();
            }
        }
    }

    private void ResetTransformAndInertia(float delay)
    {
        if (isResetting) return;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(ResetTransformAndInertiaRoutine(delay));
    }

    private void UpdateHoldTimeText()
    {
        if (textComponent == null) return;
        float remainingTime = Mathf.Max(0, requiredHoldTime - keyHoldTime);
        textComponent.text = $"位置・慣性リセットまで: {remainingTime:F1}秒";
    }

    private void ClearText()
    {
        if (textComponent == null) return;
        textComponent.text = "";
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180f)
        {
            angle -= 360f;
        }
        else if (angle < -180f)
        {
            angle += 360f;
        }
        return angle;
    }

    private IEnumerator ResetTransformAndInertiaRoutine(float delay)
    {
        isResetting = true;

        if (delay > 0)
        {
            if (textComponent != null)
            {
                textComponent.text = "位置・慣性リセットを開始します...";
            }

            if (windZone != null)
            {
                windZone.SetActive(false);
            }

            yield return new WaitForSeconds(delay);

            if (windZone != null)
            {
                windZone.SetActive(true);
            }
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // rb.ResetInertiaTensor();
        }

        if (textComponent != null)
        {
            textComponent.text = "位置と慣性をリセットしました";
            yield return new WaitForSeconds(1f);
            ClearText();
        }

        Debug.Log("位置、回転、慣性をリセットしました。");

        isResetting = false;
        resetCoroutine = null;
    }
}