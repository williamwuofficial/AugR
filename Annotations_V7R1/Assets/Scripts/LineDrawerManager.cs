using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;
using System.Collections;

public class LineDrawerManager : MonoBehaviour {

    public enum DrawMode { Draw, Erase};
    public DrawMode m_CurrentDrawMode = DrawMode.Draw;
    
    public GameObject m_lineRenderPrefab;
    public Canvas m_AnnotationCanvas;
    public List<GameObject> m_AllDrawingLines;
    public List<GameObject> m_LinesRemovalList;


    public ScaleCanvas m_ScaleCanvasInstance;
    private Vector3 m_currentMousePos;
    private Vector3 m_lastMousePos;
    private UILineRenderer m_CurrentLine;
    private List<Vector2> m_CurrentDrawPoints;
    private bool m_ModeChangeReady = false;

    [Header("GUI Components")]
    public Text m_DrawModeButton;
    private float m_DefaultLineThickness = 10f;
    public Slider m_LineThicknessSlider;

    // Use this for initialization
    void Start () {
        m_AllDrawingLines = new List<GameObject>();
        m_LinesRemovalList = new List<GameObject>();
        m_DrawModeButton.text = "Draw Mode";
        m_lineRenderPrefab.GetComponent<UILineRenderer>().LineThickness = m_DefaultLineThickness;
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("PG " + EventSystem.current.currentInputModule.name);
        

        switch (m_CurrentDrawMode)
        {
            case DrawMode.Draw:
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    m_CurrentDrawPoints = new List<Vector2>();
                    GameObject newLine = (GameObject)Instantiate(m_lineRenderPrefab, Vector3.one, Quaternion.identity);
                    m_AllDrawingLines.Add(newLine);
                    newLine.transform.parent = m_AnnotationCanvas.transform;
                    m_CurrentLine = newLine.GetComponent<UILineRenderer>();
                    m_ScaleCanvasInstance.m_childObjects.Add(newLine.GetComponent<RectTransform>());
                    StartCoroutine(m_ScaleCanvasInstance.ScaleAllCanvas());
                    m_lastMousePos = Input.mousePosition;

                }

                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    m_currentMousePos = Input.mousePosition;

                    //DebugText.text = "P" + Input.mousePosition;

                    if (!m_CurrentDrawPoints.Contains(m_currentMousePos) )
                    {
                        m_lastMousePos = m_currentMousePos;
                        m_CurrentDrawPoints.Add(m_currentMousePos);
                        m_CurrentLine.Points = m_CurrentDrawPoints.ToArray();
                        m_CurrentLine.SetAllDirty();
                    }
                }

                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    m_CurrentLine.color = Color.black;

                    if (m_ModeChangeReady)
                    {
                        ToggleModes();
                        m_ModeChangeReady = false;
                    }
                }
                
                break;
            case DrawMode.Erase:
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    m_CurrentDrawPoints = new List<Vector2>();
                    GameObject newLine = (GameObject)Instantiate(m_lineRenderPrefab, Vector3.one, Quaternion.identity);
                    newLine.transform.parent = m_AnnotationCanvas.transform;
                    m_CurrentLine = newLine.GetComponent<UILineRenderer>();
                    m_CurrentLine.color = Color.yellow;
                    m_CurrentLine.color = new Color(m_CurrentLine.color.r, m_CurrentLine.color.g, m_CurrentLine.color.b, 0);
                    m_ScaleCanvasInstance.m_childObjects.Add(newLine.GetComponent<RectTransform>());
                    StartCoroutine(m_ScaleCanvasInstance.ScaleAllCanvas());
                }

                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    m_currentMousePos = Input.mousePosition;
                    //DebugText.text = "P" + Input.mousePosition;

                    if (!m_CurrentDrawPoints.Contains(m_currentMousePos))
                    {
                        m_CurrentDrawPoints.Add(Input.mousePosition);
                        m_CurrentLine.Points = m_CurrentDrawPoints.ToArray();
                        m_CurrentLine.SetAllDirty();
                    }
                }

                if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
                {
                    

                    foreach(Vector2 erasePoints in m_CurrentDrawPoints)
                    {
                        foreach (GameObject obj in m_AllDrawingLines)
                        {
                            UILineRenderer drawnLine = obj.GetComponent<UILineRenderer>();
                            List<Vector2> comparisonList = new List<Vector2>(drawnLine.Points);                            
                            foreach(Vector2 comparisonPoint in drawnLine.Points)
                            {
                                if (Mathf.Abs(comparisonPoint.x - erasePoints.x) <= 20 && Mathf.Abs(comparisonPoint.y - erasePoints.y) <= 20)
                                {
                                    m_LinesRemovalList.Add(obj);
                                    m_ScaleCanvasInstance.m_childObjects.Remove(obj.GetComponent<RectTransform>());
                                    Destroy(obj);
                                }
                            }                       
                        }

                        m_AllDrawingLines.RemoveAll(line => m_LinesRemovalList.Contains(line));
                        m_LinesRemovalList.Clear();

                    }

                    m_ScaleCanvasInstance.m_childObjects.Remove(m_CurrentLine.GetComponent<RectTransform>());
                    Destroy(m_CurrentLine.gameObject);

                    if (m_ModeChangeReady)
                    {
                        ToggleModes();
                        m_ModeChangeReady = false;
                    }
                }
                break;

            default:
                break;
        }
	}

    public void ButtonModePress()
    {
        m_ModeChangeReady = true;
    }

    private void ToggleModes()
    {
        if (m_CurrentDrawMode == DrawMode.Draw)
        {
            m_ScaleCanvasInstance.m_childObjects.Remove(m_CurrentLine.GetComponent<RectTransform>());
            m_AllDrawingLines.Remove(m_CurrentLine.gameObject);
            Destroy(m_CurrentLine.gameObject);
            m_CurrentDrawMode = DrawMode.Erase;
            m_DrawModeButton.text = "Erase Mode";
        } else if (m_CurrentDrawMode == DrawMode.Erase)
        {
            m_CurrentDrawMode = DrawMode.Draw;
            m_DrawModeButton.text = "Draw Mode";
        }
    }

    public void ChangeLineThickness()
    {
        m_lineRenderPrefab.GetComponent<UILineRenderer>().LineThickness = m_DefaultLineThickness*m_LineThicknessSlider.value;
        foreach (GameObject obj in m_AllDrawingLines)
        {
            obj.GetComponent<UILineRenderer>().LineThickness = m_DefaultLineThickness * m_LineThicknessSlider.value;
        }
    }

}
