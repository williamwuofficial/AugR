using UnityEngine;
using UnityEngine.UI.Extensions;
using System.Collections;

public class LineScaler : MonoBehaviour {

    float m_ScreenWidth = 0f;
    float m_ScreenHeight = 0f;

    public RectTransform renderCanvas;
    public UILineTextureRenderer lineDraw;

    IEnumerator Start()
    {
        m_ScreenWidth = Screen.width;
        m_ScreenHeight = Screen.height;

        yield return new WaitForSeconds(1);

        ScaleLine();

    }


    void Update()
    {
        if (m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height)
        {
            ScaleLine();

            m_ScreenWidth = Screen.width;
            m_ScreenHeight = Screen.height;
        }
    }

    void ScaleLine()
    {

        Vector2[] points = { new Vector2(0,0), new Vector2(renderCanvas.rect.width / 2, renderCanvas.rect.height / 2)};
        lineDraw.Points = points;
        lineDraw.SetAllDirty();
    }
}
