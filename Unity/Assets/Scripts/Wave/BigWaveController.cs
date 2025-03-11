using UnityEngine;

public class BigWaveController : MonoBehaviour
{
    public BigWaveCreate travelingWave;
    private float cooldownTime = 2f;
    private bool canSpawn = true;

    void Start()
    {
        if (travelingWave == null)
        {
            Debug.LogError("TravelingWave component not assigned!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canSpawn)
        {
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        if (canSpawn)
        {
            travelingWave.StartWave();
            canSpawn = false;
            Invoke("ResetCooldown", cooldownTime);
        }
    }

    private void ResetCooldown()
    {
        canSpawn = true;
    }
}