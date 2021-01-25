using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public enum LogType
{
    WARNING,
    ERROR,
    MESSAGE
}

public class Log : MonoBehaviour
{

    private Text ui;

    public List<string> log = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        ui = GetComponent<Text>();
    }

    void FixedUpdate() {
        ui.text = "";
        for (int i = 0; i < log.Count; i++)
        {
            ui.text += log[i];
        }
    }

    public void Add(string message, LogType type) {
        Debug.Log(message);

        string color;

        switch (type)
        {
            case LogType.WARNING:
                color = "yellow";
                break;
            case LogType.ERROR:
                color = "red";
                break;
            case LogType.MESSAGE:
                color = "lightblue";
                break;
            default:
                color = "lightblue";
                break;
        }

        string messageType = "<color=" + color + ">" + type.ToString() + "</color>";

        string date = System.DateTime.Now.ToLongTimeString();
        
        log.Add(date + " " + messageType + ": " + message + "\n");
    }

    public void Clear() {
        log.Clear();
    }
}
