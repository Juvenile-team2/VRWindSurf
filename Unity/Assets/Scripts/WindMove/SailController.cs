using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // ‰ñ“]‘¬“x
    private float currentRotation = 90f; // Œ»İ‚Ì”¿‚Ì‰ñ“]Šp“x

    public GameObject pivot;

    void Update()
    {
        // “ü—Í‚ğó‚¯‚Ä”¿‚ğ¶‰E‚É“®‚©‚·
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // ‰ñ“]§ŒÀ‚ğİ’è
        currentRotation = Mathf.Clamp(currentRotation, 10f, 170f);

        // ”¿‚ğ‰ñ“]‚³‚¹‚é
        transform.localRotation = Quaternion.Euler(0f, currentRotation, 0f);

/*        if (Input.GetKey(KeyCode.LeftArrow)){
            transform.RotateAround(pivot.transform.position, Vector3.up, 20 * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.RotateAround(pivot.transform.position, Vector3.up, -20 * Time.deltaTime);
        }*/
    }
}


