using UnityEngine;

public class DebugWeightController : MonoBehaviour
{
    [Header("Debug Settings")]
    [Range(0, 6000)]
    public float simulatedValue = 3000f;

    [Tooltip("If true, value will change randomly over time")]
    public bool simulateRandom = false;

    [Tooltip("Speed of value change if simulateRandom is enabled")]
    public float randomChangeSpeed = 1000f;

    private float currentValue;

    void Start()
    {
        currentValue = simulatedValue;
    }

    void Update()
    {
        if (simulateRandom)
        {
            float change = Random.Range(-randomChangeSpeed, randomChangeSpeed) * Time.deltaTime;
            currentValue = Mathf.Clamp(currentValue + change, 0f, 6000f);
        }
        else
        {
            currentValue = simulatedValue;
        }
    }

    public float GetLatestValue()
    {
        return currentValue;
    }
}
