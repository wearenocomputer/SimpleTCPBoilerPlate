using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections;

public class ncTCPServer : MonoBehaviour
{
    // Start is called before the first frame update

    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private TcpClient connectedTcpClient;
    private bool isRunning = false;
    public string ipAddress = "127.0.0.1";
    public string port = "8052";
    private string clientasks="";

    void Start()
    {
        StartServer();
        StartCoroutine(onCoroutine());
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessage();
        }
    }

    private void OnDestroy()
    {
        StopServer();

    }

    private void OnApplicationQuit()
    {
        StopServer();

    }
    private void StopServer()
    {
        isRunning = true;

        if (connectedTcpClient != null)
        {
            connectedTcpClient.Close();
            connectedTcpClient = null;
        }

            if (tcpListenerThread != null)
        {
            tcpListenerThread.Abort();
        }

        if (tcpListener != null)
        {
            tcpListener.Stop();
            tcpListener = null;
        }
    }

    private void StartServer()
    {
        // Start TcpServer background thread 		
        isRunning = true;
        tcpListenerThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();
    }

    void ListenForIncommingRequests()
    {
       
            try
            {
                tcpListener = new TcpListener(IPAddress.Parse(ipAddress), int.Parse(port));
                tcpListener.Start();
                while (isRunning)
                {
                    Byte[] bytes = new Byte[1024];
                    Debug.Log("waiting for client");
                    connectedTcpClient = tcpListener.AcceptTcpClient();
                    NetworkStream stream = connectedTcpClient.GetStream();
                    Debug.Log("client connected");
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        string clientMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("client message received as: " + clientMessage);
                        clientasks = clientMessage;
                        Thread.Sleep(1);
                    }
                    Debug.Log("client is gone");
                    connectedTcpClient.Close();
                    stream.Close();   
                }
            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);     
            }
        
    }

    private void SendMessage()
    {
        if (connectedTcpClient == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();
            if (stream.CanWrite)
            {
                string serverMessage = "";
                if (clientasks == "INEEDNORMALDATA")
                {
                    serverMessage = "HEREISYOURNORMALDATA";
                }

                if (clientasks == "INEEDSPECIALDATA")
                {
                    serverMessage = "HEREISYOURSPECIALDATA";
                }
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
               
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    IEnumerator onCoroutine()
    {
        while (true)
        {
            SendMessage();
            yield return new WaitForSeconds(0.05f);
        }
    }

}
