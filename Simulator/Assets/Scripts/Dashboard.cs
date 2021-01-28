using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dashboard : MonoBehaviour {
    // Reference to the network
    private Network network;
    private Canvas canvas;

    // The active panel
    private int dashPanel;
    public List<DashboardData> dashDatas = new List<DashboardData>();


    public List<GameObject> panels = new List<GameObject>();

    public GameObject simulatorModal;

    void Start() {
        canvas = transform.parent.GetComponent<Canvas>();

        network = FindObjectOfType<Network>();

        // Get each data element
        GetComponentsInChildren<DashboardData>(dashDatas);

    }

    // Start is called before the first frame update
    void Update() {

        foreach (DashboardData data in dashDatas)
        {
            GetVariableFromNetwork(data);
        }

        // Only show the current panel
        foreach (GameObject panel in panels) {
            panel.SetActive(false);
        }

        if (dashPanel <= panels.Count)
            panels[dashPanel].SetActive(true);

        simulatorModal.SetActive(network.netVars.manual_control);
    }

    void GetVariableFromNetwork(DashboardData link) {
        dynamic variable;

        try {
            variable = network.GetVariable(link.gameObject.name).GetValue(network.netVars);
        } catch (System.NullReferenceException) {
            return;
        }

        if (variable == null)
            return;

        string type = variable.GetType().ToString();

        switch (type) {
            case "UnityEngine.Vector2":
                link.Set((Vector2)variable);
                break;

            case "System.Single":
                link.Set((float)variable);
                break;

            default:
                link.Set(variable.ToString());
                break;
        }
    }

    public void SetPanel(int _panel) {
        dashPanel = _panel;
    }
}
