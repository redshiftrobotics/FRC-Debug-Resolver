using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine;

using Leguar.TotalJSON;

enum State {
	NONE = 0,
	AUTO_INITIALIZE = 1,
	AUTO_LOOP = 2,
	TELEOP_INITIALIZE = 3,
	TELEOP_LOOP = 4
};

// Networked output variables
[Serializable]
public class NetworkVariables {

	// World stuff (cheating if you use this)
	public Vector2 world_position;
	public float world_rotation;

	// State of the robot, init, teleop, auto, etc
	public int state;
	public bool powered;
	public bool manual_control;

	// Encoders
	public int encoder0;
	public int encoder1;

	// Motors
	public float motor0;
	public float motor1;

	public Vector2 joystick0_stick0;
	public Vector2 joystick0_stick1;

}

[Serializable]
public class OutputMessage {
	public string header;
	public dynamic value;

	public OutputMessage(string _header, dynamic _value) {
		header = _header;
		value = _value;
	}
}

// Network MonoBehaviour
public class Network : MonoBehaviour {
	[Header("Settings")]
	public float maxMotorTorque = 100;
	public float maxSpeed = 3.743381f;

	[Header("Networked Variables")]
	public NetworkVariables netVars;

	// Reference to the client
	private TcpClient connectedTcpClient;
	// Our listener and thread
	private TcpListener tcpListener;
	private Thread tcpListenerThread;

	// Instance this shit because its not static
	private RobotResolver robotResolver;
	private CameraController cameraController;
	private Log[] logs;

	void Start() {
		// Grab local variables
		robotResolver = GetComponent<RobotResolver>();
		cameraController = FindObjectOfType<CameraController>();
		logs = FindObjectsOfType<Log>();

		// Initialize server
		tcpListenerThread = new Thread(new ThreadStart(ListenForRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();

		// Start coroutine for sending output messages
		// StartCoroutine(SendOutput());
	}

	void Update() {
		
		// Temporary. The input will be set from the python socket
		// inputVars.movement.x = Input.GetAxis("Horizontal");
		// inputVars.movement.y = Input.GetAxis("Vertical");

		// Capture 2D position
		netVars.world_position.x = robotResolver.transform.position.x;
		netVars.world_position.y = robotResolver.transform.position.z;

		// Capture yaw rotation
		netVars.world_rotation = robotResolver.transform.eulerAngles.y;

		// Capture left and right encoder values
		netVars.encoder0 += (int)(robotResolver.GetRPM().x * Time.deltaTime * 60);
		netVars.encoder1 += (int)(robotResolver.GetRPM().y * Time.deltaTime * 60);

		netVars.joystick0_stick0 = Gamepad.current.leftStick.ReadValue();
		netVars.joystick0_stick1 = Gamepad.current.rightStick.ReadValue();


	}

	// Runs in another thread and starts listening for requests to the server
	private void ListenForRequests() {
		AddLog("Started Listener thread", LogType.MESSAGE);


		// Create listener on localhost port 8052. 			
		tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8052);
		tcpListener.Start();


		while (true) {
			AddLog("Waiting for connection on port 8052", LogType.WARNING);
			// Accept the client
			connectedTcpClient = tcpListener.AcceptTcpClient();

			if (!connectedTcpClient.Connected) {
				OnDisconnect();
				continue;
			}

			AddLog("Connected", LogType.MESSAGE);

			// Fetch the stream
			NetworkStream stream = connectedTcpClient.GetStream();

			// Reference to the bytes received (sizeof 1024)
			Byte[] readBytes = new Byte[1024];

			// Listens while connected
			while (connectedTcpClient != null && connectedTcpClient.Connected) {
				int length;
				// Read incoming stream into byte array. 

				try {
					while ((length = stream.Read(readBytes, 0, readBytes.Length)) != 0) {
						byte[] incomingData = new byte[length];

						// Copies the buffer into our (correctly sized) array
						Array.Copy(readBytes, 0, incomingData, 0, length);

						// ReceivedMessage callback
						OnReceivedMessage(incomingData);
					}
				}
				catch (Exception e) {
					// OnDisconnect();
					AddLog(e.Message, LogType.ERROR);
					continue;
				}
			}
		}
	}

	private void SendMessage(object message) {

		string strMessage = JSON.Serialize(message).CreateString();
		List<byte> bytes = new List<byte>(Encoding.ASCII.GetBytes(strMessage));

		// Add terminator
		bytes.Add(0x00);

		byte[] byteArr = bytes.ToArray();


		if (connectedTcpClient == null || !connectedTcpClient.Connected)
			return;

		try {
			// Get a stream object for writing. 			
			NetworkStream stream = connectedTcpClient.GetStream();

			if (!stream.CanWrite)
				return;

			// Write byte array to socketConnection stream.               
			stream.Write(byteArr, 0, byteArr.Length);

		}
		catch (SocketException socketException) {
			Debug.Log("Socket exception: " + socketException);
		}
	}

	private void OnDisconnect() {
		connectedTcpClient.GetStream().Close();
		connectedTcpClient.Close();
		connectedTcpClient = null;

		AddLog("Disconnected", LogType.MESSAGE);

		netVars.motor0 = 0;
		netVars.motor1 = 0;
		netVars.world_rotation = 0;

		robotResolver.reset = true;
	}

	private void OnAddOverlayPoint(JSON overlayStruct) {

		OverlayPoint point = JsonUtility.FromJson<OverlayPoint>(overlayStruct.CreateString());
		point.color.a = 1;

		// Transpose top down
		float z = point.center.z;
		point.center.z = point.center.y;
		point.center.y = z;

		// Send responce
		SendMessage(new OutputMessage("added_overlay_point", point.center));

		// Draw the point
		cameraController.AddOverlayPoint(point);
	}

	private void OnAddOverlayLine(JSON overlayStruct) {

		OverlayLine line = JsonUtility.FromJson<OverlayLine>(overlayStruct.CreateString());
		line.color.a = 1;

		// Transpose top down
		float z = line.start.z;
		line.start.z = line.start.y;
		line.start.y = z;

		z = line.end.z;
		line.end.z = line.end.y;
		line.end.y = z;

		// Send responce
		SendMessage(new OutputMessage("added_overlay_line", line.start));

		// Draw the point
		cameraController.AddOverlayLine(line);
	}

	private void OnReceivedMessage(byte[] bytes) {
		// Convert byte array to string message. 							
		string stringMessage = Encoding.ASCII.GetString(bytes);
		JSON message = JSON.ParseString(stringMessage);

		string header = message.GetString("header");

		switch (header) {
			case "request":
				RequestVariable(message.GetString("variable"));
				break;

			case "set_variable":
				SetVariable(message.GetString("variable"), message.GetJNumber("value").AsFloat());
				break;

			case "add_overlay_point":
				OnAddOverlayPoint(message.GetJSON("value"));
				break;

			case "add_overlay_line":
				OnAddOverlayLine(message.GetJSON("value"));
				break;

			case "disconnect":
				OnDisconnect();
				break;

			default:
				break;
		}

	}

	// This is shit. Never use this. If you do I will find your address
	public System.Reflection.FieldInfo GetVariable(string name) {
		System.Reflection.FieldInfo variable;
		try {
			variable = netVars.GetType().GetField(name);
			return variable;
		}
		catch (System.NullReferenceException) {
			return null;
		}
	}

	// This is not so shit
	private void RequestVariable(string name) {
		dynamic variable = netVars.GetType().GetField(name).GetValue(netVars);

		OutputMessage message = new OutputMessage(name, variable);

		if (variable != null)
			SendMessage(message);
	}

	// This not shit
	private void SetVariable(string name, dynamic value) {

		// What type the variable should be
		dynamic lastVar = netVars.GetType().GetField(name).GetValue(netVars);
		Type type = lastVar.GetType();

		// Change it to the correct type
		dynamic typedValue = Convert.ChangeType(value, type);

		// Set it
		netVars.GetType().GetField(name).SetValue(netVars, typedValue);

		OutputMessage message = new OutputMessage("set", netVars.GetType().GetField(name).GetValue(netVars));

		// Return what it was set to
		SendMessage(message);
	}

	public void SetState(int _state) {
		State toChange = (State)_state;

		if (toChange == State.AUTO_LOOP && (State)netVars.state != State.AUTO_INITIALIZE) {
			AddLog("Initialize first", LogType.ERROR);
			return;
		}

		if (toChange == State.TELEOP_LOOP && (State)netVars.state != State.TELEOP_INITIALIZE) {
			AddLog("Initialize first", LogType.ERROR);
			return;
		}

		netVars.state = _state;

		State enumState = (State)netVars.state;
		AddLog("Set state to " + enumState.ToString(), LogType.MESSAGE);
	}

	public void TogglePower() {
		netVars.powered = !netVars.powered;

		AddLog("Powered " + (netVars.powered ? "ON" : "OFF"), LogType.WARNING);
	}

	public void AddLog(string message, LogType type) {
		for (int i = 0; i < logs.Length; i++) {
			logs[i].Add(message, type);
		}
	}

	public void ToggleManualControl() {
		netVars.manual_control = !netVars.manual_control;
	}
}

