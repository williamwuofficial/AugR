using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;

using UniRx;
using UniRx.InternalUtil;
using UniRx.Operators;
using UniRx.Triggers;
using UniRx.Diagnostics;


/// <summary>
/// The server will be streaming it's webcam video feed to multiple clients
/// and accept the annotations data from multiple clients
/// </summary>
[RequireComponent (typeof(NetworkStarter))]
public class StreamServer : MonoBehaviour
{
    WebCamTexture m_WebcamTexture;
    Color32[] m_WebcamData;
    Vector2 m_WebcamResolution = new Vector2(320, 240);
    int m_WebcamFPS = 5;
    int m_webcamFramesSent = 0;

    Texture2D m_TextBuffer;
    byte[] m_ColorArray;
    int m_ConnectedClients = 0;

    void OnApplicationQuit()
    {
        m_WebcamTexture.Stop();
    }

    IEnumerator Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);

        m_WebcamTexture = new WebCamTexture();
        m_WebcamTexture.requestedFPS = m_WebcamFPS;
        m_WebcamTexture.requestedWidth = (int)m_WebcamResolution.x;
        m_WebcamTexture.requestedHeight = (int)m_WebcamResolution.y;
        m_WebcamData = new Color32[(int)m_WebcamResolution.x * (int)m_WebcamResolution.y];
        m_TextBuffer = new Texture2D((int)m_WebcamResolution.x , (int)m_WebcamResolution.y);
        m_WebcamTexture.Play();

        NetworkStarter.OnNetworkConnect += StreamServer_OnNetworkConnect;
        NetworkStarter.OnNetworkDisconnect += StreamServer_OnNetworkDisconnect;

        yield return new WaitForSeconds(2.0f);
    }
    
    void Update()
    {
        if (m_ConnectedClients > 0 && !NetworkStarter._Instance.m_sendExecuting && m_WebcamTexture.didUpdateThisFrame)
        {

            m_WebcamTexture.GetPixels32(m_WebcamData);

            m_TextBuffer.SetPixels32(m_WebcamData);
            m_TextBuffer.Apply();
            GetComponent<Renderer>().material.mainTexture = m_TextBuffer;
            
            //m_ColorArray = TextureSerialiser.Color32MarshalByteArray(m_WebcamData);
            m_ColorArray = m_TextBuffer.EncodeToJPG();

            //Debug.Log("Sending Texture size " + m_ColorArray.Length);
            m_webcamFramesSent++;
            Debug.Log("Frame " + m_webcamFramesSent);
            StartCoroutine(NetworkStarter._Instance.BroadcastNetworkData(false, m_ColorArray));
        }
    }


    private void StreamServer_OnNetworkConnect(int socketID, int connectionID, int channelID, byte[] recBuffer, int bufferLen, int dataSize, byte error)
    {
        m_ConnectedClients++;
    }

    private void StreamServer_OnNetworkDisconnect(int socketID, int connectionID, int channelID, byte[] recBuffer, int bufferLen, int dataSize, byte error)
    {
        Debug.LogError("Stream Server: Client Disconnect");
        m_ConnectedClients--;
    }

}
