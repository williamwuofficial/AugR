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

[RequireComponent(typeof(NetworkStarter))]
public class StreamClient : MonoBehaviour
{

    Vector2 m_WebcamResolution = new Vector2(320, 240);
    byte[] webcamData = new byte[320*240*4]; // Maximum buffer length assuming no compression
    Texture2D m_TextBuffer;

    bool m_IsPackageSizeReceived = false;
    int m_PackageSize;
    int m_recIndex;
    

    void Awake()
    {
        GetComponent<NetworkStarter>().m_InitialiseServer = false;
    }

    void Start()
    {
        m_TextBuffer = new Texture2D((int)m_WebcamResolution.x, (int)m_WebcamResolution.y);
        NetworkStarter.OnNetworkData += StreamClient_OnNetworkData;
    }

    private void StreamClient_OnNetworkData(int socketID, int connectionID, int channelID, byte[] recBuffer, int bufferLen, int dataSize, byte error)
    {
        // TODO test if this bug is fixed >< >< >< >< >< 
        byte[] ack = new byte[1] { (byte)'A' };
        NetworkTransport.Send(socketID, connectionID, channelID, ack, ack.Length, out error);

        if (!m_IsPackageSizeReceived)
        {
            m_IsPackageSizeReceived = true;
            if (dataSize == 2)
            {
                m_PackageSize = BitConverter.ToInt16(recBuffer, 0);
            }
            else if (dataSize == 4)
            {
                m_PackageSize = BitConverter.ToInt32(recBuffer, 0);
            }
            m_recIndex = m_PackageSize;
            Debug.Log("Accepting package size: " + m_PackageSize);
        } else
        {
            //ArgumentException: Offset and length were out of bounds for the array or count is greater than the number of elements from the index to the end of the source collection
            try
            {
                System.Buffer.BlockCopy(recBuffer, 0, webcamData, m_PackageSize - m_recIndex, dataSize);
            } catch (Exception e)
            {
                Debug.LogError("Error in BlockCopy" + e.ToString());
                return;
            }            

            m_recIndex -= dataSize;
            if (m_recIndex == 0)
            {
                //Color32[] colorArray = TextureSerialiser.ByteMarshalColor32Array(webcamData);
                //m_TextBuffer.SetPixels32(colorArray);
                //m_TextBuffer.Apply();
                m_TextBuffer.LoadImage(webcamData);

                GetComponent<Renderer>().material.mainTexture = m_TextBuffer;

                m_IsPackageSizeReceived = false;
            }
        }

        
    }
}
