using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WheelPlacer : MonoBehaviour
{
    public List<WheelCollider> wheels = new List<WheelCollider>();


    [Range(0, 0.5f)]
    public float offsetX;
    [Range(-0.5f, 0.5f)]
    public float offsetY;
    [Range(0, 0.5f)]
    public float offsetZ;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        GetComponentsInChildren(wheels);

        // Back Right
        wheels[0].transform.localPosition = new Vector3(offsetX, offsetY, -offsetZ);

        // Back Left
        wheels[1].transform.localPosition = new Vector3(-offsetX, offsetY, -offsetZ);

        // Middle Right
        wheels[2].transform.localPosition = new Vector3(offsetX, offsetY, 0);

        // Middle Left
        wheels[3].transform.localPosition = new Vector3(-offsetX, offsetY, 0);

        // Front Right
        wheels[4].transform.localPosition = new Vector3(offsetX, offsetY, offsetZ);

        // Front Left
        wheels[5].transform.localPosition = new Vector3(-offsetX, offsetY, offsetZ);

    }
}
