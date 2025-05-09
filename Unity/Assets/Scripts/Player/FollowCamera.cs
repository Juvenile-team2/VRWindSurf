using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FollowCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followMoveSpeed = 0.1f;
    [SerializeField] private float followRotateSpeed = 0.02f;
    [SerializeField] private float rotateSpeedThreshold = 0.9f;
    [SerializeField] private bool isImmediateMove;
    [SerializeField] private bool isLockX;
    [SerializeField] private bool isLockY;
    [SerializeField] private bool isLockZ;
    private Quaternion rot;
    private Quaternion rotDif;

    private void Start()
    {
        if (!target) target = Camera.main.transform;
    }

    private void LateUpdate()
    {
        Vector3 targetPosition = target.position; //+ target.forward * 50000000;

        if (isImmediateMove)
            transform.position = targetPosition;
        else
            transform.position = Vector3.Lerp(transform.position, targetPosition, followMoveSpeed);

        rotDif = target.rotation * Quaternion.Inverse(transform.rotation);
        rot = target.rotation;

        if (isLockX) rot.x = 0;
        if (isLockY) rot.y = 0;
        if (isLockZ) rot.z = 0;

        if (rotDif.w < rotateSpeedThreshold)
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, followRotateSpeed * 4);
        else
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, followRotateSpeed);
    }


    //�����I�ɓ�������������
    public void ImmediateSync(Transform targetTransform)
    {
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
    }
}