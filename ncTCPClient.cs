using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ncTCPClient : MonoBehaviour {

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    public string ipAddress = "127.0.0.1";
    public string port = "8052";
    private bool isConnected = false;
    private bool isRunning = false;
    public string client_message = "INEEDNORMALDATA";
    // Start is called before the first frame update
    void Start()
    {
        StartClient();
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

    private void StartClient()
    {
     
        isRunning = true;
        isConnected = false;
        clientReceiveThread = new Thread(new ThreadStart(ListenForIncommingRequests));
        clientReceiveThread.IsBackground = true;
        clientReceiveThread.Start();

    }

    private void StopClient()
    {

        if (socketConnection != null)
        {
            socketConnection.Close();

        }

        if (clientReceiveThread != null)
        {
            clientReceiveThread.Abort();

        }
        isConnected = false;
        isRunning = false;
    }
    private void ListenForIncommingRequests()
    {

        while (!isConnected)
        {
            try
            {
                Debug.Log("looking for server");
                socketConnection = new TcpClient(ipAddress, int.Parse(port));
                Debug.Log("server found");
                isConnected = true;
                Byte[] bytes = new Byte[1024];
                while (isRunning)
                {
                    NetworkStream stream = socketConnection.GetStream();
                    //THIS IS READING
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {      
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        string clientMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("server message received as: " + clientMessage);
                        Thread.Sleep(1);
                    }


                    Debug.Log("Where is the server?");
                    isConnected = false;
                    isRunning = false;
                    stream.Close();

                }

            }
            catch (SocketException socketException)
            {
                Debug.Log("Socket exception: " + socketException);
                Thread.Sleep(1);
            }

        }
    }


    private void OnDestroy()
    {
        StopClient();
    }

    private void OnApplicationQuit()
    {
        StopClient();

    }


    private void SendMessage()
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = client_message;
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
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
