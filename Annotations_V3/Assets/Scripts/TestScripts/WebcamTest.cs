using UnityEngine;

public class WebcamTest : MonoBehaviour
{
    WebCamTexture webcamTexture;
    int webcamFPS = 5;
    Vector2 webcamResolution = new Vector2(320, 240);


    void Start()
    {

        Debug.Log("displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();



        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);

        webcamTexture = new WebCamTexture();
        webcamTexture.requestedFPS = webcamFPS;
        webcamTexture.requestedWidth = (int)webcamResolution.x;
        webcamTexture.requestedHeight = (int)webcamResolution.y;
        webcamTexture.Play();
        GetComponent<Renderer>().material.mainTexture = webcamTexture;
    }

}
