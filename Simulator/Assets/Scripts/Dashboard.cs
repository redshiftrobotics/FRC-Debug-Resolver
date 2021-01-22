using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class DashboardNetworkLink {
    public DashboardData data;
    public string varName;

    public DashboardNetworkLink(DashboardData _data) {
        data = _data;
    }
}

[ExecuteInEditMode]
public class Dashboard : MonoBehaviour {
    // Reference to the network
    private Network network;
    private Canvas canvas;

    // The active panel
    private int dashPanel;

    // List of dashboard elements
    public List<DashboardNetworkLink> data = new List<DashboardNetworkLink>();

    public List<GameObject> panels;

    // Start is called before the first frame update
    void Update() {
        if (canvas == null)
            canvas = transform.parent.GetComponent<Canvas>();

        if (network == null)
            network = FindObjectsOfType<Network>()[0];

        if (panels.Count == 0)
            panels = new List<GameObject>(GameObject.FindGameObjectsWithTag("DashboardPanel"));

        // Code ran in editor
        if (!Application.isPlaying) {
            // Get each data element
            List<DashboardData> dashData = new List<DashboardData>();
            GetComponentsInChildren<DashboardData>(dashData);

            // If we don't have all of the data elements in the hierarchy, add them
            if (data.Count < dashData.Count) {
                for (int i = data.Count; i < dashData.Count; i++) {
                    data.Add(new DashboardNetworkLink(dashData[i]));
                }
            }

            // Remove the ones that don't exist in the hierarchy
            foreach (DashboardNetworkLink link in data) {
                if (link.data == null)
                    data.Remove(link);
            }
        }

        // Code ran in play mode
        else {
            // Update the value in each data element
            foreach (DashboardNetworkLink link in data) {
                GetVariableFromNetwork(link);
            }

            // Only show the current panel
            foreach (GameObject panel in panels) {
                panel.SetActive(false);
            }

            if (dashPanel <= panels.Count)
                panels[dashPanel].SetActive(true);
        }
    }

    void GetVariableFromNetwork(DashboardNetworkLink link) {
        object variable = network.outputVars.GetType().GetField(link.varName).GetValue(network.outputVars);
        string type = variable.GetType().ToString();

        switch (type) {
            case "UnityEngine.Vector2":
                link.data.Set((Vector2)variable);
                break;

            case "System.Single":
                link.data.Set((float)variable);
                break;

            default:
                link.data.Set(variable.ToString());
                break;
        }

    }

    public void SetPanel(int _panel) {
        dashPanel = _panel;
    }
}
