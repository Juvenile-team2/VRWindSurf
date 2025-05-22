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
    private string receivedMessage = ""; // ��M�������b�Z�[�W���i�[����

    private float latestValue = 0.0f;
    private object lockObject = new object();

    public string Host = "192.168.16.3"; // �T�[�o�[��IP
    public int Port = 12346; // �T�[�o�[�̃|�[�g�ԍ�

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

            // ��M��p�X���b�h���쐬���ĊJ�n
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
                string message = theReader.ReadLine(); // 1�s����M
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
                    receivedMessage = message; // ��M�f�[�^���i�[
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
            receivedMessage = ""; // ���b�Z�[�W�����Z�b�g
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
            // �X���b�h�����S�ɏI������̂�҂�
            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(); // ���S�ɑҋ@���ďI��
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
