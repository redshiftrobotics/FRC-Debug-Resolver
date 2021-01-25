using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotResolver : MonoBehaviour {
    private Rigidbody rb;
    private Network network;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    public WheelCollider frontRightWheel;
    public WheelCollider middleRightWheel;
    public WheelCollider backRightWheel;
    public WheelCollider frontLeftWheel;
    public WheelCollider middleLeftWheel;
    public WheelCollider backLeftWheel;

    public Light powerLight;

    private WheelCollider[] colliders;

    public bool reset = false;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        network = GetComponent<Network>();

        colliders = new WheelCollider[6] { frontRightWheel,
            middleRightWheel,
            backRightWheel,
            frontLeftWheel,
            middleLeftWheel,
            backLeftWheel
        };

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update() {

        UpdateWheelMesh();

        if (reset) {
            Reset();
            reset = false;
        }

        powerLight.enabled = network.netVars.powered;

        // If we aren't in an opmode or powered, stop everything
        if (!network.netVars.powered
            || network.netVars.state == 0
            || network.netVars.state == 1
            || network.netVars.state == 3) {

            SetPower(0, 0);
            return;
        }
        //Translate(network.inputMovement);
        //Rotate(network.inputRotation);

        SetPower(
            Mathf.Clamp(network.netVars.motor0, -1, 1),
            Mathf.Clamp(network.netVars.motor1, -1, 1)
        );


    }

    public void Reset() {
        transform.position = new Vector3(Random.Range(1, 9), 0, Random.Range(1, 9));
        transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

        network.SetState(0);
    }

    private void SetPower(float left, float right) {
        frontRightWheel.motorTorque = right * network.maxMotorTorque;
        middleRightWheel.motorTorque = right * network.maxMotorTorque;
        backRightWheel.motorTorque = right * network.maxMotorTorque;

        frontLeftWheel.motorTorque = left * network.maxMotorTorque;
        middleLeftWheel.motorTorque = left * network.maxMotorTorque;
        backLeftWheel.motorTorque = left * network.maxMotorTorque;
    }

    public Vector3 GetRPM() {
        return new Vector3(middleLeftWheel.rpm, middleRightWheel.rpm);
    }


    private void UpdateWheelMesh() {
        for (int i = 0; i < 6; i++) {
            WheelCollider collider = colliders[i];
            Transform visualWheel = collider.transform.GetChild(0);

            Vector3 position;
            Quaternion rotation;
            collider.GetWorldPose(out position, out rotation);

            visualWheel.transform.position = position;
            visualWheel.transform.rotation = rotation;
        }

    }
}
