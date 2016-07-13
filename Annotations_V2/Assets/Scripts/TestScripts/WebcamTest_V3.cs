using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WebcamTest_V3 : MonoBehaviour
{

    public RawImage rawimage;
    void Start()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        rawimage.texture = webcamTexture;
        rawimage.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }
}