using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class WeightController : MonoBehaviour
{
    private TcpClient mySocket;
    private NetworkStream theStream;
    private StreamReader theReader;
    private bool socketReady = false;
    private Thread receiveThread;
    private string receivedMessage = ""; // 受信したメッセージを格納する

    private float latestValue = 0.0f;
    private object lockObject = new object();

    public string Host = "192.168.16.3"; // サーバーのIP
    public int Port = 12346; // サーバーのポート番号

    void Start()
    {
        SetupSocket();
    }

    void SetupSocket()
    {
        try
        {
            mySocket = new TcpClient(Host, Port);
            theStream = mySocket.GetStream();
            theReader = new StreamReader(theStream, Encoding.UTF8);
            socketReady = true;

            Debug.Log("Socket connected. Waiting for messages...");

            // 受信専用スレッドを作成して開始
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Socket error: " + e.Message);
        }
    }

    void ReceiveData()
    {
        try
        {
            while (socketReady)
            {
                string message = theReader.ReadLine(); // 1行ずつ受信
                Debug.Log("Received: " + message);
                if (!string.IsNullOrEmpty(message))
                {
                    if (float.TryParse(message, out float value))
                    {
                        lock (lockObject)
                        {
                            latestValue = value;
                        }
                    }
                    receivedMessage = message; // 受信データを格納
                    Debug.Log("Received: " + message);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Receive error: " + e.Message);
        }
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(receivedMessage))
        {
            Debug.Log("Processing message in Update(): " + receivedMessage);
            receivedMessage = ""; // メッセージをリセット
        }
    }

    void OnApplicationQuit()
    {
        CloseSocket();
    }

    void CloseSocket()
    {
        if (socketReady)
        {
            socketReady = false;
            // スレッドが完全に終了するのを待つ
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(); // 安全に待機して終了
            }

            if (theReader != null) theReader.Close();
            if (theStream != null) theStream.Close();
            if (mySocket != null) mySocket.Close();
            Debug.Log("Socket closed.");
        }
    }

/*    public float GetLatestValue()
    {
        if (float.TryParse(theReader.ReadLine(), out float value))
        {
            //return Mathf.Clamp(value, 0.0f, maxSensorValue);
            return value;
        }
        else
        {
            return 0.0f;
        }
    }*/

    public float GetLatestValue()
    {
        lock (lockObject)
        {
            return latestValue;
        }
    }
}
