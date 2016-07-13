using UnityEngine;
using System.Collections;

public class CameraFOV : MonoBehaviour {

    
    public Camera m_MarkerView;
    private Plane[] planes;
    public GameObject[] m_CalibrationCubes;
    public Material redColor;
    public Material blueColor;

    private enum ARStates { Found, Tracked, Lost};
    private ARStates ARState = ARStates.Lost;
	
	void Start () {
	
	}
	
	
	void Update () {
	    if(ARState == ARStates.Tracked)
        {
            //planes = GeometryUtility.CalculateFrustumPlanes(m_MarkerView);


            foreach (GameObject trackObj in m_CalibrationCubes)
            {
                if (IsInFrustrum(m_MarkerView, trackObj))
                {
                    trackObj.GetComponent<Renderer>().material = blueColor;

                }
                else
                {
                    trackObj.GetComponent<Renderer>().material = redColor;
                }
            }
           

            /*foreach (Plane camPlane in planes)
            {
                foreach (GameObject obj in m_CalibrationCubes)
                {
                    Collider objCollider = obj.GetComponent<Collider>();
                    if (GeometryUtility.TestPlanesAABB(planes, objCollider.bounds))
                    {
                        obj.GetComponent<Renderer>().material = blueColor;
                    }
                    else
                    {
                        obj.GetComponent<Renderer>().material = redColor;
                    }
                }
            }*/

        }
	}

    private bool IsInFrustrum(Camera cam, GameObject checkObj)
    {
        planes = GeometryUtility.CalculateFrustumPlanes(m_MarkerView);
        foreach (Plane camPlane in planes)
        {
            foreach (Vector3 vertice in checkObj.GetComponent<MeshFilter>().mesh.vertices)
            {
                //Debug.Log(camPlane.GetDistanceToPoint(trackObj.transform.TransformPoint(vertice)));
                if (camPlane.GetDistanceToPoint(checkObj.transform.TransformPoint(vertice)) < 0)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void CalibrateFOV()
    {
        
    }

    void OnMarkerFound(ARMarker marker)
    {
        ARState = ARStates.Found;
    }

    void OnMarkerLost(ARMarker marker)
    {
        ARState = ARStates.Lost;
    }

    void OnMarkerTracked(ARMarker marker)
    {
        ARState = ARStates.Tracked;
    }
}
