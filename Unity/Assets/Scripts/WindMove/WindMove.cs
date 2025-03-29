using UnityEngine;

public class WindMove : MonoBehaviour
{
    // •—‚Ì¬•ª
    [SerializeField]
    private float windX = 0f;
    [SerializeField]
    private float windY = 0f;
    [SerializeField]
    private float windZ = 0f;

    // Å‘å—g—ÍŒW”‚ÆR—ÍŒW”
    private float maxLiftCoefficient = 1.2f;
    private float baseDragCoefficient = 0.05f;
    private float dragIncreaseRate = 0.1f;

    private float airDensity = 1.225f;

    //public Rigidbody rb;

    void Start()
    {
        //rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        // ”¿iSailj‚É‚Ô‚Â‚©‚Á‚½ê‡
        if (other.CompareTag("Sail"))
        {
            // eƒIƒuƒWƒFƒNƒgi‘Dj‚ÌRigidbody‚ğæ“¾
            Rigidbody parentRigidbody = other.transform.GetComponent<Rigidbody>();

            // ”¿‚ÌTransform‚ğæ“¾
            Transform sailTransform = other.transform;

            if (parentRigidbody != null && sailTransform != null)
            {
                // •—‚Ì‹­‚³‚ÆŒü‚«‚ğŒvZ
                float windSpeed = CalculateWindSpeed(windX, windY, windZ);
                Vector3 windDirection = CalculateWindDirection(windX, windY, windZ);
                //Debug.Log("•—‚ÌŒü‚«" + windDirection);

                // x²‚ÌÀ•W‚©‚çA”¿‚ÌŒü‚«‚ğæ“¾ 
                Vector3 sailDirection = sailTransform.right;
                Debug.Log("”¿‚ÌŒü‚«: " + sailDirection);

                // ‹ÂŠpi•—‚Æ”¿‚ÌŠp“xj‚ğŒvZ
                float angleOfAttack = CalculateAngleOfAttack(windDirection, sailDirection);
                Debug.Log("‹ÂŠp (radians): " + angleOfAttack);

                // —g—ÍŒW”‚ÆR—ÍŒW”‚ğŒvZ
                float liftCoefficient = CalculateLiftCoefficient(angleOfAttack);
                //Debug.Log("—g—ÍŒW”: " + liftCoefficient);
                //float dragCoefficient = CalculateDragCoefficient(liftCoefficient);

                // —g—Í‚ÆR—Í‚ğŒvZ
                Vector3 liftForce = CalculateLiftForce(windSpeed, windDirection, sailDirection, parentRigidbody, liftCoefficient);
                //Debug.Log("—g—Í: " + liftForce);
                //Vector3 dragForce = CalculateDragForce(windSpeed, windDirection, parentRigidbody, dragCoefficient);

                // „i—Í‚ğŒvZ
                Vector3 thrustForce = CalculateThrustForce(liftForce, angleOfAttack);
                //Debug.Log("„i—Í: " + thrustForce);

                // eƒIƒuƒWƒFƒNƒgi‘Dj‚É—Í‚ğ“K—p
                parentRigidbody.AddForce(thrustForce, ForceMode.Acceleration);

            }
        }
    }

    // •—‚Ì‹­‚³‚ğŒvZ
    float CalculateWindSpeed(float windx, float windy, float windz)
    {
        return Mathf.Sqrt(windx * windx + windy * windy + windz * windz);
    }

    // •—‚ÌŒü‚«‚ğŒvZ
    Vector3 CalculateWindDirection(float windx, float windy, float windz)
    {
        float windSpeed = CalculateWindSpeed(windx, windy, windz);
        if (windSpeed == 0) return Vector3.zero;
        return new Vector3(windx / windSpeed, windy / windSpeed, windz / windSpeed);
    }

    // ‹ÂŠp‚ğŒvZi•—‚Æ”¿‚ÌŠp“xj
    float CalculateAngleOfAttack(Vector3 windDirection, Vector3 sailDirection)
    {
        return Vector3.Angle(windDirection, sailDirection) * Mathf.Deg2Rad;
    }

    // —g—ÍŒW”‚ğŒvZ
    float CalculateLiftCoefficient(float angleOfAttack)
    {
        return maxLiftCoefficient * Mathf.Sin(2 * angleOfAttack);
    }

    // R—ÍŒW”‚ğŒvZ
    float CalculateDragCoefficient(float liftCoefficient)
    {
        return baseDragCoefficient + dragIncreaseRate * liftCoefficient * liftCoefficient;
    }

    // —g—Í‚ğŒvZ
    Vector3 CalculateLiftForce(float windSpeed, Vector3 windDirection, Vector3 sailDirection, Rigidbody rigidbody, float liftCoefficient)
    {

        // ”¿‚Ì•ûŒü‚É‘Î‚µ‚Ä‚’¼‚È—g—Í‚Ì•ûŒü‚ğŒvZ
        Vector3 liftDirection = Vector3.Cross(sailDirection, Vector3.up).normalized;

        Debug.Log("•—‚Æ”¿‚ÌŠOÏ" + liftDirection);

        // Y•ûŒü‚Ì—g—Í‚ğ–³‹iXZ•½–Ê‚ÉŒÀ’èj
        liftDirection.y = 0;

        // —g—Í‚Ì‘å‚«‚³‚ğŒvZ
        float liftForceMagnitude = 0.5f * windSpeed * windSpeed * liftCoefficient * airDensity * rigidbody.mass;

        // —g—ÍƒxƒNƒgƒ‹‚ğ•Ô‚·
        return liftForceMagnitude * -liftDirection;
    }

    // R—Í‚ğŒvZ
    Vector3 CalculateDragForce(float windSpeed, Vector3 windDirection, Rigidbody rigidbody, float dragCoefficient)
    {
        float dragForceMagnitude = 0.5f * windSpeed * windSpeed * dragCoefficient * airDensity * rigidbody.mass;
        Vector3 dragForce = dragForceMagnitude * windDirection; // •—‚Æ“¯‚¶Œü‚«

        // Y•ûŒü‚ÌR—Í‚ğ–³‹iXZ•½–Ê‚ÉŒÀ’èj
        dragForce.y = 0;

        return dragForce;
    }

    // „i—Í‚ğŒvZ
    Vector3 CalculateThrustForce(Vector3 lift, float angleOfAttack)
    {

        Vector3 thrustForce = lift * Mathf.Sin(angleOfAttack);

        return thrustForce;
    }
}
