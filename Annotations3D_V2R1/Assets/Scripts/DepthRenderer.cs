using UnityEngine;
using Windows.Kinect;
using System;
using System.Collections;

public class DepthRenderer : MonoBehaviour {

    public GameObject MultiSourceManager;
    private MultiSourceManager _MultiManager;

    private KinectSensor _Sensor;
    private CoordinateMapper _Mapper;

    // Vertex is limited to 65000, therefore limit ViewSpace
    // Full Depth data is 512 x 424 = 217,088, that is ~4x over limit
    public int m_ViewStartX = 150, m_ViewStartY = 180;
    [HideInInspector] // Must call InitialiseMemoryAndMesh() when changed
    public int m_ViewWidth = 250, m_ViewHeight = 200;
    [Range(0,1f)]
    public double m_DepthScaleFactor = 0.2f;
    private const int MAX_DEPTH = 1000;
    private float m_DepthOffset;
        
    private Mesh _Mesh;
    private Vector3[] _Vertices;
    private Vector2[] _UV;
    private int[] _Triangles;

    public bool m_enableColor = true;
    public bool m_enableDepth = true;
    public bool m_enableFiltering = true;
    private const int m_filterSize = 3; // Square filters, image processing
    private double[] m_imageFilter = new double[9] {
        0.125, 0.125, 0.125,
        0.125, 0, 0.125,
        0.125, 0.125, 0.125
    };

    public bool m_enableMovingAverage = true;
    [HideInInspector]
    public int m_NumAvgFrames = 30; // Must call InitialiseMemoryAndMesh() when changed
    private int m_currentFrameIndex = 0;
    private double[] m_bufferDepthArray;

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();
        if (_Sensor != null)
        {
            _Mapper = _Sensor.CoordinateMapper;
            //InitialiseMemoryAndMesh(); // Called in UIManager

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }
    }

    public void InitialiseMemoryAndMesh()
    {
        if (m_ViewHeight * m_ViewWidth > 65000)
        {
            Debug.LogError("[Error - ViewSize] Ensure width*height does not exceed 65000");
        }
        m_currentFrameIndex = 0;
        m_bufferDepthArray = new double[m_NumAvgFrames * m_ViewWidth * m_ViewHeight];
        m_DepthOffset = 1.5f * ((float)(MAX_DEPTH * m_DepthScaleFactor)) / 2.0f ;
        Debug.Log(m_DepthOffset + " offset");
        CreateMesh(m_ViewWidth, m_ViewHeight);
    }

    void CreateMesh(int width, int height)
    {
        _Mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _Mesh;

        _Vertices = new Vector3[width * height];
        _UV = new Vector2[width * height];
        _Triangles = new int[6 * ((width - 1) * (height - 1))];

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                _Vertices[index] = new Vector3(x-width/2, -y+height/2, 0);
                _UV[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

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

        //Debug.Log("[V3] VertexCount " + _Mesh.vertexCount);
        Debug.Log(String.Format("[V3] Mesh.VertexCount [{0}x{1}]={2}",
            m_ViewWidth, m_ViewHeight, _Mesh.vertexCount));


    }

    void Update()
    {

        if (MultiSourceManager == null)
        {
            return;
        }

        _MultiManager = MultiSourceManager.GetComponent<MultiSourceManager>();
        if (_MultiManager == null)
        {
            return;
        }

        if (m_enableColor)
        {
            gameObject.GetComponent<Renderer>().material.mainTexture = _MultiManager.GetColorTexture();
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.mainTexture = null;
        }

        RefreshData(_MultiManager.GetDepthData(),
                    _MultiManager.ColorWidth,
                    _MultiManager.ColorHeight);
    }

    private void RefreshData(ushort[] depthData, int colorWidth, int colorHeight)
    {
        var frameDesc = _Sensor.DepthFrameSource.FrameDescription;

        ColorSpacePoint[] colorSpace = new ColorSpacePoint[depthData.Length];
        _Mapper.MapDepthFrameToColorSpace(depthData, colorSpace);

        for (int y = 0; y < m_ViewHeight; y++)
        {
            for (int x = 0; x < m_ViewWidth; x++)
            {
                int vertex_index = y * m_ViewWidth + x;
                int depth_x = m_ViewStartX + x, depth_y = m_ViewStartY + y;
                int depth_index = (depth_y) * frameDesc.Width + (depth_x);
                int buffer_index = (m_currentFrameIndex) * m_ViewHeight * m_ViewWidth + vertex_index;
                
                
                double filterPixel = 0, depth_pixel = MAX_DEPTH, avg_sum = 0;
                if (m_enableDepth)
                {
                    // Averaging covolution filter
                    if (!m_enableFiltering)
                    {
                        if (depthData[depth_index] == 0)
                        {
                            filterPixel = MAX_DEPTH;
                        }
                        else
                        {
                            filterPixel = depthData[depth_index];
                        }
                    }
                    else
                    {
                        int filt_buff = (int)Math.Floor((double)m_filterSize / 2);
                        if (depth_y > filt_buff && depth_y < (frameDesc.Height - filt_buff)
                            && depth_x > filt_buff && depth_x < (frameDesc.Width - filt_buff))
                        {
                            int start_y = depth_y - filt_buff, start_x = depth_x - filt_buff;
                            for (int i = 0; i < m_filterSize; i++)
                            {
                                for (int j = 0; j < m_filterSize; j++)
                                {
                                    if (depthData[(start_y + i) * frameDesc.Width + (start_x + j)] == 0)
                                    {
                                        depth_pixel = MAX_DEPTH;
                                    }
                                    else
                                    {
                                        depth_pixel = depthData[(start_y + i) * frameDesc.Width + (start_x + j)];
                                    }
                                    filterPixel += m_imageFilter[i * m_filterSize + j] * depth_pixel;
                                }
                            }
                        }
                    }

                    // Moving Frame Average
                    m_bufferDepthArray[buffer_index] = filterPixel;
                    if (!m_enableMovingAverage)
                    {
                        avg_sum = filterPixel;
                    }
                    else
                    {
                        for (int i = 0; i < m_NumAvgFrames; i++)
                        {
                            avg_sum += m_bufferDepthArray[i * m_ViewHeight * m_ViewWidth + vertex_index];
                        }
                        avg_sum = avg_sum / m_NumAvgFrames;
                    }
                }
                
                _Vertices[vertex_index].z = (float)(avg_sum * m_DepthScaleFactor - m_DepthOffset);

                // Update UV mapping with CDRP
                var colorSpacePoint = colorSpace[depth_index];
                _UV[vertex_index] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }

        m_currentFrameIndex = (m_currentFrameIndex + 1) % m_NumAvgFrames;
        _Mesh.vertices = _Vertices;
        _Mesh.uv = _UV;
        _Mesh.triangles = _Triangles;
        _Mesh.RecalculateNormals();
    }

    void OnApplicationQuit()
    {
        if (_Mapper != null)
        {
            _Mapper = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
