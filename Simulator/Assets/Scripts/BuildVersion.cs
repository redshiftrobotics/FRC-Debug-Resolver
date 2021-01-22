using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

[ExecuteInEditMode]
public class BuildVersion : MonoBehaviour
{
    // Update is called once per frame
    void Start()
    {
        string bn = Application.version+"b"+"e"+Application.unityVersion;

        GetComponent<Text>().text = "Build " + bn;
    }
}
