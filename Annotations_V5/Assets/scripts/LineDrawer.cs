using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour {

    private UILineTextureRenderer m_lineRenderer;

    private bool m_isMousePressed;
    private List<Vector2> m_pointsList;
    private Vector3 m_currentMousePos;
    public Text DebugText;
    public Vector3 m_calibration = new Vector3(0, -20, 0);

    void Start()
    {
        m_lineRenderer = GetComponent<UILineTextureRenderer>();
        //m_lineRenderer.material = new Material(Shader.Find("Self-Illumin/Diffuse"));
        m_lineRenderer.Points = new[] { new Vector2(0, 0), new Vector2(1, 1) };
        m_lineRenderer.LineThickness = 10f;
        m_lineRenderer.color = Color.blue;
        m_isMousePressed = false;
        m_pointsList = new List<Vector2>();
    }


    void Update()
    {
        // Test tje
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            m_isMousePressed = true;
            m_lineRenderer.Points = new[] { new Vector2(0, 0), new Vector2(1, 1) };
            m_pointsList.Clear();
            m_lineRenderer.color = Color.red;
            Debug.Log("mouse down");
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            m_isMousePressed = false;
            Debug.Log("mouse up");
        }
        // Drawing line when mouse is moving(presses)
        if (m_isMousePressed)
        {
            m_currentMousePos = Input.mousePosition;
            //Debug.LogError("P" + Input.mousePosition);
            DebugText.text = "P" + Input.mousePosition;

            if (!m_pointsList.Contains(m_currentMousePos))
            {
                m_pointsList.Add(Input.mousePosition + m_calibration);
                m_lineRenderer.Points = m_pointsList.ToArray();
                m_lineRenderer.SetAllDirty();
            }
        }
    }
}
