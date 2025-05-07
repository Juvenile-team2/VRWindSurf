using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class FallPrevention : MonoBehaviour
{
    [SerializeField]
    private TMP_Text textComponent;
    [SerializeField]
    private GameObject windZone;
    [SerializeField]
    private float rotationLimit = 60f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float keyHoldTime = 0f;
    private float requiredHoldTime = 3f;
    private bool isResetting = false;
    private Coroutine resetCoroutine = null;

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (isResetting) return; // リセット中は処理をスキップ

        Vector3 currentRotation = transform.rotation.eulerAngles;
        float xRotation = NormalizeAngle(currentRotation.x);
        float zRotation = NormalizeAngle(currentRotation.z);

        // 角度制限チェック, rotationLimit以上ならリセット発動
        if (Mathf.Abs(xRotation) > rotationLimit || Mathf.Abs(zRotation) > rotationLimit)
        {
            ResetPosition(0f);
            return;
        }

        HandlePKeyInput();
    }

    //Pキー入力で位置リセット
    private void HandlePKeyInput()
    {
        if (Input.GetKey(KeyCode.P))
        {
            keyHoldTime += Time.deltaTime;
            UpdateHoldTimeText();

            if (keyHoldTime >= requiredHoldTime)
            {
                ResetPosition(3f);
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

    private void ResetPosition(float delay)
    {
        if (isResetting) return;

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }

        resetCoroutine = StartCoroutine(ResetPositionRoutine(delay));
    }

    private void UpdateHoldTimeText()
    {
        if (textComponent == null) return;
        float remainingTime = requiredHoldTime - keyHoldTime;
        textComponent.text = $"位置リセットまで: {remainingTime:F1}秒";
    }

    private void ClearText()
    {
        if (textComponent == null) return;
        textComponent.text = "";
    }

    private float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private IEnumerator ResetPositionRoutine(float delay)
    {
        isResetting = true;

        if (delay > 0)
        {
            if (textComponent != null)
            {
                textComponent.text = "位置リセットを開始します";
            }

            // WindZoneを無効化
            if (windZone != null)
            {
                windZone.SetActive(false);
            }

            yield return new WaitForSeconds(delay);

            // 遅延後にWindZoneを有効化
            if (windZone != null)
            {
                windZone.SetActive(true);
            }
        }

        // 位置リセット
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (textComponent != null)
        {
            textComponent.text = "位置をリセットしました";
            yield return new WaitForSeconds(1f);
            textComponent.text = "";
        }
        Debug.Log("位置リセットを行いました");
        isResetting = false;
        resetCoroutine = null;
    }
}