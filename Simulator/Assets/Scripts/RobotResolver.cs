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

    private Vector3 rightPreviousPosition;
    private Vector3 leftPreviousPosition;

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

        rb.centerOfMass = new Vector3(0,-0.05f,0);

        UpdateWheelMesh();

        if (reset) {
            Reset();
            reset = false;
        }

        //powerLight.enabled = network.netVars.powered;
        Material mat = powerLight.GetComponent<MeshRenderer>().material;
        mat.EnableKeyword("_EMISSION");
        //mat.SetColor("_EmissiveColor", new Color(255, 107, 0) *);
        //mat.SetFloat("_EmissiveExposureWeight", );
        mat.SetColor("_EmissionColor", new Vector4(255, 107, 0, 0) * (network.netVars.powered ? 0.05f : 0));

        if (network.netVars.manual_control) {
            SetPower(network.netVars.joystick0_stick0.y, network.netVars.joystick0_stick0.y);
            return;
        }


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
        transform.position = new Vector3(0,0.1f,0);
        transform.eulerAngles = Vector3.zero;
        rb.velocity = Vector3.zero;

        network.netVars.encoder0 = 0;
        network.netVars.encoder1 = 0;

        network.SetState(0);
    }

    private void SetPower(float left, float right) {
        if (Mathf.Abs(right) < 0.01f)
            right = 0;

        if (Mathf.Abs(left) < 0.01f)
            left = 0;


        Vector3 rightSpeed = (middleRightWheel.transform.position - rightPreviousPosition) / Time.deltaTime;
        rightPreviousPosition = middleRightWheel.transform.position;

        Vector3 leftSpeed = (middleLeftWheel.transform.position - leftPreviousPosition) / Time.deltaTime;
        leftPreviousPosition = middleLeftWheel.transform.position;



        // Parent right
        if (rightSpeed.magnitude <= network.maxSpeed)
            middleRightWheel.motorTorque += right * network.maxMotorTorque * Time.deltaTime;

        // Parent left
        if (rightSpeed.magnitude <= network.maxSpeed)
            middleLeftWheel.motorTorque += left * network.maxMotorTorque * Time.deltaTime;
        
        
        frontRightWheel.motorTorque = middleRightWheel.motorTorque;
        backRightWheel.motorTorque = middleRightWheel.motorTorque;

        frontLeftWheel.motorTorque = middleLeftWheel.motorTorque;
        backLeftWheel.motorTorque = middleLeftWheel.motorTorque;

        // rb.AddForceAtPosition(middleRightWheel.transform.forward * right * network.maxMotorTorque, middleRightWheel.transform.position);
        // rb.AddForceAtPosition(middleLeftWheel.transform.forward * right * network.maxMotorTorque, middleLeftWheel.transform.position);
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
