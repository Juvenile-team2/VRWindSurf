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
                if (!string.IsNullOrEmpty(message))
                {
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
            theReader.Close();
            theStream.Close();
            mySocket.Close();
            receiveThread.Abort();
            Debug.Log("Socket closed.");
        }
    }

    public float GetLatestValue(float maxSensorValue)
    {
        if(float.TryParse(theReader.ReadLine(), out float value))
        {
            return Mathf.Clamp(value, 0.0f, maxSensorValue);
        }
        else
        {
            return 0.0f;
        }
    }
}
