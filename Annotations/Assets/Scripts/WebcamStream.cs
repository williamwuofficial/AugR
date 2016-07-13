using UnityEngine;
using System.Collections;

public class WebcamStream : MonoBehaviour {

    WebCamTexture webcamTexture;

    void Start () {

        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);

        webcamTexture = new WebCamTexture();
        GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.requestedFPS = 30;
        webcamTexture.Play();
    }
	
    void Update()
    {
        Debug.Log("FPS: " + webcamTexture.requestedFPS);
    }
}
