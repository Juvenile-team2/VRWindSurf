using UnityEngine;

public class SailController : MonoBehaviour
{
    public float rotationSpeed = 50f; // ‰ñ“]‘¬“x
    [SerializeField] private float currentRotation = 0f; // Œ»İ‚Ì”¿‚Ì‰ñ“]Šp“x

    public GameObject pivot;

    void Update()
    {
        // “ü—Í‚ğó‚¯‚Ä”¿‚ğ¶‰E‚É“®‚©‚·
        float rotationInput = Input.GetAxis("Horizontal");
        currentRotation += rotationInput * rotationSpeed * Time.deltaTime;

        // ‰ñ“]§ŒÀ‚ğİ’è
        currentRotation = Mathf.Clamp(currentRotation, -80f, 80f);
        //currentRotation = Mathf.Clamp(currentRotation, 10f, 170f);

        // ”¿‚ğ‰ñ“]‚³‚¹‚é
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


