using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for rendering the Time Slice of a 3D Radar Chart, i.e., a mesh (visualized as 2D Radar Chart) connecting the data items of all data varibles at the currently selected index.
/// </summary>
public class TDRCTimeSliceRenderer : MonoBehaviour
{
    [Header("Component References")]
    public CanvasRenderer timeSliceCanvasRenderer;                  // reference to the CanvasRenderer (in the UI layer) that is going to visualize the Time Slice

    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;              // reference to overall interface
    private TDRCFrequencyPolygonManager tdrcFreqPolyManager;        // reference to the instance managing all data variable axes of the 3D Radar Chart   


    #region PROPERTY_SETTERS

    /// <summary>
    /// Method to keep track of a reference to the 3D Radar Chart's main interface.
    /// </summary>
    /// <param name="tdrcInterfaceRef">Reference to the ThreeDimRadarChartInterface instance.</param>
    public void setTDRCInterface(ThreeDimRadarChartInterface tdrcInterfaceRef)
    {
        tdrcInterface = tdrcInterfaceRef;
    }

    /// <summary>
    /// Method to keep track of a reference to the 3D Radar Chart's Frequency Polygon Manager instance.
    /// </summary>
    /// <param name="tdrcFreqPolyManagerRef">Reference to the ThreeDimRadarChartInterface's Frequency Polygon Manager instance.</param>
    public void setTDRCFreqPolyManager(TDRCFrequencyPolygonManager tdrcFreqPolyManagerRef)
    {
        tdrcFreqPolyManager = tdrcFreqPolyManagerRef;
    }

    #endregion


    #region DISPLAY

    /// <summary>
    /// Initialize the display / drawing of the 2D Radar Chart based on the currently selected time index.
    /// </summary>
    /// <returns>Reference to the created mesh.</returns>
    public Mesh displayRadar()
    {
        return displayRadarForFrequencyPointComponentList(tdrcFreqPolyManager.getFrequencyPointComponentListForCurrentlySelectedTimeIndex(), tdrcInterface.cnfgTmSlc_radarColor);
    }

    /// <summary>
    /// Initialize the display / drawing of the 2D Radar Chart based on the currently selected time index and a given color.
    /// </summary>
    /// <param name="polygonColor">Color32 value representing the color used for the created 2D Radar Chart mesh.</param>
    /// <returns>Reference to the created mesh.</returns>
    public Mesh displayRadarWithColor(Color32 polygonColor)
    {
        return displayRadarForFrequencyPointComponentList(tdrcFreqPolyManager.getFrequencyPointComponentListForCurrentlySelectedTimeIndex(), polygonColor);
    }

    /// <summary>
    /// Initialize the display / drawing of the 2D Radar Chart based on a given time index.
    /// </summary>
    /// <param name="index">Int value representing the time index for drawing the Mesh.</param>
    /// <returns>Reference to the created mesh.</returns>
    public Mesh displayRadarForIndex(int index)
    {
        return displayRadarForFrequencyPointComponentList(tdrcFreqPolyManager.getFrequencyPointComponentListForIndex(index), tdrcInterface.cnfgTmSlc_radarColor);
    }

    /// <summary>
    /// Method to create the mesh representing the 2D Radar Chart.
    /// </summary>
    /// <param name="fpcList">List of FrequencyPointComponent instances representing the 2D Radar Chart.</param>
    /// <param name="polygonColor">Color32 value representing the color used for the created 2D Radar Chart mesh.</param>
    /// <returns>Reference to the created mesh.</returns>
    private Mesh displayRadarForFrequencyPointComponentList(List<TDRCFrequencyPointComponent> fpcList, Color32 polygonColor)
    {
        // init mesh and required variables
        Mesh polygonMesh              = new Mesh();
        List<Vector2> meshPoints      = new List<Vector2>();
        List<int> indices             = null;
        List<Vector3> vertices        = null;
        List<List<Vector2>> holesList = new List<List<Vector2>>();

        // setup mesh coordinates
        for (int i = 0; i < fpcList.Count; i++)
        {
            float yPos = TDRCFrequencyPolygon.getDataValueForVisualizationBasedOnTDRCInterfaceConfig(fpcList[i].fp.value, tdrcInterface);  // transform original data value based on configured options
            Vector2 pos = RotateVector(new Vector2(0.0f, yPos), 360.0f / fpcList.Count * i);
            meshPoints.Add(pos);
        }

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
        // Note: TimeSliceRenderer GameObject needs to have the correct rotation (see below) and scale (inherited from interface configuration):
        //  - Rotation: x: 0, y: -90, z: 180
        this.transform.localEulerAngles = new Vector3(0.0f, -90.0f, 180.0f);
        this.transform.localScale = Vector3.one * tdrcInterface.cnfg3d_scale;
        timeSliceCanvasRenderer.SetMesh(polygonMesh);

        // setup the mesh's material
        Material polygonMeshMat = new Material(Shader.Find("UI/Default"));
        //Material polygonMeshMat = new Material(Shader.Find("UI/Unlit/Transparent"));      // TODO - for some reason this shader does not load in the compiled application
        polygonMeshMat.color = polygonColor;
        timeSliceCanvasRenderer.SetMaterial(polygonMeshMat, null);

        // return created Mesh (for other elements to use)
        return polygonMesh;
    }

    #endregion


    #region HELPER

    /// <summary>
    /// Helper method to rotate a Vector2 according to a specified angle.
    /// </summary>
    /// <param name="v">Vector2 to be rotated.</param>
    /// <param name="angle">Rotation angle in degrees.</param>
    /// <returns>Rotated Vector2.</returns>
    public static Vector2 RotateVector(Vector2 v, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;

        float sin = Mathf.Sin(radian);
        float cos = Mathf.Cos(radian);

        float x = v.x * cos - v.y * sin;
        float y = v.x * sin + v.y * cos;

        return new Vector2(x, y);
    }

    #endregion
}
