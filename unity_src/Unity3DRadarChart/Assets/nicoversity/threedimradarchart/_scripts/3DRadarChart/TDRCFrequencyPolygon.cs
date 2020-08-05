using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing a 2D Frequency Polygon (i.e., a 2D frequency chart), used as an individual data variable (i.e., axis) in the 3D Radar Chart.
/// </summary>
public class TDRCFrequencyPolygon : MonoBehaviour
{
    /// <summary>
    /// Enum representing different data value scaling options (used for the data transformation in the visualization as a Frequency Polygon).
    /// </summary>
    public enum DataValueScaling : int
    {
        NoScaling = 0,                  // no scaling
        Linear = 1,                     // linear scaling, based on a linear scaling factor specified in the overall inteface
        Logarithmic_Base2 = 2           // logarithmic scaling with base 2
    }


    [Header("Properties")]
    public Vector3 origin;                                                                                              // origin coordinates in the 3D space representing the start point (i.e., anchor) of the Frequency Polygon
    public float length;                                                                                                // length (of data points) of the visualized Frequency Polygon

    [Header("Points")]
    public Transform pointsRef;                                                                                         // reference to the Transform that is going to have all FrequencyPointComponents attached
    public Color32 pointColor;                                                                                          // color value representing the color of all FrequencyPointComponents of the Frequency Polygon

    [Header("LineRenderer")]
    private LineRenderer lineRenderer;                                                                                  // line renderer component that is going to visualize the outline of the Frequency Polygon
    [Range(0.0f, 1.0f)]
    public float lineWidth;                                                                                             // value representing the width of the line renderer

    [Header("UI Polygon")]
    public CanvasRenderer canvasRendererRef;                                                                            // reference to the CanvasRenderer (in the UI layer) that is going to visualize the Frequency Polygon
    public Color32 polygonColor;                                                                                        // color value representing the color of the Frequency Polygon        

    // private propeties
    private ThreeDimRadarChartInterface tdrcInterface;                                                                  // reference to overall interface
    private static string frequencyPointSpherePrefabRef = "3D_Radar_Chart-resources/Prefabs/FrequencyPointSphere";      // reference to the prefab representing an individual Frequency Point in the Frequency Polygon (loaded from the application's "Assets/Resources" directory)
    private List<TDRCFrequencyPointComponent> fpcList;                                                                  // reference to all FrequencyPoint components of the Frequency Polygon


    /// <summary>
	/// Constructor, called before Start().
	/// </summary>
    private void Awake()
    {
        // init properties
        fpcList = new List<TDRCFrequencyPointComponent>();
    }


    #region PROPERTY_SETTERS

    /// <summary>
    /// Method to keep track of a reference to the 3D Radar Chart's main interface.
    /// </summary>
    /// <param name="tdrcInterfaceRef">Reference to the ThreeDimRadarChartInterface instance.</param>
    public void setTDRCInterface(ThreeDimRadarChartInterface tdrcInterfaceRef)
    {
        tdrcInterface = tdrcInterfaceRef;
    }

    #endregion


    #region DATA_HANDLER

    /// <summary>
	/// Inititalize the FrequencyPolygon.
	/// </summary>
	/// <param name="fpl">List of all FrequencyPoints representing the FrequencyPolygon.</param>
    public void initWithFrequencyPointList(List<TDRCFrequencyPoint> fpl)
    {
        initFrequencyPolygonFromListData(fpl);
    }

    /// <summary>
	/// Helper method: Convert a list of FrequencyPoints into a Vector3 array containing each point's position.
	/// </summary>
	/// <param name="fpl">Inut List of Frequency Points.</param>
	/// <returns>Vector3 Array containing all positions of the input list's elements.</returns>
    private Vector3[] frequencyPointPositionsToArray(List<TDRCFrequencyPoint> fpl)
    {
        Vector3[] positions = new Vector3[fpl.Count];
        for (int i = 0; i < fpl.Count; i++)
        {
            positions[i] = fpl[i].pos;
        }
        return positions;
    }

    /// <summary>
	/// Function to access the internal data structure.
	/// </summary>
	/// <returns>List of FrequencyPointComponents representing the FrequencyPolygon.</returns>
    public List<TDRCFrequencyPointComponent> getFrequencyPointComponentList()
    {
        return fpcList;
    }

    /// <summary>
    /// Retrieve reference to the FrequencyPointComponent at the currently selected index.
    /// </summary>
    /// <returns>Reference to FrequencyPointComponent.</returns>
    public TDRCFrequencyPointComponent getFrequencyPointComponentForCurrentSelectedTimeIndex()
    {
        return getFrequencyPointComponentForIndex(tdrcInterface.stt_selectedTimeIndex);
    }

    /// <summary>
    /// Retrieve reference to FrequencyPointComponent at a given index.
    /// </summary>
    /// <param name="index">Index for which the FrequencyPointComponent is asked for.</param>
    /// <returns>Reference to FrequencyPointComponent at referred index.</returns>
    public TDRCFrequencyPointComponent getFrequencyPointComponentForIndex(int index)
    {
        return fpcList[index];
    }

    /// <summary>
	/// Retrieve reference to FrequencyPoint data structure of the FrequencyPointComponent at the currently selected index.
	/// </summary>
	/// <returns>Reference to FrequencyPoint data structure.</returns>
    public TDRCFrequencyPoint GetFrequencyPointForCurrentSelectedTimeIndex()
    {
        return getFrequencyPointForIndex(tdrcInterface.stt_selectedTimeIndex);
    }

    /// <summary>
	/// Retrieve reference to FrequencyPoint data structure of the FrequencyPointComponent at a given index.
    /// </summary>
    /// <param name="index">Index for which the FrequencyPoint is asked for.</param>
    /// <returns>Reference to Frequency Point at referred index.</returns>
    public TDRCFrequencyPoint getFrequencyPointForIndex(int index)
    {
        return fpcList[index].fp;
    }

    /// <summary>
    /// Retrieve reference to the last FrequencyPoint data structure in the data series.
    /// </summary>
    /// <returns>Reference to the last Frequency Point.</returns>
    public TDRCFrequencyPoint getFrequencyPointForLastIndex()
    {
        return fpcList[fpcList.Count - 1].fp;
    }

    /// <summary>
    /// Retrieve the total amount of Frequency Points (= data points in the data set).
    /// </summary>
    /// <returns>Int value representing the amount (i.e., count) of data points in the Frequency Polygon.</returns>
    public int getFrequencyPointsCount()
    {
        return fpcList.Count;
    }

    #endregion


    #region DISPLAY

    /// <summary>
	/// Initialize the display / drawing of the FrequencyPolygon based on its input data.
	/// </summary>
	/// <param name="fpl">List of FrequencyPoints representing the Frequency Polygon.</param>
    private void initFrequencyPolygonFromListData(List<TDRCFrequencyPoint> fpl)
    {
        // init FrequencyPointComponents
        for (int i = 0; i < fpl.Count; i++)
        {
            // calculate and set position
            float yPos = getDataValueForVisualizationBasedOnTDRCInterfaceConfig(fpl[i].value, tdrcInterface);       // transform original data value based on configured options
            fpl[i].pos = new Vector3(i * tdrcInterface.cnfg2d_frequencyPointDistance, yPos, 0.0f);                  // distance between data points as configures in the 3D Radar Chart's Interface
            //fpl[i].pos = new Vector3(i, yPos, 0.0f);                                                              // distance between data points is default (i.e., 1.0f)
            fpl[i].color    = pointColor;                                                                           // keep track of color configuration

            // setup new GameObject
            Transform p     = Instantiate((Resources.Load(frequencyPointSpherePrefabRef) as GameObject).transform);
            p.parent        = pointsRef;
            p.localPosition = fpl[i].pos;
            p.localScale    = Vector3.one * tdrcInterface.cnfg2d_frequencyPointScale;
            p.name          = fpl[i].dimension + "_" + fpl[i].time;
            p.gameObject.GetComponent<Renderer>().material.color = pointColor;

            // attach and set FrequencyPoint data structure to GameObject
            TDRCFrequencyPointComponent fpc = p.gameObject.AddComponent<TDRCFrequencyPointComponent>();
            fpc.fp = fpl[i];

            // keep track of all instantiated FrequencyPointComponents
            fpcList.Add(fpc);
        }

        // update length property for drawing
        length = fpcList[fpcList.Count - 1].transform.localPosition.x;

        // init LineRenderer component
        if (tdrcInterface.cnfg2d_isFrequencyPolygonLineEnabled)
        {
            // initialize all LineRenderer component related properties
            lineRenderer = pointsRef.gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth     = lineWidth;
            lineRenderer.endWidth       = lineWidth;
            lineRenderer.useWorldSpace  = false;
            lineRenderer.material       = new Material(Shader.Find("Standard"));
            lineRenderer.material.color = pointColor;

            // update LineRenderer's positions
            //

            // line renderer only using original points
            //lr.positionCount = fpl.Count;
            //lr.SetPositions(frequencyPointPositionsToArray(fpl));

            // line renderer incl. connecting to the 2D Frequency Polygon origin axis
            lineRenderer.positionCount = fpl.Count + 2;
            Vector3[] lrPoints = new Vector3[lineRenderer.positionCount];
            lrPoints[0] = new Vector3(origin.x, origin.y, origin.z);                                                // add one point at the start
            for (int i = 1; i <= fpl.Count; i++)
            {
                lrPoints[i] = fpl[i - 1].pos;                                                                       // add points for the Frequency Polygon
            }
            lrPoints[lineRenderer.positionCount - 1] = new Vector3(fpl[fpl.Count - 1].pos.x, origin.y, origin.z);   // add one point at the end
            lineRenderer.SetPositions(lrPoints);                                                                    // set line renderer positions
        }

        // init mesh for renderering the FrequencyPolygon (in the UI layer)
        if (tdrcInterface.cnfg2d_isFrequencyPolygonEnabled)
        {
            // init mesh and required variables
            Mesh polygonMesh              = new Mesh();
            List<Vector2> meshPoints      = new List<Vector2>();
            List<int> indices             = null;
            List<Vector3> vertices        = null;
            List<List<Vector2>> holesList = new List<List<Vector2>>();

            // setup mesh coordinates
            Vector3[] originalPositions = frequencyPointPositionsToArray(fpl);
            meshPoints.Add(new Vector2(origin.x, origin.y));                                            // add one point at origin at the start of the polygon
            foreach (Vector3 v in originalPositions)
            {
                meshPoints.Add(new Vector2(v.x, v.y));                                                  // add points for the Frequency Polygon
            }
            meshPoints.Add(new Vector2(originalPositions[originalPositions.Length - 1].x, origin.y));   // add one end point at the end of the polygon

            // perform Triangulation
            // Developer Note: This is based on the implemented PolyExtruder package, Source: https://github.com/nicoversity/unity_polyextruder 
            Triangulation.triangulate(meshPoints, holesList, 0.0f, out indices, out vertices);

            // update mesh with new data
            polygonMesh.Clear();
            polygonMesh.vertices  = vertices.ToArray();
            polygonMesh.triangles = indices.ToArray();
            polygonMesh.RecalculateNormals();
            polygonMesh.RecalculateBounds();

            // setup mesh in the UI GameObject
            canvasRendererRef.SetMesh(polygonMesh);

            // setup the mesh's material
            Material polygonMeshMat = new Material(Shader.Find("UI/Default"));
            polygonMeshMat.color = polygonColor;
            canvasRendererRef.SetMaterial(polygonMeshMat, null);

            // rotate due to Triangulation (i.e., PolyExtruder) implementation 
            canvasRendererRef.transform.Rotate(Vector3.right * -90.0f);


            // IN_ENGINE_SCREENSHOT_CAPTURE
            // Developer Note: Enable this to capture polygons in 360 screenshot capture.
            //MeshFilter mf = canvasRendererRef.gameObject.GetComponent<MeshFilter>();
            //if (mf == null) mf = canvasRendererRef.gameObject.AddComponent<MeshFilter>();
            //mf.mesh = polygonMesh;
            //MeshRenderer mr = canvasRendererRef.gameObject.GetComponent<MeshRenderer>();
            //if (mr == null) mr = canvasRendererRef.gameObject.AddComponent<MeshRenderer>();
            //mr.material = polygonMeshMat;
        }
    }

    #endregion


    #region DATA_VALUE_TRANSFORMATION

    /// <summary>
    /// Method used for data value transformation (normalization or scaling) based on the configuration of the 3D Radar Chart's Interface. (Note: Data value is just transformed for visualization, i.e., no changes in the actual data are performed.)
    /// </summary>
    /// <param name="dataValue">Float value representing the original data value that is going to be transformed (normalized or scaled) for visualization.</param>
    /// <param name="tdrci">Reference to ThreeDimRadarChartInterface instance used for configuration.</param>
    /// <returns>Float value representing the transformed original data value.</returns>
    public static float getDataValueForVisualizationBasedOnTDRCInterfaceConfig(float dataValue, ThreeDimRadarChartInterface tdrci)
    {
        // check whether to apply normalization
        if (tdrci.cnfg2d_isFrequencyPolygonVisualizationNormalized)
        {
            return getDataValueForVisualizationBasedOnConfiguredNormalization(dataValue, tdrci) * tdrci.cnfg2d_frequencyPolygonNormalizationScaleFactor;
        }
        // if not: apply scaling based on configuration
        else
        {
            return getDataValueForVisualizationBasedOnConfiguredScaling(dataValue, tdrci);
        }
    }

    /// <summary>
    /// Method used for data value normalization based on the configuration of the 3D Radar Chart's Interface. (Note: Data value is just transformed for visualization, i.e., no changes in the actual data are performed.)
    /// </summary>
    /// <param name="dataValue">Float value representing the original data value that is going to be normalized for visualization.</param>
    /// <param name="tdrci">Reference to ThreeDimRadarChartInterface instance used for configuration.</param>
    /// <returns>Float value representing the normalized original data value (data value range = 0 to 1).</returns>
    private static float getDataValueForVisualizationBasedOnConfiguredNormalization(float dataValue, ThreeDimRadarChartInterface tdrci)
    {
        return (dataValue - tdrci.cnfg2d_frequencyPolygonMinimumDataValueForNormalization) / (tdrci.cnfg2d_frequencyPolygonMaximumDataValueForNormalization - tdrci.cnfg2d_frequencyPolygonMinimumDataValueForNormalization);
    }

    /// <summary>
    /// Method used for data value scaling based on the configuration of the 3D Radar Chart's Interface. (Note: Data value is just transformed for visualization, i.e., no changes in the actual data are performed.)
    /// </summary>
    /// <param name="dataValue">Float value representing the original data value that is going to be scaled for visualization.</param>
    /// <param name="tdrci">Reference to ThreeDimRadarChartInterface instance used for configuration.</param>
    /// <returns>Float value representing the scaled original data value.</returns>
    private static float getDataValueForVisualizationBasedOnConfiguredScaling(float dataValue, ThreeDimRadarChartInterface tdrci)
    {
        float scaledDataValue;
        switch (tdrci.cnfg2d_frequencyPolygonDataValueScaling)
        {
            case DataValueScaling.NoScaling:
            default:
                scaledDataValue = (dataValue > 0) ? dataValue : 0.0f;
                break;

            case DataValueScaling.Linear:
                scaledDataValue = (dataValue > 0) ? dataValue * tdrci.cnfg2d_linearScaleFactor : 0.0f;
                break;

            case DataValueScaling.Logarithmic_Base2:
                scaledDataValue = (dataValue > 0) ? Mathf.Log(dataValue, 2) : 0.0f;
                break;
        }
        return scaledDataValue;
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Method that displays all FrequencyPoints of the FrequencyPolygon.
    /// </summary>
    /// <param name="isDisplayed">Flag to determine whether all FrequencyPoints should be displayed (true) or hidden (false).</param>
    public void displayAllFrequencyPoints(bool isDisplayed)
    {
        foreach (TDRCFrequencyPointComponent fpc in fpcList)
        {
            fpc.GetComponent<Renderer>().enabled = isDisplayed;
        }
    }

    /// <summary>
	/// Method that displays a FrequencyPointComponent of the FrequencyPolygon at a specified index.
	/// </summary>
	/// <param name="index">Index of which the FrequencyPointComponent is to be displayed.</param>
	/// <param name="isActivated">Flag to determine whether the FrequencyPointComponent should be displayed (true) or hidden (false).</param>
    public void displayFrequencyPointForIndex(int index, bool isActivated)
    {
        // check whether the specifed index is valid
        if (index < fpcList.Count && index >= 0)
        {
            if (isActivated)
            {
                fpcList[tdrcInterface.stt_previousSelectedTimeIndex].GetComponent<Renderer>().enabled = false;      // disable the old (i.e., previous) one   
                fpcList[tdrcInterface.stt_selectedTimeIndex].GetComponent<Renderer>().enabled = true;               // enable the new one
            }
            else
            {
                fpcList[tdrcInterface.stt_selectedTimeIndex].GetComponent<Renderer>().enabled = false;              // disable the current one
            }
        }
    }

    /// <summary>
    /// Retrieve positions of the Frequency Points for a specified range of indices.
    /// </summary>
    /// <param name="fpcl">List of FrequencyPointComponent instances representing the input data.</param>
    /// <param name="startIndex">Start index (including).</param>
    /// <param name="endIndex">End index (including).</param>
    /// <returns>Vector3 array containing the positions for the selected range of indices.</returns>
    private Vector3[] frequencyPointPositionsToArrayForRange(List<TDRCFrequencyPointComponent> fpcl, int startIndex, int endIndex)
    {
        Vector3[] positions = new Vector3[endIndex - startIndex + 1];
        for (int i = 0; i <= endIndex - startIndex; i++)
        {
            positions[i] = fpcl[startIndex + i].fp.pos;
        }
        return positions;
    }

    /// <summary>
    /// Function to display only a specified range (i.e., subset) of indices of the FrequencyPolygons.
    /// </summary>
    /// <param name="startIndex">Start index (including).</param>
    /// <param name="endIndex">End index (including).</param>
    public void displayRange(int startIndex, int endIndex)
    {
        // get positions for selected range
        Vector3[] rangePositions = frequencyPointPositionsToArrayForRange(fpcList, startIndex, endIndex);

        // update LineRenderer's positions (if required)
        if (tdrcInterface.cnfg2d_isFrequencyPolygonLineEnabled)
        {
            // line renderer incl. connecting to the 2D Frequency Polygon origin axis
            lineRenderer.positionCount = rangePositions.Length + 2;
            Vector3[] lrPoints = new Vector3[lineRenderer.positionCount];
            lrPoints[0] = new Vector3(rangePositions[0].x, origin.y, origin.z);                                                         // add one point at the start
            for (int i = 1; i <= rangePositions.Length; i++)
            {
                lrPoints[i] = rangePositions[i - 1];                                                                                    // add points for the Frequency Polygon
            }
            lrPoints[lineRenderer.positionCount - 1] = new Vector3(rangePositions[rangePositions.Length - 1].x, origin.y, origin.z);    // add one point at the end
            lineRenderer.SetPositions(lrPoints);                                                                                        // set line renderer positions
        }

        // mesh for renderering the FrequencyPolygon in the UI layer
        if (tdrcInterface.cnfg2d_isFrequencyPolygonEnabled)
        {
            // init mesh and required variables
            Mesh polygonMesh              = new Mesh();
            List<Vector2> meshPoints      = new List<Vector2>();
            List<int> indices             = null;
            List<Vector3> vertices        = null;
            List<List<Vector2>> holesList = new List<List<Vector2>>();

            // setup mesh coordinates
            meshPoints.Add(new Vector2(rangePositions[0].x, origin.y));                             // add one point at origin at the start of the polygon
            foreach (Vector3 v in rangePositions)
            {
                meshPoints.Add(new Vector2(v.x, v.y));                                              // add points for the Frequency Polygon
            }
            meshPoints.Add(new Vector2(rangePositions[rangePositions.Length - 1].x, origin.y));     // add one end point at the end of the polygon

            // perform Triangulation
            // Developer Note: This is based on the implemented PolyExtruder package, Source: https://github.com/nicoversity/unity_polyextruder 
            Triangulation.triangulate(meshPoints, holesList, 0.0f, out indices, out vertices);

            // update mesh with new data
            polygonMesh.Clear();
            polygonMesh.vertices  = vertices.ToArray();
            polygonMesh.triangles = indices.ToArray();
            polygonMesh.RecalculateNormals();
            polygonMesh.RecalculateBounds();

            // setup mesh in the UI GameObject
            canvasRendererRef.SetMesh(polygonMesh);


            // IN_ENGINE_SCREENSHOT_CAPTURE
            // Developer Note: Enable this to capture polygons in 360 screenshot capture.
            //MeshFilter mf = canvasRendererRef.gameObject.GetComponent<MeshFilter>();
            //if (mf == null) mf = canvasRendererRef.gameObject.AddComponent<MeshFilter>();
            //mf.mesh = polygonMesh;
            //MeshRenderer mr = canvasRendererRef.gameObject.GetComponent<MeshRenderer>();
            //if (mr == null) mr = canvasRendererRef.gameObject.AddComponent<MeshRenderer>();
        }
    }

    #endregion


    #region HELPER

    /// <summary>
    /// Setup a Color32 instance based on a hexadecimal color value.
    /// </summary>
    /// <param name="hex">Color represented in hexadecimal notation (without leading #).</param>
    /// <param name="alpha">Transparency (alpha) value of the color (0 - 255).</param>
    /// <returns>Color32.</returns>
    public static Color32 colorWithHexAndAlpha(string hex, int alpha)
    {
        // via wiki.unity3d.com/index.php?title=HexConverter
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

        return new Color32(r, g, b, (byte)alpha);
    }

    #endregion
}
