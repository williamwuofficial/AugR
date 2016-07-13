using UnityEngine;
using System.Collections;

public class CanvasScaleLock : MonoBehaviour {

    float m_ScreenWidth = 0f;
    float m_ScreenHeight = 0f;

    public Canvas canvas;
    public RectTransform[] childCanvas;

    void Start () {

        Debug.Log("Displays connected: " + Display.displays.Length);
        // Display.displays[0] is the primary, default display and is always ON.
        // Check if additional displays are available and activate each.
        if (Display.displays.Length > 1)
            Display.displays[1].Activate();
        if (Display.displays.Length > 2)
            Display.displays[2].Activate();

        m_ScreenWidth = Screen.width;
        m_ScreenHeight = Screen.height;

        StartCoroutine(ScaleCanvas());
    }
	
	
	void Update () {
        if (m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height)
        {
            StartCoroutine(ScaleCanvas());

            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;
        }
    }

    IEnumerator ScaleCanvas()
    {

        //canvas.position = new Vector3(0,0,0);
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        yield return new WaitForEndOfFrame();
        canvas.renderMode = RenderMode.WorldSpace;

        foreach (RectTransform c_canvas in childCanvas)
        {
            c_canvas.offsetMin = new Vector2(0, 0);
            c_canvas.offsetMax = new Vector2(0, 0);
            c_canvas.localPosition = new Vector3(0, 0, -1500);
        }
        
    }
}
