using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing various visual guidance elements for its associated 3D Radar Chart's time-axis.
/// </summary>
public class TDRCGuidanceAxis : MonoBehaviour
{
    [Header("Transform Component References")]
    public Transform timeAxis;                              // reference to the transform representing the 3D Radar Chart's time-axis
    public Transform pointStart;                            // reference to the transform representing the 3D Radar Chart's time-axis' start point
    public Transform pointEnd;                              // reference to the transform representing the 3D Radar Chart's time-axis' end point
    public Material axisMaterial;                           // reference to the material used for the time-axis

    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;      // reference to overall interface


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


    #region STATE_MANIPULATION

    /// <summary>
    /// Method to set dynamically the properties of the visual guidance elements.
    /// </summary>
    /// <param name="chartLengthWorldspace">Float value representing the length of the 3D Radar Chart in the 3D world space.</param>
    /// <param name="axisScale">Float value indicating the scale of the time-axis.</param>
    /// <param name="pointScale">Float value indicating the scale of the time-axis' start and end points.</param>
    public void updateAxis(float chartLengthWorldspace, Vector3 axisScale, Vector3 pointScale)
    {
        // apply color to material
        axisMaterial.color = tdrcInterface.cnfgGdncAxs_color;

        // set initial scale properties
        timeAxis.localScale   = axisScale;
        pointStart.localScale = pointScale;
        pointEnd.localScale   = pointScale;

        // set axis length (scale) and position
        timeAxis.localScale    = new Vector3(timeAxis.localScale.x, chartLengthWorldspace * 0.5f + pointEnd.localScale.y * 0.5f, timeAxis.localScale.z);
        timeAxis.localPosition = new Vector3(timeAxis.localPosition.x, chartLengthWorldspace * 0.5f, timeAxis.localPosition.z);

        // set start (bottom) / end (top) position based on axis' length (ie. y-scale)
        pointStart.localPosition = new Vector3(0.0f, pointStart.localScale.y * -0.5f, 0.0f);
        pointEnd.localPosition   = new Vector3(0.0f, chartLengthWorldspace + (pointStart.localScale.y * 0.5f), 0.0f);
    }

    #endregion
}
