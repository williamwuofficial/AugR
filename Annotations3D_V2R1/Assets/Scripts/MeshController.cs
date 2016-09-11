using UnityEngine;
using System.IO.Ports;
using System;
using System.Collections;

/// <summary>
/// Allow control of the depth plane mesh by different methods
/// IMU - MPU6050 integration
/// </summary>
public class MeshController : MonoBehaviour {

    public UIManager uiManager;

    private int m_PlaneRotationSpeed = 20;
    private int m_PlaneMoveSpeed = 50;
    private int m_PlaneScaleSpeed = 20;
    private int m_PlaneZoomSpeed = 20;

   
    void Start () {
  
    }
	
	
	void Update () {
        float yVal = Input.GetAxis("Horizontal");
        float xVal = -Input.GetAxis("Vertical");

        transform.Rotate(
            (-xVal * Time.deltaTime * m_PlaneRotationSpeed),
            (-yVal * Time.deltaTime * m_PlaneRotationSpeed),
            0,
            Space.World);

        float leftRight = Input.GetAxis("JoyX");
        transform.Translate(Vector3.right * Time.deltaTime * leftRight * m_PlaneMoveSpeed, Space.World);

        float upDown = Input.GetAxis("JoyY");
        transform.Translate(Vector3.up * Time.deltaTime * upDown * m_PlaneMoveSpeed, Space.World);

        float frontBack = Input.GetAxis("JoyZ");
        transform.Translate(Vector3.forward * Time.deltaTime * frontBack * m_PlaneMoveSpeed, Space.World);

        // Actually scale the object
        float scale = Input.GetAxis("Scale");
        transform.localScale += new Vector3(Time.deltaTime * scale * m_PlaneScaleSpeed, 
                                                Time.deltaTime * scale * m_PlaneScaleSpeed, 
                                                Time.deltaTime * scale * m_PlaneScaleSpeed);

        // Actually increase the value of the numbers in the surface mesh
        // call back to UI Manager
        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if(uiManager != null)
            {
                uiManager.m_DepthScale.value += 0.1f;
                uiManager.SaveSettings();
                uiManager.ApplySettings();
            }
        } else if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (uiManager != null)
            {
                uiManager.m_DepthScale.value -= 0.1f;
                uiManager.SaveSettings();
                uiManager.ApplySettings();
            }
        }

    }


}
