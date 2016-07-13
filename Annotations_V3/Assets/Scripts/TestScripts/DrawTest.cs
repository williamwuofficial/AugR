using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Using previous draw test V4 
/// Instantiate a line renderer prefab, then for this test run everything with the prefab gameobject
/// </summary>
public class DrawTest : MonoBehaviour {

    public GameObject m_lineRendererPrefab;
    private LineRenderer m_lineRenderer;

    private bool m_isMousePressed;
    private List<Vector3> m_pointsList;
    private Vector3 m_currentMousePos;

	
	void Start () {
        GameObject clone =  (GameObject) Instantiate(m_lineRendererPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        clone.transform.parent = transform;
        clone.name = "ClientSketcher[1]";

        m_lineRenderer = clone.GetComponent<LineRenderer>();
        //m_lineRenderer.material = new Material(Shader.Find("Self-Illumin/Diffuse"));
        //m_lineRenderer.SetVertexCount(1);
        m_lineRenderer.SetWidth(1f, 1f);
        m_lineRenderer.SetColors(Color.green, Color.green);
        m_lineRenderer.useWorldSpace = false; 
        m_isMousePressed = false;
        m_pointsList = new List<Vector3>();
    }
	
	
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            m_isMousePressed = true;
            m_lineRenderer.SetVertexCount(1);
            m_pointsList.RemoveRange(0, m_pointsList.Count);
            //m_pointsList.Clear();
            m_lineRenderer.SetColors(Color.green, Color.green);
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_isMousePressed = false;
        }
        // Drawing line when mouse is moving(presses)
        if (m_isMousePressed)
        {
            //m_currentMousePos = Input.mousePosition;
            Debug.Log("P" + Input.mousePosition);

            if (!m_pointsList.Contains(m_currentMousePos))
            {
                m_pointsList.Add(Input.mousePosition);
                m_lineRenderer.SetVertexCount(m_pointsList.Count);
                m_lineRenderer.SetPosition(m_pointsList.Count - 1, (Vector3)m_pointsList[m_pointsList.Count - 1]);
            }        
        }
    }
}
