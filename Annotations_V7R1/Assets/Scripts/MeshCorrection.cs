using UnityEngine;
using System.Collections;

public class MeshCorrection : MonoBehaviour {

    public Camera m_ViewCamera;
    public GameObject m_RenderSurface;
    [Header("Warp Matrix Values")]
    [Range(0, 360)]
    public float _MeshRotation;
    [Range(10,100)]
    public float _MeshXScale, _MeshYScale;
    [Range(-30, 30)]
    public float _MeshXShear, _MeshYShear;
    [Range(-1.5f, 3)]
    public float _MeshXProj;
    [Range(-3, 1.5f)]
    public float _MeshYProj;

    public Vector3 _Row0;
    public Vector3 _Row1;
    public Vector3 _Row2;
    private Vector4 _Column3;
    private Matrix4x4 _WarpMatrix = new Matrix4x4();
    private Vector3[] _newVertices;
    
    [Header("Mesh Settings")]
    private Vector2 m_MeshSize = new Vector2(11, 7);
    private int _CanvasScale = 10;
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;

    void Start () {
        WebCamTexture webcamTexture = new WebCamTexture();
        m_RenderSurface.GetComponent<Renderer>().material.mainTexture = webcamTexture;
        webcamTexture.Play();
        CreateMesh(m_RenderSurface, (int)m_MeshSize.x, (int)m_MeshSize.y);
        
        _Row0 = new Vector3(_CanvasScale, 0, 0);
        _Row1 = new Vector3(0, _CanvasScale, 0);
        _Row2 = new Vector3(0, 0, 100);
        _Column3 = new Vector4(0, 0, 100, 0);
        _newVertices = new Vector3[_Vertices.Length];
        transform.position = new Vector3(transform.position.x, transform.position.y, 100);

        WarpMesh();
    }
	
	void Update () {
        _Row0 = new Vector3(_MeshXScale, _MeshXShear, 0);
        _Row1 = new Vector3(_MeshYShear, _MeshYScale, 0);
        _Row2 = new Vector3(_MeshXProj, _MeshYProj, 100);
        WarpMesh();
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

                _Vertices[index] = new Vector3(x * _CanvasScale, -y * _CanvasScale, 0);
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

    void WarpMesh()
    {
        _WarpMatrix.SetRow(0, _Row0);
        _WarpMatrix.SetRow(1, _Row1);
        _WarpMatrix.SetRow(2, _Row2);
        _WarpMatrix.SetColumn(3, _Column3);

        int i = 0;
        while (i < _Vertices.Length)
        {
            _newVertices[i] = _WarpMatrix.MultiplyPoint3x4(_Vertices[i]);
            _newVertices[i].x = _newVertices[i].x / _newVertices[i].z;
            _newVertices[i].y = _newVertices[i].y / _newVertices[i].z;
            _newVertices[i].z = 0;
            i++;
        }

        _Mesh.vertices = _newVertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
        _Mesh.RecalculateBounds();
    }
}
