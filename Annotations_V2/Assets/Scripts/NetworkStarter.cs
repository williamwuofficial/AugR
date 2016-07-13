using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections;

public class NetworkStarter : MonoBehaviour {

    public static NetworkStarter _Instance;

    // Sockets, IP and Port
    string m_ServerIP = "127.0.0.1";
    int m_Port = 8880;
    int m_ServerSocket = -1;
    int m_ClientSocket = -1;
    int m_ClientConnection = -1;

    // Transport Layer Initialisation
    GlobalConfig m_GlobalConfig;
    ConnectionConfig m_ConnConfig;
    int m_MaxConnections = 12;
    HostTopology m_HostTop;
    byte m_ChannelReliable;
    byte m_ChannelUnreliable;
    
    // Network Event Dispatchers
    public delegate void OnNetworkRecEvent(int socketID, int connectionID, 
        int channelID, byte[] recBuffer, int bufferLen, int dataSize, byte error);
    public static event OnNetworkRecEvent OnNetworkConnect = delegate { };
    public static event OnNetworkRecEvent OnNetworkData = delegate { };
    public static event OnNetworkRecEvent OnNetworkDisconnect = delegate { };

    // Receive Handling
    byte m_error;
    int m_recSocketID;
    int m_connectionID; // Unique per socket
    List<int> m_registeredConnections = new List<int>();
    int m_channelID;
    NetworkEventType m_recDataType = NetworkEventType.Nothing;
    int m_recDataSize;
    int m_recBufferLength = 1024;
    byte[] m_recBuffer = new byte[1024];

    // Send Handling
    int m_sendBufferLength = 1024;
    byte[] m_sendBuffer = new byte[1024];
    int m_remainingBytes = 0;
    int m_currentIndex = 0;
    [HideInInspector]
    public bool m_sendExecuting = false;
        
    // Change to false to initialise client instead
    [HideInInspector]
    public bool m_InitialiseServer = true; 

    void OnApplicationQuit()
    {
        NetworkTransport.Shutdown();
        Debug.Log("Application ended after " + Time.time + " seconds");
    }

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        } else
        {
            Debug.LogError("NetworkStarter Instance already exists!");
        }
    }

    void Start()
    {
        // Build global config
        m_GlobalConfig = new GlobalConfig();
        m_GlobalConfig.ReactorModel = ReactorModel.SelectReactor;
        //m_GlobalConfig.ReactorModel = ReactorModel.FixRateReactor;
        //m_GlobalConfig.ThreadAwakeTimeout = 10;
        
        // Build channel config
        ConnectionConfig m_ConnConfig = new ConnectionConfig();
        m_ChannelReliable = m_ConnConfig.AddChannel(QosType.ReliableSequenced);
        m_ChannelUnreliable = m_ConnConfig.AddChannel(QosType.UnreliableFragmented);
        m_ConnConfig.MaxSentMessageQueueSize = 1024;
        //m_ConnConfig.FragmentSize = 500; 

        // Create host topology from connection configuration
        m_HostTop = new HostTopology(m_ConnConfig, m_MaxConnections);

        // Initialise the transport layer
        NetworkTransport.Init(m_GlobalConfig);

        if (m_InitialiseServer)
        {
            ServerInit();
        } else
        {
            ClientInit();
        }
    }

    public void ServerInit()
    {
        m_ServerSocket = NetworkTransport.AddHost(m_HostTop, m_Port);
        if (m_ServerSocket < 0) { Debug.LogError("Server socket creation failed!"); } else { Debug.Log("Server socket creation successful"); }
    }

    public void ClientInit()
    {
        m_ClientSocket = NetworkTransport.AddHost(m_HostTop);
        if (m_ClientSocket < 0) { Debug.LogError("Client socket creation failed!"); } else { Debug.Log("Client socket creation successful!"); }
        m_ClientConnection = NetworkTransport.Connect(m_ClientSocket, m_ServerIP, m_Port, 0, out m_error);
        if ((NetworkError)m_error != NetworkError.Ok) { Debug.LogError("Unable to allocate resources for client connection"); }
    }

    void Update()
    {

        do {
            m_recDataType  = NetworkTransport.Receive(out m_recSocketID, 
                out m_connectionID, out m_channelID, m_recBuffer, m_recBufferLength, out m_recDataSize, out m_error);

            switch (m_recDataType)
            {
                case NetworkEventType.Nothing:         //1
                    break;
                case NetworkEventType.ConnectEvent:    //2
                    Debug.Log(string.Format(((m_recSocketID == m_ServerSocket) ? "Server" : "Client") 
                        + "Connection: host {0} connection {1}\n", 
                        m_recSocketID, m_connectionID));
                    OnNetworkConnect(m_recSocketID, m_connectionID, m_channelID, m_recBuffer, m_recBufferLength, m_recDataSize, m_error);
                    m_registeredConnections.Add(m_connectionID);
                    break;
                case NetworkEventType.DataEvent:       //3
                    Debug.Log(string.Format(NetworkTransport.GetCurrentIncomingMessageAmount() + " Received " + ((m_recSocketID == m_ServerSocket) ? "Server" : "Client") 
                        + "Data: host {0} connection {1} channel {2} message length {3} errpr {4}\n", 
                        m_recSocketID, m_connectionID, m_channelID, m_recDataSize, (NetworkError) m_error));
                    OnNetworkData(m_recSocketID, m_connectionID, m_channelID, m_recBuffer, m_recBufferLength, m_recDataSize, m_error);
                    break;
                case NetworkEventType.DisconnectEvent: //4
                    Debug.Log(string.Format("Disconnected: host {0} connection {1}\n", m_recSocketID, m_connectionID));
                    OnNetworkDisconnect(m_recSocketID, m_connectionID, m_channelID, m_recBuffer, m_recBufferLength, m_recDataSize, m_error);
                    m_registeredConnections.Remove(m_connectionID);
                    switch (m_error)
                    {
                        case ((byte)NetworkError.VersionMismatch):
                            Debug.LogError("Network Version Mismatch: Transport protocol may be different");
                            break;

                        case ((byte)NetworkError.CRCMismatch):
                            Debug.LogError("Network CRC Mismatch: Peer has different network configurations");
                            break;
                        case ((byte)NetworkError.Timeout):
                            Debug.LogError("Network Timeout: unable to connect");
                            break;
                    }
                    break;
            }
        } while (m_recDataType != NetworkEventType.Nothing);

    }

    public IEnumerator BroadcastNetworkData(bool reliableChannel, byte[] sendData)
    {
        byte sendChannel = (reliableChannel) ? m_ChannelReliable : m_ChannelUnreliable;
        int sendSocket = (m_ServerSocket != -1) ? m_ServerSocket : m_ClientSocket;
        m_currentIndex = 0;
        m_remainingBytes = sendData.Length;
        m_sendExecuting = true;
        int sendLength;

        // Send size of data 
        byte[] sizeToSend = BitConverter.GetBytes(sendData.Length);
        m_registeredConnections.ForEach(connection =>
        {
            byte s_error;
            NetworkTransport.Send(sendSocket, connection, sendChannel, sizeToSend, sizeToSend.Length, out s_error);
            if ((NetworkError)s_error != NetworkError.Ok)
            {
                Debug.LogError("Network Error [" + (NetworkError) s_error + "]: Unable to broadcast package size");
            }
        });

        // Send the rest of data 
        while (m_remainingBytes > 0)
        {
            sendLength = (m_remainingBytes >= m_sendBufferLength) ? m_sendBufferLength : m_remainingBytes;
            System.Buffer.BlockCopy(sendData, m_currentIndex, m_sendBuffer, 0, sendLength);
            m_registeredConnections.ForEach(connection =>
            {
                byte s_error;
                NetworkTransport.Send(sendSocket, connection, sendChannel, m_sendBuffer, sendLength, out s_error);
                if ((NetworkError) s_error != NetworkError.Ok)
                {
                    Debug.LogError("Network Error [" + ((NetworkError)s_error).ToString() + "]: Unable to broadcast package data");
                }
            });
            
            m_currentIndex += m_sendBufferLength;
            m_remainingBytes -= m_sendBufferLength;
            Debug.Log(NetworkTransport.GetCurrentOutgoingMessageAmount() + " Remaining package bytes: " + m_remainingBytes);

            if (m_remainingBytes <= 0)
            {
                m_sendExecuting = false;
                yield return new WaitForEndOfFrame();
            } else
            {
                yield return null;
            }
            
        }

    }

}
