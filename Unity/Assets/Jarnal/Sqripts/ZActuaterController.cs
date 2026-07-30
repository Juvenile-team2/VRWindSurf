using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ZActuatorController : MonoBehaviour
{
    bool socketReady = false;
    TcpClient mySocket;
    public NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;
    public String Host = "192.168.16.6";
    public Int32 Port = 12345;
    public GameObject targetObject;  // 対象のGameObject
    public String messageToSend = "0";
    public float sendInterval = 0.1f;
    [Range(0f, 100f)]
    public float sendScale = 100f;          // 送信値のスケール（%）
    [Tooltip("角度差分に掛ける補正係数。Half条件で波が半分になっても送信値をNormalと揃えるために使う")]
    public float angleCompensation = 1f;
    private float previousZAngle = 0f;      // 前回のZ角度

    void Start()
    {
        setupSocket();
        previousZAngle = GetZAngle();  // 初期角度取得
        InvokeRepeating("SendMessage", 0f, sendInterval);
    }

    void OnDestroy()      => Shutdown();
    void OnApplicationQuit() => Shutdown();

    // シーン切り替え・終了時に、送信ループを止めてアクチュエータへ停止値を送ってから接続を閉じる
    // 緊急停止からも呼ばれるため public
    public void Shutdown()
    {
        CancelInvoke("SendMessage");

        if (socketReady)
        {
            try
            {
                byte[] stopBytes = Encoding.UTF8.GetBytes("0.00");
                theStream.Write(stopBytes, 0, stopBytes.Length);
            }
            catch (Exception e)
            {
                Debug.LogError("Stop send error: " + e.Message);
            }
        }

        theWriter?.Close();
        theReader?.Close();
        theStream?.Close();
        mySocket?.Close();
        socketReady = false;
    }

    void SendMessage()
    {
        //if (socketReady && theWriter != null)
        //{
            try
            {
                // 現在のZ軸回転取得
                float currentZAngle = GetZAngle();

                // 差分計算（-180〜180度範囲内での差）
                float angleDiff = Mathf.DeltaAngle(previousZAngle, currentZAngle);

                // -50〜50 にマッピング（最大180度の時 ±50 になるようにスケーリング）
                float mappedDiff = Mathf.Clamp(angleDiff * angleCompensation * 100 / 2.4f, -500f, 500f) * (sendScale / 100f);

                // メッセージ送信
                messageToSend = mappedDiff.ToString("F2");
                byte[] sendBytes = Encoding.UTF8.GetBytes(messageToSend);
                theStream.Write(sendBytes, 0, sendBytes.Length);
                Debug.Log($"Sent diff: {messageToSend} (angleDiff: {angleDiff:F2})");


                // 今回の角度を保存
                previousZAngle = currentZAngle;
            }
            catch (Exception e)
            {
                Debug.LogError("Send error: " + e.Message);
            }
        //}
    }

    float GetZAngle()
    {
        return targetObject.transform.eulerAngles.z;
    }

    public void setupSocket()
    {
        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theWriter = new StreamWriter(theStream);
            theReader = new StreamReader(theStream);
            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket error:" + e);
        }
    }
}

