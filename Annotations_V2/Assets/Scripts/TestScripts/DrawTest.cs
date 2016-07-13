using UnityEngine;
using System.Collections;

public class DrawTest : MonoBehaviour {

    public Color c1 = Color.yellow;
    public Color c2 = Color.red;
    public int lengthOfLineRenderer = 20;
    void Start()
    {
        LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(c1, c2);
        lineRenderer.SetWidth(0.2F, 0.2F);
        lineRenderer.SetVertexCount(lengthOfLineRenderer);
    }
    void Update()
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        Vector3[] points = new Vector3[lengthOfLineRenderer];
        float t = Time.time;
        int i = 0;
        while (i < lengthOfLineRenderer)
        {
            points[i] = new Vector3(i * 0.5F, Mathf.Sin(i + t), 0);
            i++;
        }
        lineRenderer.SetPositions(points);
    }
}
