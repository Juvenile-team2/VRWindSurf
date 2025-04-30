using UnityEngine;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class WindMoveController : MonoBehaviour
{
    private TcpClient mySocket;
    private NetworkStream theStream;
    private StreamReader theReader;
    private bool socketReady = false;
    private Thread receiveThread;
    private string receivedMessage = ""; // ��M�������b�Z�[�W���i�[����

    private Vector3 latestValue = Vector3.zero;
    private object lockObject = new object();

    public string Host = "192.168.16.11"; // �T�[�o�[��IP
    public int Port = 12345; // �T�[�o�[�̃|�[�g�ԍ�

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
            var writer = new StreamWriter(theStream, Encoding.UTF8) { AutoFlush = true };

            // �N���C�A���g�o�^���b�Z�[�W���M
            writer.WriteLine("REGISTER:ClientB");

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
                if (!string.IsNullOrEmpty(message))
                {
                    Vector3 value = ParseVector3(message);

                    lock (lockObject)
                    {
                        latestValue = value;
                        Debug.Log("Wind Get: " +  latestValue);

                    }
                    receivedMessage = message;
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
            theReader.Close();
            theStream.Close();
            mySocket.Close();
            receiveThread.Abort();
            Debug.Log("Socket closed.");
        }
    }

    public Vector3 GetLatestValue()
    {
        lock (lockObject)
        {
            return latestValue;
        }
    }

    private Vector3 ParseVector3(string input)
    {
        try
        {
            input = input.Trim('(', ')'); // ���ʂ�����
            string[] parts = input.Split(',');

            if (parts.Length != 3) return Vector3.zero;

            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);

            return new Vector3(x, y, z);
        }
        catch
        {
            Debug.LogWarning("Failed to parse Vector3: " + input);
            return Vector3.zero;
        }
    }

}
