using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class responsible for handling the display of additional user interface ellements related to the 3D Radar Chart's Time Slice, i.e., an Information Window (= 2D panel in the 3D space).
/// </summary>
public class TDRCTimeSliceRadarUI : MonoBehaviour
{
    [Header("Component References")]
    public CanvasRenderer timeSliceRadarUICanvasRenderer;                                                                           // reference to the CanvasRenderer (in the UI layer) that is going to visualize the current Time Slice as a 2D Radar Chart in the Information Window

    [Header("Guidance")]
    public Transform guidanceAxesRef;                                                                                               // reference to the transform holding all individual guidance axes (as children in the scene hierachy)
    private static string individualAxisPrefabRef = "3D_Radar_Chart-resources/Prefabs/TimeSliceRadarUIGuidanceIndividualAxis";      // reference to the prefab representing an individual guidance axis element (loaded from the application's "Assets/Resources" directory)

    [Header("Labels")]
    public Text header;                                                                                                             // text representing the header in the Information Window
    public Transform labelsRef;                                                                                                     // reference to the transform holding all individual data variable labels (as children in the scene hierarchy)
    private static string timeSliceRadarUILabelPrefabRef = "3D_Radar_Chart-resources/Prefabs/TimeSliceRadarUILabel";                // reference to the prefab representing individual data variable labels (loaded from the application's "Assets/Resources" directory)

    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;                                                                              // reference to overall interface
    private static readonly float rdrUI_scale = 0.04f;                                                                              // helper value for scaling of the UI; default = 0.04f at a 3D Radar Chart scale of 1.0f, and based on the overall Time Slice Radar UI components setup in the Unity Inspector (prefab)


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


    #region DISPLAY

    /// <summary>
    /// Method to update and redraw the UI, presenting information in the 2D panel.
    /// </summary>
    /// <param name="mesh">Mesh of the 2D Radar Chart representing the selected Time Slice in the 3D Radar Chart.</param>
    /// <param name="fpl">List of Frequency Points representing the data of the selected Time Slicein the 3D Radar Chart.</param>
    public void updateDisplay(Mesh mesh, List<TDRCFrequencyPoint> fpl)
    {
        setRadarChart2DMesh(mesh);
        updateDisplayHeader();
        updateDisplayForFrequencyPointList(fpl);
    }

    /// <summary>
    /// Method  to draw / display the 2D Radar Chart within the UI.
    /// </summary>
    /// <param name="mesh">Mesh of the 2D Radar Chart representing the selected Time Slice in the 3D Radar Chart.</param>
    private void setRadarChart2DMesh(Mesh mesh)
    {
        // set the mesh
        timeSliceRadarUICanvasRenderer.SetMesh(mesh);

        // set color
        Material polygonMeshMat = new Material(Shader.Find("UI/Default"));
        polygonMeshMat.color = tdrcInterface.cnfgTmSlcRdrUI_radarColor;
        timeSliceRadarUICanvasRenderer.SetMaterial(polygonMeshMat, null);
    }

    /// <summary>
    /// Method to update the header label in the UI.
    /// </summary>
    public void updateDisplayHeader()
    {
        // update header (compose text to display current time selection as well as current min/max selectable indices in the data set)
        string currentTime    = tdrcInterface.freqPolyManager.getFrequencyPointListForCurrentlySelectedTimeIndex()[0].time;
        string currentMinTime = tdrcInterface.freqPolyManager.getFrequencyPointListForIndex(tdrcInterface.stt_minSelectableTimeIndex)[0].time;
        string currentMaxTime = tdrcInterface.freqPolyManager.getFrequencyPointListForIndex(tdrcInterface.stt_maxSelectableTimeIndex)[0].time;
        header.text = currentTime + "   [" + currentMinTime + " -- " + currentMaxTime + "]";
    }

    /// <summary>
    /// Method to display data information (labels etc) within the UI.
    /// </summary>
    /// <param name="fpl">List of Frequency Points representing the data of the selected Time Slice in the 3D Radar Chart.</param>
    private void updateDisplayForFrequencyPointList(List<TDRCFrequencyPoint> fpl)
    {
        // clear UI
        foreach (Transform t in labelsRef)
        {
            GameObject.Destroy(t.gameObject);
        }

        // remove assistive guidance UI elements
        if (tdrcInterface.cnfgTmSlcRdrUI_isGuidanceDisplayed)
        {
            foreach (Transform t in guidanceAxesRef)
            {
                GameObject.Destroy(t.gameObject);
            }
        }

        // iterate through all data variables (= dimensions) of the 3D Radar Chart
        for (int i = 0; i < fpl.Count; i++)
        {
            // construct individual labels for each axis
            string itemText = fpl[i].dimension + ":\n" + fpl[i].value;

            // init label and display values
            Transform item = Instantiate(Resources.Load(timeSliceRadarUILabelPrefabRef) as GameObject).transform;
            item.gameObject.SetActive(true);
            item.gameObject.name = itemText;
            item.GetComponent<Text>().text = itemText;
            item.GetComponentInChildren<Image>().color = fpl[i].color;

            // assign parent GameObject
            item.SetParent(labelsRef, false);

            // reset position and scale (after attachment to parent GameObject)
            item.localPosition = Vector3.zero;
            item.localScale = Vector3.one;

            // set position
            float angle = (360.0f / fpl.Count * i * -1.0f) + 90.0f;
            Vector2 rotatedVector = TDRCTimeSliceRenderer.RotateVector(new Vector2(0.0f, 220.0f), angle);
            item.localPosition = rotatedVector;

            // display assistive guidance UI elements
            if (tdrcInterface.cnfgTmSlcRdrUI_isGuidanceDisplayed)
            {
                Transform axis = Instantiate(Resources.Load(individualAxisPrefabRef) as GameObject).transform;
                axis.name = "TimeSliceRadarUIGuidanceIndividualAxis_" + i;
                axis.gameObject.SetActive(true);
                axis.SetParent(guidanceAxesRef, false);
                axis.localEulerAngles = new Vector3(0.0f, 0.0f, angle);
            }
        }
    }

    #endregion


    #region TRANSFORM_MANIPULATION

    /// <summary>
    /// Method to update the position of the UI along the y-axis.
    /// </summary>
    /// <param name="y">Float value representing the new coordiante along the y-axis in the 3D space.</param>
    public void updateLocalPositionY(float y)
    {
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, y, this.transform.localPosition.z);
    }

    /// <summary>
    /// Method to update the scale (based on the interface's configuration) and the rotation of the UI, rotating it (1) around the 3D Radar Chart, and (2) to face the user.
    /// </summary>
    public void updateScaleAndRotation()
    {
        // update scale
        this.transform.localScale = Vector3.one * (rdrUI_scale * tdrcInterface.cnfg3d_scale);

        // update position of the 2D plane (rotate around the chart)
        this.transform.localPosition = new Vector3(0.0f, this.transform.localPosition.y, 0.0f);
        Quaternion userRotation      = Quaternion.Euler(0.0f, tdrcInterface.cam.transform.rotation.eulerAngles.y, 0.0f);
        Vector3 direction            = userRotation * Vector3.left * (tdrcInterface.cnfgTmSlcRdrUI_horizontalPositionOffset * tdrcInterface.cnfg3d_scale);
        this.transform.localPosition += direction;

        // update rotation of the 2D plane to face the user
        this.transform.rotation = userRotation;
    }

    #endregion
}
