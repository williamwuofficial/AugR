using UnityEngine;
//using UnityEngine.
using System;
using System.Collections;

public class WarpImage : MonoBehaviour {

    public GameObject m_RenderSurface;
    public Camera m_ViewCamera;


    public Vector4 _Column0;
    public Vector4 _Column1;
    public Vector4 _Column2;
    public Vector4 _Column3;
    private Matrix4x4 _WarpMatrix = new Matrix4x4();
    private Vector3[] _WarpVertices;

    public Vector3 _Row0 = new Vector3(0.8660f, 0.5000f, 0.0001f);
    public Vector3 _Row1 = new Vector3(-1.5000f, 0.8660f,0.0001f);
    public Vector3 _Row2 = new Vector3(5.0000f, 10.0000f, 1.0000f);
    
    /*[Header("Ortho Matrix")]
    public bool m_enableOth;
    public float oth_left, oth_right, oth_bottom, oth_top, oth_znear, oth_zfar;
    private Matrix4x4 m_othMatrix;
    
    [Header("Perspective Matrix")]
    public bool m_enablePer;
    public float per_fov = 45, per_aspect = 1.5f, per_znear = 10, per_zfar = 20;
    private Matrix4x4 m_perMatrix;

    [Header("TRS Matrix")]
    public bool m_enableTRS;
    public Vector3 trs_pos;
    public Quaternion trs_qrt;
    public Vector3 trs_scale = new Vector3(1, 1, 1);
    private Matrix4x4 m_trsMatrix;*/



    [Header("Mesh Settings")]
    public Vector2 m_MeshSize = new Vector2(10, 5);
    private int _CanvasScale = 10;
    private Mesh _Mesh;
    public Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;
    
    void Start()
    {
        WebCamTexture webcamTexture = new WebCamTexture();
        m_RenderSurface.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.Play();

        CreateMesh(m_RenderSurface, (int) m_MeshSize.x, (int) m_MeshSize.y);


        _Column0 = transform.right;
        _Column1 = transform.up;
        _Column2 = transform.forward;
        _Column3 = new Vector4(transform.position.x, transform.position.y, transform.position.z, 1);
        //m_othMatrix = Matrix4x4.Ortho(oth_left, oth_right, oth_bottom, oth_top, oth_znear, oth_zfar);
        //m_perMatrix = Matrix4x4.Perspective(per_fov, per_aspect, per_znear, per_zfar);
        //m_trsMatrix = Matrix4x4.TRS(trs_pos, trs_qrt, trs_scale);
        _WarpVertices = new Vector3[_Vertices.Length];

        transform.position = new Vector3(transform.position.x, transform.position.y, -60);

    }

    void WarpCalibration()
    {
        WarpMesh(_Vertices);

        // scale and center

    }

    void Update()
    {
        //m_othMatrix = Matrix4x4.Ortho(oth_left, oth_right, oth_bottom, oth_top, oth_znear, oth_zfar);
        //m_perMatrix = Matrix4x4.Perspective(per_fov, per_aspect, per_znear, per_zfar);
        //m_trsMatrix = Matrix4x4.TRS(trs_pos, trs_qrt, trs_scale);

        _WarpMatrix.SetColumn(0, _Column0);
        _WarpMatrix.SetColumn(1, _Column1);
        _WarpMatrix.SetColumn(2, _Column2);
        _WarpMatrix.SetColumn(3, _Column3);

        WarpMesh(_Vertices);
    }

    void CreateMesh(GameObject surface, int width, int height)
    {
        _Mesh = new Mesh();
        surface.GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[width * height];
        _UV = new Vector2[width * height];
        _Triangles = new int[6 * ((width - 1) * (height - 1))];

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                _Vertices[index] = new Vector3(x*_CanvasScale, -y*_CanvasScale, 0);
                _UV[index] = new Vector2((float)x / ((float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    _Triangles[triangleIndex++] = topLeft;
                    _Triangles[triangleIndex++] = (index % 2 == 0) ? topRight : bottomRight;
                    _Triangles[triangleIndex++] = bottomLeft;
                    _Triangles[triangleIndex++] = (index % 2 == 0) ? bottomLeft : topLeft;
                    _Triangles[triangleIndex++] = topRight;
                    _Triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
        _Mesh.RecalculateBounds();
    }

    void WarpMesh(Vector3[] meshVertices)
    {
        int i = 0;

        //Matrix4x4 total = m_perMatrix;

        while (i < meshVertices.Length)
        {
            _WarpVertices[i] = _WarpMatrix.MultiplyPoint3x4(meshVertices[i]);
            _WarpVertices[i].x = _WarpVertices[i].x / _WarpVertices[i].z;
            _WarpVertices[i].y = _WarpVertices[i].y / _WarpVertices[i].z;



            i++;
        }
        
        _Mesh.vertices = _WarpVertices;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
        _Mesh.RecalculateBounds();
    }

}
