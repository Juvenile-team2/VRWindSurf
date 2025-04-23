using UnityEngine;

public class WindIndicator : MonoBehaviour
{
    [SerializeField] private Transform arrow; // ����
    [SerializeField] private WindMove windMove; // WindMove.cs
    [SerializeField] private Transform surfboard; // �{�[�h��Transform

    private void Update()
    {
        UpdateWindArrowDirection();
    }

    private void UpdateWindArrowDirection()
    {
        if (windMove == null || surfboard == null || arrow == null) return;

        // WindMove���畗�������擾
        Vector3 worldWindDirection = windMove.GetWindDirection();

        // �T�[�t�{�[�h�̃��[�J�����W�n�ł̕������v�Z
        Vector3 localWindDirection = surfboard.InverseTransformDirection(worldWindDirection);

        // XZ���ʂł̕����p�x���v�Z�i�T�[�t�{�[�h���猩�ĕ����ǂ̕������琁���Ă��邩�j
        float angle = Mathf.Atan2(localWindDirection.x, localWindDirection.z) * Mathf.Rad2Deg;

        // ������]�iY������j
        arrow.localRotation = Quaternion.Euler(0, angle, 0);
    }
}