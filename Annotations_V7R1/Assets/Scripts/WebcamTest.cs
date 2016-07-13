using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WebcamTest : MonoBehaviour {

    public RawImage rawimage;
    void Start()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        rawimage.color = new Color(255, 255, 255, 255);
        rawimage.texture = webcamTexture;
        //rawimage.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }
}
