using System;
using System.Collections.Generic;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;

[Serializable]
public class OverlayLine {
    public string id;
    public Vector3 start;
    public Vector3 end;
    public Color color;

    public OverlayLine(Vector3 _start, Vector3 _end, Color _color) {
        start = _start;
        end = _end;
        color = _color;
    }
}

[Serializable]
public class OverlayPoint {
    public string id;
    public Vector3 center;
    public float radius;
    public Color color;

    public OverlayPoint(Vector3 _center, float _radius, Color _color) {
        center = _center;
        radius = _radius;
        color = _color;
    }
}

public class CameraController : MonoBehaviour {
    int type = 0;
    int maxViews = 3;

    private Camera cameraComponent;
    private RobotResolver robotResolver;

    public Vector3 offsetPosition;

    [Header("Drawing")]
    private Material overlayMaterial;
    public List<OverlayPoint> overlayPoints = new List<OverlayPoint>();
    public List<OverlayLine> overlayLines = new List<OverlayLine>();

    // Start is called before the first frame update
    void Start() {
        cameraComponent = GetComponent<Camera>();
        robotResolver = FindObjectsOfType<RobotResolver>()[0];
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (type) {
            case 0:
                TopDown();
                break;
            case 1:
                CenteredTopDown();
                break;
            case 2:
                Follow();
                break;
            default:
                TopDown();
                break;
        }
    }

    void TopDown() {
        Vector3 target = new Vector3(5, 5, 5);

        float distance = (target - transform.position).magnitude;

        cameraComponent.orthographic = true;
        if (distance > 0.01f) {
            transform.position = Vector3.Lerp(transform.position, target, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(90, 0, 0)), 0.1f);
        } else {
            transform.position = target;
            transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }

    void Follow() {
        cameraComponent.orthographic = false;
        Transform r = robotResolver.transform;
        Vector3 target = r.position + r.forward * offsetPosition.z + r.right * offsetPosition.x + Vector3.up * offsetPosition.y;
        transform.position = Vector3.Lerp(transform.position, target, 0.1f);

        Quaternion targetRotation = Quaternion.LookRotation(r.position - transform.position);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.1f);
    }

    void CenteredTopDown() {
        Vector3 target = new Vector3(robotResolver.transform.position.x, 3, robotResolver.transform.position.z);

        cameraComponent.orthographic = false;
        transform.position = Vector3.Lerp(transform.position, target, 0.1f);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(90, 0, 0)), 0.1f);
    }

    public void ChangeView() {
        type = (type + 1) % maxViews;
    }

    void OnPostRender() {
        if (!overlayMaterial) {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things. In this case, we just want to use
            // a blend mode that inverts destination colors.
            var shader = Shader.Find("Hidden/Internal-Colored");
            overlayMaterial = new Material(shader);
            overlayMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Set blend mode to invert destination colors.
            // overlayMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
            // overlayMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            // // Turn off backface culling, depth writes, depth test.
            overlayMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            overlayMaterial.SetInt("_ZWrite", 0);
            overlayMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
        }
        GL.PushMatrix();

        // Activate first shader pass
        overlayMaterial.SetPass(0);

        GL.LoadProjectionMatrix(cameraComponent.projectionMatrix);
        GL.modelview = cameraComponent.worldToCameraMatrix;

        GL.Viewport(cameraComponent.pixelRect);

        // Draw the points
        for (int i = 0; i < overlayPoints.Count; i++) {
            DrawFilledCircle(overlayPoints[i]);
        }

        for (int i = 0; i < overlayLines.Count; i++) {
            DrawLine(overlayLines[i], 10);
        }

        GL.PopMatrix();
    }

    private void DrawLine(OverlayLine overlayLine, int thickness) {
        GL.Begin(GL.QUADS);
        GL.Color(overlayLine.color);

        Vector3 perpendicular = Vector3.Cross((overlayLine.start - overlayLine.end).normalized, Vector3.up) * 0.001f * thickness;

        GL.Vertex(overlayLine.start - perpendicular);
        GL.Vertex(overlayLine.start + perpendicular);
        GL.Vertex(overlayLine.end + perpendicular);
        GL.Vertex(overlayLine.end - perpendicular);

        GL.End();
    }

    private void DrawFilledCircle(OverlayPoint overlayPoint) {
        GL.Begin(GL.TRIANGLES);
        GL.Color(overlayPoint.color);

        Vector3 normal = Vector3.up.normalized;
        Vector3 forward = normal == Vector3.up ?
            Vector3.ProjectOnPlane(Vector3.forward, normal).normalized :
            Vector3.ProjectOnPlane(Vector3.up, normal);
        Vector3 right = Vector3.Cross(normal, forward);

        float circleQuality = 0.1f;

        for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += circleQuality) {
            Vector3 currentPoint = overlayPoint.center + forward * Mathf.Cos(theta) * overlayPoint.radius + right * Mathf.Sin(theta) * overlayPoint.radius;
            theta += circleQuality;
            Vector3 nextPoint = overlayPoint.center + forward * Mathf.Cos(theta) * overlayPoint.radius + right * Mathf.Sin(theta) * overlayPoint.radius;
            theta -= circleQuality;
            // Create triangle
            GL.Vertex(overlayPoint.center);
            GL.Vertex(currentPoint);
            GL.Vertex(nextPoint);
        }

        GL.End();
    }

    private void DrawCircle(OverlayPoint overlayPoint) {
        GL.Begin(GL.LINES);
        GL.Color(overlayPoint.color);

        Vector3 normal = Vector3.up.normalized;
        Vector3 forward = normal == Vector3.up ?
            Vector3.ProjectOnPlane(Vector3.forward, normal).normalized :
            Vector3.ProjectOnPlane(Vector3.up, normal);
        Vector3 right = Vector3.Cross(normal, forward);

        float circleQuality = 0.1f;

        for (float theta = 0.0f; theta < (2 * Mathf.PI); theta += circleQuality) {
            Vector3 currentPoint = overlayPoint.center + forward * Mathf.Cos(theta) * overlayPoint.radius + right * Mathf.Sin(theta) * overlayPoint.radius;
            // Create triangle
            GL.Vertex(currentPoint);
        }

        GL.End();
    }

    // Add a point to be rendered by openGL
    public void AddOverlayPoint(OverlayPoint point) {

        // If it already exists, just update it instead of creating a new instance
        for (int i = 0; i < overlayPoints.Count; i++) {
            if (overlayPoints[i].id == point.id) {
                overlayPoints[i] = point;
                return;
            }
        }

        // If we got more than 100 points, remove the first ones
        while (overlayPoints.Count > 100)
            overlayPoints.RemoveAt(0);

        // Finally, if its not existing, add it
        overlayPoints.Add(point);
    }

    // Adds a line to be rendered by openGL
    public void AddOverlayLine(OverlayLine line) {

        // If it already exists, just update it instead of creating a new instance
        for (int i = 0; i < overlayLines.Count; i++) {
            if (overlayLines[i].id == line.id) {
                overlayLines[i] = line;
                return;
            }
        }

        // If we got more than 100 lines, remove the first ones
        while (overlayLines.Count > 100)
            overlayLines.RemoveAt(0);

        // Finally, if its not existing, add it
        overlayLines.Add(line);
    }

    // Remove the point
    public void RemoveOverlayPoint(OverlayPoint point) {
        if (overlayPoints.Contains(point))
            overlayPoints.Remove(point);
    }

    public void ToggleRealism() {
        GetComponent<PostProcessVolume>().weight = (!(GetComponent<PostProcessVolume>().weight != 0)) ? 1 : 0;
	}
}
