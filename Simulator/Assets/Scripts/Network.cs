using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

// Networked output variables
[Serializable]
public class OutputNetworkVars
{
    public Vector2 outputWorldPosition;
    public float outputWorldRotation;
    public Vector2 outputEncoderCount;
}

// Networked input variables
[Serializable]
public class InputNetworkVars
{
    public Vector3 movement;
    public Vector3 rotation;
}


// Network MonoBehaviour
public class Network : MonoBehaviour
{
    [Header("Settings")]
    public float maxMotorTorque = 100;

    [Header("Networked Variables")]
    public OutputNetworkVars outputVars;
    public InputNetworkVars inputVars;

    // Reference to the client
    private TcpClient connectedTcpClient;
    // Our listener and thread
    private TcpListener tcpListener;
    private Thread tcpListenerThread;

    // Instance this shit because its not static
    private RobotResolver robotResolver;

    void Start()
    {
        // Grab local variables
        robotResolver = GetComponent<RobotResolver>();

        // Initialize server
        tcpListenerThread = new Thread(new ThreadStart(ListenForRequests));
        tcpListenerThread.IsBackground = true;
        tcpListenerThread.Start();

        // Start coroutine for sending output messages
        StartCoroutine(SendOutput());
    }

    void Update()
    {
        // Temporary. The input will be set from the python socket
        // inputVars.movement.x = Input.GetAxis("Horizontal");
        // inputVars.movement.y = Input.GetAxis("Vertical");

        // Capture 2D position
        outputVars.outputWorldPosition.x = robotResolver.transform.position.x;
        outputVars.outputWorldPosition.y = robotResolver.transform.position.z;

        // Capture yaw rotation
        outputVars.outputWorldRotation = robotResolver.transform.eulerAngles.y;

        // Capture left and right encoder values
        outputVars.outputEncoderCount.x += (int)(robotResolver.GetRPM().x * Time.deltaTime * 60);
        outputVars.outputEncoderCount.y += (int)(robotResolver.GetRPM().y * Time.deltaTime * 60);

        //SendMessage(outputVars);
    }

    // Runs in another thread and starts listening for requests to the server
    private void ListenForRequests()
    {
        Debug.Log("Started Listener thread");


        // Create listener on localhost port 8052. 			
        tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8052);
        tcpListener.Start();


        while (true)
        {
            Debug.Log("Waiting for connection on port 8052");
            // Accept the client
            connectedTcpClient = tcpListener.AcceptTcpClient();

            if (!connectedTcpClient.Connected)
            {
                OnDisconnect();
                continue;
            }

            Debug.Log("Connected");

            // Fetch the stream
            NetworkStream stream = connectedTcpClient.GetStream();

            // Reference to the bytes received (sizeof 1024)
            Byte[] readBytes = new Byte[1024];

            // Listens while connected
            while (connectedTcpClient.Connected)
            {
                int length;
                // Read incoming stream into byte array. 

                try
                {
                    while ((length = stream.Read(readBytes, 0, readBytes.Length)) != 0)
                    {
                        byte[] incomingData = new byte[length];

                        // Copies the buffer into our (correctly sized) array
                        Array.Copy(readBytes, 0, incomingData, 0, length);

                        // ReceivedMessage callback
                        OnReceivedMessage(incomingData);
                    }
                }
                catch (Exception e)
                {
                    OnDisconnect();
                    // Debug.Log(e.ToString());
                    continue;
                }
            }
        }
    }

    IEnumerator SendOutput()
    {
        while (true)
        {
            SendMessage(outputVars);
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void SendMessage(object message)
    {

        string strMessage = JsonUtility.ToJson(message);
        byte[] bytes = Encoding.ASCII.GetBytes(strMessage);

        if (connectedTcpClient == null || !connectedTcpClient.Connected)
            return;

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedTcpClient.GetStream();

            if (!stream.CanWrite)
                return;

            // Write byte array to socketConnection stream.               
            stream.Write(bytes, 0, bytes.Length);

        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void OnDisconnect()
    {
        Debug.Log("Disconnected");

        inputVars.movement = new Vector3(0, 0);
        inputVars.rotation = new Vector3(0, 0);
        robotResolver.reset = true;
    }

    private void OnReceivedMessage(byte[] bytes)
    {
        // Convert byte array to string message. 							
        string stringMessage = Encoding.ASCII.GetString(bytes);

        if (stringMessage.Contains("}{"))
            return;

        InputNetworkVars message = JsonUtility.FromJson<InputNetworkVars>(stringMessage);

        inputVars = message;
    }
}
