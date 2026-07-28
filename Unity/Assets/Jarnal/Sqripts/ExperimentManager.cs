using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

/// <summary>
/// 実験セッションの設定・ログ管理を一元管理するコンポーネント。
/// 独立変数: waveMode（波の高さ） × actuatorScale（アクチュエータ強度）
/// シーン内にひとつだけ置く。
/// </summary>
public class ExperimentManager : MonoBehaviour
{
    // ── 実験メタ情報 ──────────────────────────────────────────
    [Header("実験情報")]
    public string participantId = "P001";

    // ── 独立変数1: 波モード ───────────────────────────────────
    public enum WaveMode { Normal, Half, Third }

    [Header("独立変数1: 波の高さ")]
    public WaveMode waveMode = WaveMode.Normal;

    [Tooltip("WavesFormulaHeightJarnal が付いているオブジェクト（Cube）")]
    public WavesFormulaHeightJarnal wavesFormula;

    [Tooltip("WaveMeshJarnal が付いているオブジェクト（WaveJarnal）")]
    public WaveMeshJarnal waveMesh;

    // ── 独立変数2: アクチュエータスケール ────────────────────
    [Header("独立変数2: アクチュエータ送信スケール (0〜100%)")]
    [Range(0f, 100f)]
    public float actuatorScale = 100f;
    public ZActuatorController actuatorController;

    // ── 条件リストによる自動進行 ──────────────────────────────
    [Header("条件リスト（設定すると waveMode / actuatorScale はリストの現在の条件で上書きされる）")]
    [Tooltip("実行する条件（波モード×アクチュエータスケール）を順番に並べたアセット")]
    public ExperimentConditionListSO conditionList;

    [Tooltip("1条件あたりの実行時間（秒）。経過すると自動的に待機シーンへ遷移する")]
    public float sessionDuration = 120f;

    [Tooltip("条件終了後に戻る待機シーン名")]
    public string waitSceneName = "JarnalWaiting";

    // ── ログ対象 ──────────────────────────────────────────────
    [Header("ログ対象")]
    [Tooltip("位置ログを取るCube本体")]
    public Transform cubeTransform;

    [Tooltip("頭の位置として使うCameraRig（[BuildingBlock] Camera Rig）")]
    public Transform cameraRigTransform;

    [Header("ログ間隔（秒）")]
    public float logInterval = 0.1f;  // 10Hz

    // ── 実回転角度ロガー（ラズパイ）───────────────────────────
    [Header("実回転角度ロガー（ラズパイのIMU）")]
    [Tooltip("オフにするとラズパイへの接続・ログ要求を一切行わない")]
    public bool useRotationLogger = true;
    [Tooltip("RealRotationLogger/server.py を実行しているラズパイのIPアドレス")]
    public string rotationLoggerHost = "192.168.16.8";
    public int rotationLoggerPort = 12346;

    // ── 内部 ─────────────────────────────────────────────────
    private StreamWriter cubeLog;
    private StreamWriter cameraLog;
    private float logTimer = 0f;
    private string sessionDir;
    private float sessionStartTime;
    private bool trialEnded = false;
    private TcpClient rotationLoggerClient;
    private NetworkStream rotationLoggerStream;

    // 条件コードを自動生成: Wave-Normal_Act-100 のような形式
    private string ConditionCode =>
        $"Wave-{waveMode}_Act-{actuatorScale:F0}";

    // ─────────────────────────────────────────────────────────

    void Start()
    {
        ApplyCurrentCondition();
        ApplySettings();
        CreateSessionDirectory();
        WriteExperimentInfo();
        OpenLogFiles();
        ConnectRotationLogger();
        sessionStartTime = Time.time;
    }

    void Update()
    {
        logTimer += Time.deltaTime;
        if (logTimer >= logInterval)
        {
            logTimer = 0f;
            WriteLogs();
        }

        if (conditionList != null && !trialEnded && Time.time - sessionStartTime >= sessionDuration)
        {
            EndTrial();
        }
    }

    // 条件リストが設定されていれば、現在の条件で waveMode / actuatorScale を上書きする
    void ApplyCurrentCondition()
    {
        if (conditionList == null || !conditionList.HasCurrent) return;

        var condition = conditionList.Current;
        waveMode = condition.waveMode;
        actuatorScale = condition.actuatorScale;
    }

    // 実行時間が経過したら次の条件に進めて待機シーンへ遷移する
    void EndTrial()
    {
        trialEnded = true;
        CloseFiles();
        conditionList.Advance();
        SceneManager.LoadScene(waitSceneName);
    }

    // ── 緊急停止 ──────────────────────────────────────────────

    // Qキー長押し等の緊急停止から呼ばれる。アクチュエータを即座に停止し、
    // トライアルを中断して待機シーンへ戻る。条件リストは通常終了と同様に進める。
    public void EmergencyAbort()
    {
        if (trialEnded) return;
        trialEnded = true;

        actuatorController?.Shutdown();
        AppendEmergencyInfo();
        CloseFiles();
        conditionList?.Advance();
        SceneManager.LoadScene(waitSceneName);
    }

    // experiment_info.csv に緊急停止の情報を追記する（専用ファイルは作らない）
    void AppendEmergencyInfo()
    {
        if (string.IsNullOrEmpty(sessionDir)) return;

        string path = Path.Combine(sessionDir, "experiment_info.csv");
        string content = "emergencyStop,true\n"
            + $"emergencyStoppedAt,{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n"
            + $"emergencyElapsedSec,{(Time.time - sessionStartTime):F3}\n";
        File.AppendAllText(path, content, Encoding.UTF8);
    }

    void OnDestroy()      => CloseFiles();
    void OnApplicationQuit() => CloseFiles();

    void CloseFiles()
    {
        cubeLog?.Close();
        cameraLog?.Close();
        cubeLog   = null;
        cameraLog = null;
        DisconnectRotationLogger();
    }

    // ── 実回転角度ロガー（ラズパイ）への接続 ──────────────────
    // 接続直後にセッションフォルダ名を送る。ラズパイ側はこの名前で
    // real_rotation.csv を作成し、切断されるまでIMUの角度を書き込み続ける。
    void ConnectRotationLogger()
    {
        if (!useRotationLogger) return;

        try
        {
            rotationLoggerClient = new TcpClient(rotationLoggerHost, rotationLoggerPort);
            rotationLoggerStream = rotationLoggerClient.GetStream();
            byte[] bytes = Encoding.UTF8.GetBytes(Path.GetFileName(sessionDir) + "\n");
            rotationLoggerStream.Write(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("RotationLogger接続エラー: " + e.Message);
        }
    }

    // "END"を送るとラズパイ側がログ書き込みを止め、CSVの中身を送り返してくる。
    // それをこのセッションのフォルダに real_rotation.csv として保存してから接続を閉じる。
    void DisconnectRotationLogger()
    {
        if (!useRotationLogger) return;

        if (rotationLoggerStream != null)
        {
            try
            {
                byte[] endBytes = Encoding.UTF8.GetBytes("END\n");
                rotationLoggerStream.Write(endBytes, 0, endBytes.Length);

                rotationLoggerStream.ReadTimeout = 5000;
                string sizeLine = ReadLineFromStream(rotationLoggerStream);
                if (!string.IsNullOrEmpty(sizeLine) && int.TryParse(sizeLine, out int size) && size > 0)
                {
                    byte[] buffer = new byte[size];
                    int totalRead = 0;
                    while (totalRead < size)
                    {
                        int read = rotationLoggerStream.Read(buffer, totalRead, size - totalRead);
                        if (read <= 0) break;
                        totalRead += read;
                    }

                    if (totalRead == size && !string.IsNullOrEmpty(sessionDir))
                    {
                        string realRotationPath = Path.Combine(sessionDir, "real_rotation.csv");
                        File.WriteAllBytes(realRotationPath, buffer);
                        Debug.Log($"real_rotation.csv を受信しました（{totalRead}バイト）");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("RotationLogger終了処理エラー: " + e.Message);
            }
        }

        rotationLoggerStream?.Close();
        rotationLoggerClient?.Close();
        rotationLoggerStream = null;
        rotationLoggerClient = null;
    }

    // NetworkStreamから"\n"までの1行を読み取る
    string ReadLineFromStream(NetworkStream stream)
    {
        var sb = new StringBuilder();
        int b;
        while ((b = stream.ReadByte()) != -1)
        {
            if (b == '\n') break;
            sb.Append((char)b);
        }
        return sb.ToString();
    }

    // ── 設定の各コンポーネントへの反映 ────────────────────────

    void ApplySettings()
    {
        // 独立変数1: 波の高さ
        float waveMultiplier = waveMode switch
        {
            WaveMode.Half  => 0.5f,
            WaveMode.Third => 1f / 3f,
            _              => 1.0f,
        };
        if (wavesFormula != null)
        {
            wavesFormula.baseAmplitude  *= waveMultiplier;
            wavesFormula.noiseAmplitude *= waveMultiplier;
        }
        if (waveMesh != null)
        {
            waveMesh.baseAmplitude  *= waveMultiplier;
            waveMesh.noiseAmplitude *= waveMultiplier;
        }

        // 独立変数2: アクチュエータスケール
        if (actuatorController != null)
        {
            actuatorController.sendScale = actuatorScale;
            // Half条件では波が半分になりCubeの傾きも約半分になるため、
            // 送信角度をNormal条件と揃える補正を掛ける（tanの非線形分の誤差は数%許容）
            actuatorController.angleCompensation = 1f / waveMultiplier;
        }
    }

    // ── ログ用ディレクトリ・ファイル生成 ─────────────────────

    void CreateSessionDirectory()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string basePath  = Path.Combine(Application.dataPath, "Jarnal", "Result", "Log", "Cube");
        sessionDir = Path.Combine(basePath, $"{participantId}_{ConditionCode}_{timestamp}");
        Directory.CreateDirectory(sessionDir);
    }

    void WriteExperimentInfo()
    {
        string infoPath = Path.Combine(sessionDir, "experiment_info.csv");
        var sb = new StringBuilder();
        sb.AppendLine("key,value");
        sb.AppendLine($"participantId,{participantId}");
        sb.AppendLine($"conditionCode,{ConditionCode}");
        sb.AppendLine($"waveMode,{waveMode}");
        sb.AppendLine($"actuatorScale,{actuatorScale}");
        sb.AppendLine($"sessionStart,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        File.WriteAllText(infoPath, sb.ToString(), Encoding.UTF8);
    }

    void OpenLogFiles()
    {
        string cubePath   = Path.Combine(sessionDir, "cube_position.csv");
        string cameraPath = Path.Combine(sessionDir, "camerarig_position.csv");

        cubeLog = new StreamWriter(cubePath, false, Encoding.UTF8);
        cubeLog.WriteLine("time_sec,world_x,world_y,world_z,world_rot_x,world_rot_y,world_rot_z,local_rot_x,local_rot_y,local_rot_z");

        cameraLog = new StreamWriter(cameraPath, false, Encoding.UTF8);
        cameraLog.WriteLine("time_sec,world_x,world_y,world_z,local_x,local_y,local_z,world_rot_x,world_rot_y,world_rot_z,local_rot_x,local_rot_y,local_rot_z");
    }

    // ── ログ書き込み ──────────────────────────────────────────

    void WriteLogs()
    {
        float t = Time.time - sessionStartTime;

        if (cubeTransform != null && cubeLog != null)
        {
            Vector3 p = cubeTransform.position;
            Vector3 worldRot = cubeTransform.eulerAngles;
            Vector3 localRot = cubeTransform.localEulerAngles;
            cubeLog.WriteLine($"{t:F3},{p.x:F4},{p.y:F4},{p.z:F4},{worldRot.x:F4},{worldRot.y:F4},{worldRot.z:F4},{localRot.x:F4},{localRot.y:F4},{localRot.z:F4}");
        }

        if (cameraRigTransform != null && cameraLog != null)
        {
            Vector3 world = cameraRigTransform.position;
            Vector3 worldRot = cameraRigTransform.eulerAngles;
            Vector3 local, localRot;
            if (cubeTransform != null)
            {
                local = cubeTransform.InverseTransformPoint(world);
                localRot = (Quaternion.Inverse(cubeTransform.rotation) * cameraRigTransform.rotation).eulerAngles;
            }
            else
            {
                local = world;
                localRot = worldRot;
            }
            cameraLog.WriteLine($"{t:F3},{world.x:F4},{world.y:F4},{world.z:F4},{local.x:F4},{local.y:F4},{local.z:F4},{worldRot.x:F4},{worldRot.y:F4},{worldRot.z:F4},{localRot.x:F4},{localRot.y:F4},{localRot.z:F4}");
        }
    }
}
