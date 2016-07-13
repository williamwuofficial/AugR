using UnityEngine;
using System.Collections;

public class WebcamTest : MonoBehaviour {

    WebCamTexture webcamTexture;
    int webcamFPS = 30;
    Vector2 webcamResolution = new Vector2(1920, 1080);
    //public Texture webcamProjectionSurface;

    Color32[] webcamData;
    byte[] colorByteArray;
    Texture2D textBuffer;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);

        webcamTexture = new WebCamTexture();
        //GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.requestedFPS = webcamFPS;
        webcamTexture.requestedWidth = (int)webcamResolution.x;
        webcamTexture.requestedHeight = (int)webcamResolution.y;

        webcamData = new Color32[(int)webcamResolution.x * (int)webcamResolution.y];
        textBuffer = new Texture2D((int)webcamResolution.x , (int)webcamResolution.y);

        webcamTexture.Play();
    }

    void Update()
    {
        
        SerialiseWebcam();
        DeserialiseWebcam();


    }


    void SerialiseWebcam()
    {
        /*textBuffer.SetPixels32(webcamTexture.GetPixels32());
        textBuffer.Apply();
        colorByteArray = textBuffer.EncodeToJPG();*/

        webcamTexture.GetPixels32(webcamData);
        colorByteArray = TextureSerialiser.Color32MarshalByteArray(webcamData);
    }

    void DeserialiseWebcam()
    {
        /*textBuffer.LoadImage(colorByteArray);*/

        webcamData = TextureSerialiser.ByteMarshalColor32Array(colorByteArray);
        textBuffer.SetPixels32(webcamData);
        textBuffer.Apply();

        
        
        GetComponent<Renderer>().material.mainTexture = textBuffer;

    }
}
