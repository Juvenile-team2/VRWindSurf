using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class ActuatorController : MonoBehaviour
{
    bool socketReady = false;
    TcpClient mySocket;
    public NetworkStream theStream;
    StreamWriter theWriter;
    StreamReader theReader;
    public String Host = "192.168.16.5";
    public Int32 Port = 12345;
    public GameObject targetObject;  // 対象のGameObject
    public String messageToSend = "0";
    public float sendInterval = 1f / 60f;  // 60fpsで送信
    private float previousZAngle = 0f;      // 前回のZ角度

    void Start()
    {
        setupSocket();
        previousZAngle = GetZAngle();  // 初期角度取得
        InvokeRepeating("SendMessage", 0f, sendInterval);
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
                float mappedDiff = Mathf.Clamp(angleDiff / 3.6f, -50f, 50f) * 300;

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

