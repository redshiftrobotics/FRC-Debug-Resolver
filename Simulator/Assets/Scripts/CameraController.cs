using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    int type = 0;
    int maxViews = 2;

    private Camera camera;
    private RobotResolver robotResolver;

    public Vector3 offsetPosition;

    // Start is called before the first frame update
    void Start() {
        camera = GetComponent<Camera>();
        robotResolver = FindObjectsOfType<RobotResolver>()[0];
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (type) {
            case 0:
                TopDown();
                break;
            case 1:
                Follow();
                break;
            default:
                TopDown();
                break;
        }
    }

    void TopDown() {
        camera.orthographic = true;
        transform.position = new Vector3(5, 5, 5);
        transform.eulerAngles = new Vector3(90, 0, 0);
    }

    void Follow() {
        camera.orthographic = false;
        Transform r = robotResolver.transform;
        Vector3 target = r.position + r.forward * offsetPosition.z + r.right * offsetPosition.x + Vector3.up * offsetPosition.y;
        transform.position = Vector3.Lerp(transform.position, target, 0.1f);
        transform.LookAt(robotResolver.transform.position);
    }

    public void ChangeView() {
        type = (type + 1) % maxViews;
    }
}
