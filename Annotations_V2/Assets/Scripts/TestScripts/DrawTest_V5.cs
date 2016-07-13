using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class DrawTest_V5 : MonoBehaviour {


    private LineRenderer line;
    private bool isMousePressed;
    public List<Vector3> pointsList;
    //public //
    //render texture and render camera
    private Vector3 mousePos;


    //    -----------------------------------    
    void Awake()
    {
        // Create line renderer component and set its property
        line = GetComponent<LineRenderer>();
        //line.material = new Material(Shader.Find("Particles/Additive"));
        line.material = new Material(Shader.Find("Self-Illumin/Diffuse"));
        line.SetVertexCount(1);
        line.SetWidth(1f, 1f);
        line.SetColors(Color.green, Color.green);
        line.useWorldSpace = true;
        isMousePressed = false;
        pointsList = new List<Vector3>();
    }

    //    -----------------------------------    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMousePressed = true;
            line.SetVertexCount(1);
            pointsList.RemoveRange(0, pointsList.Count);
            line.SetColors(Color.green, Color.green);
        }
        if (Input.GetMouseButtonUp(0))
        {
            isMousePressed = false;
        }
        // Drawing line when mouse is moving(presses)
        if (isMousePressed)
        {
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            //            if (!pointsList.Contains(mousePos))
            //          {
            Debug.Log("V" + Input.mousePosition);
            pointsList.Add(Input.mousePosition);
            line.SetVertexCount(pointsList.Count);
            line.SetPosition(pointsList.Count - 1, (Vector3)pointsList[pointsList.Count - 1]);

            //        }
        }
    }

}
