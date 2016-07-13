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

public class StreamServer : MonoBehaviour
{
    WebCamTexture webcamTexture;
    byte[] webcamData;
    Vector2 webcamResolution = new Vector2(320, 240);
    int webcamFPS = 1;
    GameObject webcamSurface;


    // Connection Initialisation
    int port = 8880;
    ConnectionConfig m_Config;
    int m_CommunicationChannel;
    int m_GenericHostId = -1;
    
    int recHostId;
    int connectionId;
    int channelId;
    int bufferLength = 1024;
    byte[] recBuffer;
    //int bufferSize = bufferLength;
    int bufferSize = 1024;
    bool sizeReceived = false;
    int bytesToReceive;
    int dataSize;
    byte error;


    void Start()
    {
        NetworkTransport.Init();
        m_Config = new ConnectionConfig();
        m_CommunicationChannel = m_Config.AddChannel(QosType.ReliableSequenced);


       // m_Config.IsAcksLong = true;
       // .MaxSentMessageQueueSize = 256 or higher (128 is default)
       // ConnectionConfig.FragmentSize to...our ConnectionConfig.PacketSize ? What is the goal of having the default be 1 / 3 of default PacketSize (1500 and 500) ?



        HostTopology topology = new HostTopology(m_Config, 12);
        m_GenericHostId = NetworkTransport.AddHost(topology, port);
        if (m_GenericHostId < 0) { Debug.LogError("Server socket creation failed!"); } else { Debug.Log("Server socket creation success\n"); }
        recBuffer = new byte[bufferLength];
        webcamData = new byte[(int)webcamResolution.x * (int)webcamResolution.y * 4];
    }

    void Update()
    {
        
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, bufferSize, out dataSize, out error);

        switch (recData)
        {
            case NetworkEventType.Nothing:         //1
                break;

            case NetworkEventType.ConnectEvent:    //2
                Debug.Log(string.Format("Connection: host {0} connection {1}\n", recHostId, connectionId));
                break;

            case NetworkEventType.DataEvent:       //3
                byte[] ack = new byte[1] { (byte)'A' };
                NetworkTransport.Send(recHostId, connectionId, channelId, ack, ack.Length, out error);
                //GetCurrentOutgoingMessageAmount()
                Debug.Log("Message Queue length " + NetworkTransport.GetCurrentOutgoingMessageAmount());
                Debug.Log("Receive Queue length " + NetworkTransport.GetCurrentIncomingMessageAmount());

                if (!sizeReceived)
                {
                    sizeReceived = true;

                    if (dataSize == 2)
                    {
                        bytesToReceive = BitConverter.ToInt16(recBuffer, 0);
                    }
                    else if (dataSize == 4)
                    {
                        bytesToReceive = BitConverter.ToInt32(recBuffer, 0);
                    }

                    Debug.Log("We will receive: " + bytesToReceive);
                } else
                {
                    Debug.Log(string.Format("Received event host {0} connection {1} channel {2} message length {3}, Error {4}\n", recHostId, connectionId, channelId, dataSize, error));
                    Debug.Log("Received " + bufferSize + " bytes\n");
                    System.Buffer.BlockCopy(recBuffer, 0, webcamData, 307200 - bytesToReceive, bufferLength);

                    bytesToReceive -= bufferSize;
                    //copy redBuffer to webcamData

                    
                    Debug.Log("Remaining " + bytesToReceive + " bytes\n");


                    if (bytesToReceive == 0)
                    {
                        Texture2D temp = new Texture2D((int)webcamResolution.x, (int)webcamResolution.y);
                        Color32[] colorArray = TextureSerializer.ByteArrayToColor32Array(webcamData, temp);
                        Debug.Log("Received Size " + colorArray.Length);

                        temp.SetPixels32(colorArray);

                        temp.Apply();

                        GetComponent<Renderer>().material.mainTexture = temp;
                    }
                    

                }
                /*Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                string message = formatter.Deserialize(stream) as string;
                Debug.Log("incoming message event received: " + message);
                */

                
 


                break;

            case NetworkEventType.DisconnectEvent: //4
                Debug.Log(string.Format("Disconnected: host {0} connection {1}\n", recHostId, connectionId));

                switch (error)
                {
                    case ((byte)NetworkError.VersionMismatch):
                        //treansport protocol is different
                        Debug.LogError("Version Mismatch");
                        break;
                    case ((byte)NetworkError.CRCMismatch):
                        //peer has different network configuration
                        Debug.LogError("CRC Mismatch");
                        break;
                    case ((byte)NetworkError.Timeout):
                        //cannot connect to other peer in period of time, possible peer has not running
                        Debug.LogError("Timeout");
                        break;
                }
                break;

        }
    }

   

}
