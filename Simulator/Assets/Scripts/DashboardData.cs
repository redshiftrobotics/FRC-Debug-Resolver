﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[ExecuteInEditMode]
public class DashboardData : MonoBehaviour
{
    public string title;

    private Text titleElement;
    private Text dataElement;

    void Awake() {
        titleElement = transform.GetChild(0).GetComponent<Text>();
        dataElement = transform.GetChild(1).GetComponent<Text>();
    }
    

    void Update() {
        titleElement.text = title;
    }

    public void Set(string _title, string _data) {
        titleElement.text = _title;
        dataElement.text = _data;
    }

    public void Set(string _data) {
        dataElement.text = _data;
    }

    public void Set(Vector3 _data) {
        string data = _data.ToString();

        Set(data);
    }

    public void Set(Vector2 _data) {
        string data = _data.ToString();

        Set(data);
    }

    public void Set(float _data) {
        string data = _data.ToString("F2");
        
        Set(data);
    }
}
