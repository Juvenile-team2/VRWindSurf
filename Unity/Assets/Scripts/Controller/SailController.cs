using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // ‰ñ“]‘¬“x
    private float currentRotation = 0f; // Œ»İ‚Ì”¿‚Ì‰ñ“]Šp“x

    void Update()
    {
        // “ü—Í‚ğó‚¯‚Ä”¿‚ğ¶‰E‚É“®‚©‚·
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // ‰ñ“]§ŒÀ‚ğİ’è
        currentRotation = Mathf.Clamp(currentRotation, -80f, 80f);

        // ”¿‚ğ‰ñ“]‚³‚¹‚é
        transform.localRotation = Quaternion.Euler(0f, currentRotation, 0f);
    }
}


