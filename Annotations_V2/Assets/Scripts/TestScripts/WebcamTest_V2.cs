using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using UniRx;
using UniRx.InternalUtil;
using UniRx.Operators;
using UniRx.Triggers; 
using UniRx.Diagnostics;

public class WebcamTest_V2 : MonoBehaviour {

    WebCamTexture webcamTexture;
    int webcamFPS = 1;
    Vector2 webcamResolution = new Vector2(320, 240);

    Color32[] webcamData;
    byte[] colorByteArray;
    Texture2D textBuffer;

    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);

        webcamTexture = new WebCamTexture();
        webcamTexture.requestedFPS = webcamFPS;
        webcamTexture.requestedWidth = (int)webcamResolution.x;
        webcamTexture.requestedHeight = (int)webcamResolution.y;
        GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.Play();

        var webStream = Observable.EveryUpdate()
            .Publish()
            .Where(_ => webcamTexture.didUpdateThisFrame)

        .Subscribe(x =>
        {
            StartCoroutine("AsyncNet");
        });


        //var clickStream = Observable.FromEvent(Input, 'click');
        //        .Where(_ => Input.GetMouseButtonDown(0));
        

        /*clickStream.Buffer(clickStream.Throttle(TimeSpan.FromMilliseconds(250)))
            .Where(xs => xs.Count >= 2)
            .Subscribe(xs => Debug.Log("DoubleClick Detected! Count:" + xs.Count));*/

    }

    //void function text

    IEnumerator AsyncNet()
    {
        yield return null;   
    }
}
