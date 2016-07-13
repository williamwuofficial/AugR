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

public class StreamClient : MonoBehaviour
{
    WebCamTexture webcamTexture;
    Color32[] webcamData;
    Vector2 webcamResolution = new Vector2(320, 240);
    int webcamFPS = 1;
    GameObject webcamSurface; 

    // Connection Initialisation
    string ip = "127.0.0.1";
    int port = 8880;
    ConnectionConfig m_Config;
    int m_CommunicationChannel;
    int m_GenericHostId;
    int m_ConnectionId;

    int recHostId;
    int connectionId;
    int channelId;
    byte[] recBuffer;
    int bufferLength = 1024;
    //int bufferSize = bufferLength;
    int bufferSize = 1024;
    int dataSize;
    byte error;
    bool _readyToSend = false;

    byte[] colorArray;
    int index = 0;
    int i = 1;

    byte[] bytes = new byte[1024];
    int remainingBytes;

    // Sending total size
    byte[] sizeToSend;
    Texture2D temp;

    bool NetworkInitialised = false;


    void OnApplicationQuit()
    {
        webcamTexture.Stop();
        NetworkTransport.Shutdown();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }

    void Start()
    {
        Screen.fullScreen = false;

        WebCamDevice[] devices = WebCamTexture.devices;
        for (var i = 0; i < devices.Length; i++)
            Debug.Log(devices[i].name);

        webcamTexture = new WebCamTexture();
        webcamTexture.requestedFPS = webcamFPS;
        webcamTexture.requestedWidth = (int) webcamResolution.x;
        webcamTexture.requestedHeight = (int) webcamResolution.y;
        webcamData = new Color32[(int) webcamResolution.x * (int) webcamResolution.y];
        if (webcamSurface != null)
        {
            webcamSurface.GetComponent<Renderer>().material.mainTexture = webcamTexture;
            //webcamSurface = webcamTexture;
        }
        webcamTexture.Play();

        Invoke("NetworkInitialisation",2);


    }

    void NetworkInitialisation()
    {
        //Begin Network Initialisation
        NetworkTransport.Init();
        m_Config = new ConnectionConfig();
        m_CommunicationChannel = m_Config.AddChannel(QosType.ReliableSequenced);
        HostTopology topology = new HostTopology(m_Config, 12);
        m_GenericHostId = NetworkTransport.AddHost(topology);
        if (m_GenericHostId < 0) { Debug.LogError("Client socket creation failed!"); } else { Debug.Log("Client socket creation success\n"); }
        m_ConnectionId = NetworkTransport.Connect(m_GenericHostId, ip, port, 0, out error);
        if ((NetworkError)error != NetworkError.Ok)
        {
            Debug.LogError("Unable to allocate resources for connection");
        }
        recBuffer = new byte[bufferLength];
        NetworkInitialised = true;
    }

    void Update()
    {

        if (!NetworkInitialised)
        {
            return;
        }
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recData)
        {
            case NetworkEventType.Nothing:         //1
                break;
            case NetworkEventType.ConnectEvent:    //2
                Debug.Log(string.Format("Connection: host {0} connection {1}\n", recHostId, connectionId));
                webcamTexture.GetPixels32(webcamData);


                temp = new Texture2D(320, 240);
                temp.SetPixels32(webcamData);
                temp.Apply();
                webcamSurface.GetComponent<Renderer>().material.mainTexture = temp;


                colorArray = TextureSerializer.Color32ArrayToByteArray(webcamData);
                // Sending total size
                sizeToSend = BitConverter.GetBytes(colorArray.Length);
                Debug.Log("Sending size " + colorArray.Length);

                remainingBytes = colorArray.Length;


                NetworkTransport.Send(m_GenericHostId, m_ConnectionId, m_CommunicationChannel, sizeToSend, sizeToSend.Length, out error);
                _readyToSend = true;
                break;
            case NetworkEventType.DataEvent:       //3
                Debug.Log(string.Format("Received event host {0} connection {1} channel {2} message length {3}\n", recHostId, connectionId, channelId, dataSize));
                break;
            case NetworkEventType.DisconnectEvent: //4
                Debug.Log(string.Format("Disconnected: host {0} connection {1}\n", recHostId, connectionId));

                switch (error)
                {
                    case ((byte)NetworkError.VersionMismatch):
                        //treansport protocol is different
                        Debug.LogError("Network Version Mismatch");
                        break;

                    case ((byte)NetworkError.CRCMismatch):
                        //peer has different network configuration
                        Debug.LogError("Network CRC Mismatch");
                        break;
                    case ((byte)NetworkError.Timeout):
                        //cannot connect to other peer in period of time, possible peer has not running
                        Debug.LogError("Network Timeout");
                        break;
                }
                break;
        }

        if (_readyToSend)
        {
            //_readyToSend = false; // To send just the first frame

            //byte[] colourArray = SerializeObject(MakeSerializable(GetRenderTexturePixels(webcamTexture))); // Serialize the webcam texture

            if (remainingBytes >= bufferLength)
            {
                System.Buffer.BlockCopy(colorArray, index, bytes, 0, bufferLength);
                NetworkTransport.Send(m_GenericHostId, m_ConnectionId, m_CommunicationChannel, bytes, bytes.Length, out error);
                remainingBytes -= bufferLength;
                Debug.Log(i++ + " - Remaining bytes: " + remainingBytes + " - Error: " + error);
                Debug.Log("Message Queue length " + NetworkTransport.GetCurrentOutgoingMessageAmount());
                Debug.Log("Receive Queue length " + NetworkTransport.GetCurrentIncomingMessageAmount());

                index += bufferLength;
            }
            else if (remainingBytes > 0)
            {
                System.Buffer.BlockCopy(colorArray, index, bytes, 0, remainingBytes);
                NetworkTransport.Send(m_GenericHostId, m_ConnectionId, m_CommunicationChannel, bytes, remainingBytes, out error);
                remainingBytes -= bufferLength;
                Debug.Log("Error: " + error);
            }
        }

            




    }

}
