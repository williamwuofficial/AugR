using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections.Generic;
using System.Collections;

public class ScaleCanvas : MonoBehaviour {

    float m_ScreenWidth = 0f;
    float m_ScreenHeight = 0f;

    public Canvas m_annotationCanvas;
    public List<RectTransform> m_childObjects;

    void Start()
    {
        m_ScreenWidth = Screen.width;
        m_ScreenHeight = Screen.height;

        if (m_childObjects == null)
        {
            m_childObjects = new List<RectTransform>();
        }
        StartCoroutine(ScaleAllCanvas());
    }


    void Update()
    {
        if (m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height)
        {
            StartCoroutine(ScaleAllCanvas());

            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;
        }
    }

    public IEnumerator ScaleAllCanvas()
    {
        //yield return new WaitForSeconds(0.1f);
        //canvas.position = new Vector3(0,0,0);
        m_annotationCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        yield return new WaitForEndOfFrame();
        m_annotationCanvas.renderMode = RenderMode.WorldSpace;

        foreach (RectTransform c_canvas in m_childObjects)
        {
            c_canvas.offsetMin = new Vector2(0, 0);
            c_canvas.offsetMax = new Vector2(0, 0);
            c_canvas.localScale = new Vector3(1, 1, 1);
            //c_canvas.rotation = Quaternion.Euler(0, 0, 0);
            //c_canvas.Rotate(0, 0, 0, Space.World);

            c_canvas.transform.localEulerAngles = new Vector3(0, 0, 0);
            c_canvas.localPosition = new Vector3(0, 0, 0);
        }

        Component[] c_Lines;
        c_Lines = m_annotationCanvas.GetComponentsInChildren<UILineRenderer>();
        foreach (UILineRenderer m_Line in c_Lines)
        {
            m_Line.color = new Color(m_Line.color.r, m_Line.color.g, m_Line.color.b, 255);
        }
    }
}
