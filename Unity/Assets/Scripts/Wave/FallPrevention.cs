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
        if (isResetting) return; // ���Z�b�g���͏������X�L�b�v

        Vector3 currentRotation = transform.rotation.eulerAngles;
        float xRotation = NormalizeAngle(currentRotation.x);
        float zRotation = NormalizeAngle(currentRotation.z);

        // �p�x�����`�F�b�N, rotationLimit�ȏ�Ȃ烊�Z�b�g����
        if (Mathf.Abs(xRotation) > rotationLimit || Mathf.Abs(zRotation) > rotationLimit)
        {
            ResetPosition(0f);
            return;
        }

        HandlePKeyInput();
    }

    //P�L�[���͂ňʒu���Z�b�g
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
        textComponent.text = $"�ʒu���Z�b�g�܂�: {remainingTime:F1}�b";
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
                textComponent.text = "�ʒu���Z�b�g���J�n���܂�";
            }

            // WindZone�𖳌���
            if (windZone != null)
            {
                windZone.SetActive(false);
            }

            yield return new WaitForSeconds(delay);

            // �x�����WindZone��L����
            if (windZone != null)
            {
                windZone.SetActive(true);
            }
        }

        // �ʒu���Z�b�g
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (textComponent != null)
        {
            textComponent.text = "�ʒu�����Z�b�g���܂���";
            yield return new WaitForSeconds(1f);
            textComponent.text = "";
        }
        Debug.Log("�ʒu���Z�b�g���s���܂���");
        isResetting = false;
        resetCoroutine = null;
    }
}