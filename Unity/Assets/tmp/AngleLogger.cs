using System;
using System.IO;
using UnityEngine;

public class AngleLogger : MonoBehaviour
{
    [Header("Log Settings")]
    public float logInterval = 0.5f;
    public string fileName = "angle_log.txt";

    private string logPath;
    private float nextLogTime;

    void Start()
    {
        // Assets/tmp/ に出力
        logPath = Path.Combine(Application.dataPath, "tmp", fileName);

        File.WriteAllText(logPath, $"# Angle Log - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        File.AppendAllText(logPath, "Time,EulerX,EulerY,EulerZ\n");

        nextLogTime = Time.time;
        Debug.Log($"[AngleLogger] Logging to: {logPath}");
    }

    void Update()
    {
        if (Time.time < nextLogTime) return;

        Vector3 euler = transform.eulerAngles;
        File.AppendAllText(logPath, $"{Time.time:F2},{euler.x:F2},{euler.y:F2},{euler.z:F2}\n");
        nextLogTime += logInterval;
    }
}
