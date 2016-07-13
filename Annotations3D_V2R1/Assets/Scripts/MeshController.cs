using UnityEngine;
using System.Collections;

/// <summary>
/// Allow control of the depth plane mesh by different methods
/// </summary>
public class MeshController : MonoBehaviour {

    private Camera m_MainCamera;
    private float m_planeAngleOffset;
    private int m_PlaneRotationSpeed = 20;
    private Vector3 m_LookPoint = new Vector3(125, -100, 0);

   
    void Start () {
        m_MainCamera = Camera.main;
        //m_MainCamera.transform.LookAt(m_LookPoint);
    }
	
	
	void Update () {
        float yVal = Input.GetAxis("Horizontal");
        float xVal = -Input.GetAxis("Vertical");

        // TODO: Fix so rotates at centre of object
        transform.Rotate(
            (xVal * Time.deltaTime * m_PlaneRotationSpeed),
            (yVal * Time.deltaTime * m_PlaneRotationSpeed),
            0,
            Space.World);

        //transform.Rotate(Vector3.up, 10*Time.deltaTime);
        //transform.RotateAround(new Vector3(125, 100, 100), Vector3.up, 10 * Time.deltaTime);
    }
}
