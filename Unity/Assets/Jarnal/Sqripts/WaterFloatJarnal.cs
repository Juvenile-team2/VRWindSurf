using Ditzelgames;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterFloatJarnal : MonoBehaviour
{
    public float AirDrag = 2f;
    public float WaterDrag = 20f;
    public float BuoyancyForce = 3f;
    public float SurfaceThreshold = 0.2f;
    public bool AffectDirection = true;
    public bool AttachToSurface = true;
    public Transform[] FloatPoints;

    private Rigidbody rb;
    private WavesFormulaHeightJarnal waves;

    private float waterLine;
    private Vector3[] waterLinePoints;
    private Vector3 smoothVectorRotation;
    private Vector3 targetUp;
    private Vector3 centerOffset;

    public Vector3 Center => transform.position + centerOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.sleepThreshold = 0f;

        waterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++)
            waterLinePoints[i] = FloatPoints[i].position;
        centerOffset = PhysicsHelper.GetCenter(waterLinePoints) - transform.position;
    }

    void Start()
    {
        // 同一オブジェクト優先、なければシーン全体から探す
        waves = GetComponent<WavesFormulaHeightJarnal>()
             ?? FindObjectOfType<WavesFormulaHeightJarnal>();

        if (waves == null)
        {
            Debug.LogError("WaterFloatJarnal: WavesFormulaHeightJarnal が見つかりません。");
            return;
        }

        for (int i = 0; i < FloatPoints.Length; i++)
            waterLine += waves.GetHeight(FloatPoints[i].position) / FloatPoints.Length;
    }

    void FixedUpdate()
    {
        if (waves == null) return;

        var newWaterLine = 0f;
        var pointUnderWater = false;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            waterLinePoints[i] = FloatPoints[i].position;
            waterLinePoints[i].y = waves.GetHeight(FloatPoints[i].position);
            newWaterLine += waterLinePoints[i].y / FloatPoints.Length;
            if (waterLinePoints[i].y > FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        float waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        targetUp = PhysicsHelper.GetNormal(waterLinePoints);

        float submergeDepth = waterLine - Center.y;
        rb.linearDamping = AirDrag;

        if (submergeDepth > 0)
        {
            rb.linearDamping = WaterDrag;
            if (AttachToSurface)
            {
                rb.position = new Vector3(rb.position.x, waterLine - centerOffset.y, rb.position.z);
            }
            else
            {
                Vector3 upDir = AffectDirection ? targetUp : Vector3.up;
                rb.AddForce(upDir * (-Physics.gravity.y * submergeDepth * BuoyancyForce), ForceMode.Acceleration);
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }
        else if (submergeDepth < -SurfaceThreshold)
        {
            rb.linearDamping = AirDrag;
        }
        else
        {
            rb.linearDamping = WaterDrag;
        }

        if (pointUnderWater)
        {
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, 0.2f);
            rb.rotation = Quaternion.FromToRotation(transform.up, targetUp) * rb.rotation;
        }
    }

    void OnDrawGizmos()
    {
        if (FloatPoints == null) return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null) continue;

            if (waves != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(waterLinePoints[i], Vector3.one * 0.3f);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.1f);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, waterLine, Center.z), Vector3.one * 1f);
            Gizmos.DrawRay(new Vector3(Center.x, waterLine, Center.z), targetUp * 1f);
        }
    }
}
