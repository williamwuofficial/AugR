using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections;

/// <summary>
/// Allow control of the depth plane mesh by different methods
/// IMU - MPU6050 integration
/// </summary>
public class MeshController : MonoBehaviour {

    private Camera m_MainCamera;

    private SerialPort stream;
    private string streamData;
    int rotationVal;    
    double scaleFactor;
        
    float timeDelay;
    float lastReadTime, currentReadTime;
    float readDelay;

    private int m_PlaneRotationSpeed = 20;
    private Vector3 m_LookPoint = new Vector3(125, -100, 0);

    void OnApplicationQuit()
    {
        if (stream != null)
        {
            stream.Close();
        }
        
    }
   
    void Start () {
        m_MainCamera = Camera.main;
        //m_MainCamera.transform.LookAt(m_LookPoint);

        stream = new SerialPort("COM7", 115200);
        stream.ReadTimeout = 3000;
        stream.Open();

        streamData = null;
        streamData = stream.ReadLine();
        stream.BaseStream.Flush();

        scaleFactor = (double)(359) / (680);
        timeDelay = Time.realtimeSinceStartup;
        float lastReadTime = Time.realtimeSinceStartup;
    }
	
	
	void Update () {
        float yVal = Input.GetAxis("Horizontal");
        float xVal = -Input.GetAxis("Vertical");

        transform.Rotate(
            (xVal * Time.deltaTime * m_PlaneRotationSpeed),
            (yVal * Time.deltaTime * m_PlaneRotationSpeed),
            0,
            Space.World);
     
        /*try
        {
            if (Time.realtimeSinceStartup - timeDelay < 0.245f)
            {
                return;
            }
            timeDelay = Time.realtimeSinceStartup;

            readDelay = Time.realtimeSinceStartup;
            streamData = stream.ReadLine();
            stream.BaseStream.Flush();
            Debug.Log("R - " + (Time.realtimeSinceStartup - readDelay));

            currentReadTime = Time.realtimeSinceStartup;
            Debug.Log( currentReadTime - lastReadTime);
            //Debug.Log(streamData);
            lastReadTime = currentReadTime;

            rotationVal = int.Parse(streamData);
            Debug.Log(rotationVal);
            transform.rotation = Quaternion.Euler(   0, (int)(-1*rotationVal * scaleFactor),  0 );

        }
        catch (TimeoutException)
        {
            Debug.LogError("Arduino Timeout Exception");
            Debug.Break();
        }
        catch (FormatException)
        {
            //Ignore
        }
        */
    }


}
