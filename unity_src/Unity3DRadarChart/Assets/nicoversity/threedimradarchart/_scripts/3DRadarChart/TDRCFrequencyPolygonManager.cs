/*
 * TDRCFrequencyPolygonManager.cs
 *
 * Supported Unity version: 2019.2.17f1 Personal (tested)
 *
 * Author: Nico Reski
 * Web: https://reski.nicoversity.com
 * Twitter: @nicoversity
 * GitHub: https://github.com/nicoversity
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for managing / organizing all individual TDRCFrequencyPolygon components representing the 3D Radar Chart.
/// </summary>
public class TDRCFrequencyPolygonManager : MonoBehaviour
{
    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;                  // reference to overall interface
    private Dictionary<string, List<TDRCFrequencyPoint>> fplDict;       // loaded data
    private Dictionary<string, string> colorDict;                       // loaded color data
    private List<GameObject> freqPolyGOList;                            // reference to all GameObjects with TDRCFrequencyPolygon components attached, representing the 3D Radar Chart


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


    #region INITIALIZATION

    /// <summary>
    /// Method to initiate the process of setting up all GameObjects representing the 3D Radar Chart.
    /// </summary>
    /// <param name="tdrcFplDictRef">Dictionary with loaded data.</param>
    /// <param name="tdrcColorDictRef">Dictionary with loaded color data for each dimension (= data variable).</param>
    public void initWithTDRCFrequencyPointListAndColorDictionary(Dictionary<string, List<TDRCFrequencyPoint>> tdrcFplDictRef, Dictionary<string, string> tdrcColorDictRef)
    {
        // keep track of data structures
        fplDict = tdrcFplDictRef;
        colorDict = tdrcColorDictRef;

        // set up and keep track of all individual data variables, represented as 2D freqency polygons
        freqPolyGOList = new List<GameObject>();
        foreach (string keyDimension in fplDict.Keys)
        {
            GameObject freqPolyGO = initTDRCFrequencyPolygon(keyDimension, fplDict[keyDimension], colorDict[keyDimension]);
            freqPolyGO.transform.SetParent(tdrcInterface.frequencyPolygonParent);
            freqPolyGOList.Add(freqPolyGO);
        }

        // automatically rotate all individual 2D frequency polygons based on the amount of data variables that represent the 3D Radar Chart
        for (int i = 0; i < freqPolyGOList.Count; i++)
        {
            float angle = 360.0f / freqPolyGOList.Count * i;
            freqPolyGOList[i].transform.Rotate(Vector3.right * angle);
        }


        // additional configurations based on interface properties
        //

        // rotate to map the time-series to the y-axis in the 3D space
        tdrcInterface.frequencyPolygonParent.Rotate(Vector3.forward * -270.0f);       // LAST point in time-series data at the top
        //tdrcInterface.frequencyPolygonParent.Rotate(Vector3.forward * -90.0f);      // FIRST point in time-series data at the top

        // apply overall scale modifier
        tdrcInterface.frequencyPolygonParent.localScale = Vector3.one * tdrcInterface.cnfg3d_scale;

        // set up guidance axis:
        // * 3D Radar Chart world length = overall 3D Radar Chart scale * ( x-pos of last frequency point in data series )
        // * guidance scale under consideration of overall 3D Radar Chart scale
        tdrcInterface.guidanceAxis.updateAxis(freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointForLastIndex().pos.x * tdrcInterface.cnfg3d_scale,
            (Vector3.one * tdrcInterface.cnfgGdncAxs_axisScale)  * tdrcInterface.cnfg3d_scale,
            (Vector3.one * tdrcInterface.cnfgGdncAxs_pointScale) * tdrcInterface.cnfg3d_scale);

        // set up interaction activation toggle:
        // * activate toggle scale = overall 3D radar chart scale * individual activation toggle scale
        float activationToggleScale = tdrcInterface.cnfg3d_scale * tdrcInterface.cnfgActTggl_scale;
        // * activation toggle y pos = overall 3D Radar Chart scale * ( x-pos of last frequency point in data series ) + configured offset to set activation toggle further "above" the frequency polygons under consideration of the activation toggle's scale + radius of activation sphere
        float activationToggleYPos = tdrcInterface.cnfg3d_scale * (freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointForLastIndex().pos.x) + tdrcInterface.cnfgActTggl_yOffset * activationToggleScale + activationToggleScale * 0.5f;
        // * apply calculated position and scale
        tdrcInterface.interact_activationToggle.updateYPositionAndScale(activationToggleYPos, Vector3.one * activationToggleScale);

        // set up interaction rotation handle:
        // * rotation handle y pos = overall 3D Radar Chart scale * ( x-pos of last frequency point in data series ) + configured offset to set rotation handle further "above" the frequency polygons under consideration of the activation toggle's scale + radius of rotation handle
        // * rotation handle resolution = amount of data variables (dimensions)
        float rotationHandleScale = tdrcInterface.cnfg3d_scale * tdrcInterface.cnfgRotHndl_scale;
        tdrcInterface.interact_rotationHandle.initWithProperties(tdrcInterface.cnfg3d_scale * (freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointForLastIndex().pos.x) + tdrcInterface.cnfgRotHndl_yOffset * rotationHandleScale + rotationHandleScale * 0.5f,
            freqPolyGOList.Count);

        // set up interaction time slice
        // * initiate min and max selectable time indices from the data
        // * set the time slice accordingly
        bool areTimeIndicesInitiated = tdrcInterface.initiateTimeIndicesFromData((freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointsCount()-1) / 2, 0, freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointsCount()-1, 0, freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointsCount()-1);
        if(areTimeIndicesInitiated) tdrcInterface.forceTimeSliceUpdateForIndex(tdrcInterface.stt_selectedTimeIndex);    // Note: forceTimeSliceUpdateForIndex() method used for initialization, usually use of tryTimeSliceUpdateForIndex() method once the 3D Radar Chart has been initialized.

        // set initial state of the 3D Radar Chart (to being deactivated)
        tdrcInterface.setActivationState(false);

        // finally: set flag to indicate that the 3d Radar Chart has been initialized
        tdrcInterface.stt_isInitialized = true;
    }

    /// <summary>
    /// Method to set up one data variable axis as a 2D frequency polygon.
    /// </summary>
    /// <param name="name">Name representing the data variable axis.</param>
    /// <param name="fplList">Data representing the data variable axis.</param>
    /// <param name="hexColor">Color (in RGB gex notiation) representing the data variable axis.</param>
    /// <returns>Reference to the created GameObject.</returns>
    private GameObject initTDRCFrequencyPolygon(string name, List<TDRCFrequencyPoint> fplList, string hexColor)
    {
        GameObject freqPolyGO = Instantiate(Resources.Load("3D_Radar_Chart-resources/Prefabs/TDRCFrequencyPolygon") as GameObject, this.transform);
        freqPolyGO.name = name;

        TDRCFrequencyPolygon freqPoly = freqPolyGO.GetComponent<TDRCFrequencyPolygon>();
        freqPoly.setTDRCInterface(tdrcInterface);
        freqPoly.origin       = Vector3.zero;
        freqPoly.pointColor   = TDRCFrequencyPolygon.colorWithHexAndAlpha(hexColor, tdrcInterface.cnfg2d_frequencyPointTransparency);
        freqPoly.polygonColor = TDRCFrequencyPolygon.colorWithHexAndAlpha(hexColor, tdrcInterface.cnfg2d_frequencyPolygonTransparency);
        freqPoly.lineWidth    = tdrcInterface.cnfg2d_frequencyPolygonLineWidth * tdrcInterface.cnfg3d_scale;
        freqPoly.initWithFrequencyPointList(fplList);

        return freqPolyGO;
    }

    #endregion


    #region DATA_HANDLER

    /// <summary>
    /// Retrieve references to all individual FrequencyPolygons composing this 3D Radar Chart.
    /// </summary>
    /// <returns>List to all FrequencyPolygon Transforms.</returns>
    public List<Transform> getFrequencyPolygons()
    {
        List<Transform> freqPolyTFList = new List<Transform>();
        foreach (GameObject go in freqPolyGOList)
        {
            freqPolyTFList.Add(go.transform);
        }
        return freqPolyTFList;
    }

    /// <summary>
    /// Retrieve references to the FrequencyPointComponents of each FrequencyPolygon at the currently selected index.
    /// </summary>
    /// <returns>List of FrequencyPointComponents.</returns>
    public List<TDRCFrequencyPointComponent> getFrequencyPointComponentListForCurrentlySelectedTimeIndex()
    {
        return getFrequencyPointComponentListForIndex(tdrcInterface.stt_selectedTimeIndex);
    }

    /// <summary>
    /// Retrieve references to the FrequencyPointComponents of each FrequencyPolygon at a gven index.
    /// </summary>
    /// <param name="index">Index for which the FrequencyPointComponents are asked for.</param>
    /// <returns>List of FrequencyPointComponents.</returns>
    public List<TDRCFrequencyPointComponent> getFrequencyPointComponentListForIndex(int index)
    {
        List<TDRCFrequencyPointComponent> fpcList = new List<TDRCFrequencyPointComponent>();
        foreach (GameObject fpg in freqPolyGOList)
        {
            fpcList.Add(fpg.GetComponent<TDRCFrequencyPolygon>().getFrequencyPointComponentForIndex(index));
        }
        return fpcList;
    }

    /// <summary>
    /// Retrieve references to the FrequencyPoint data structures of each FrequencyPolygon at the currently selected index.
    /// </summary>
    /// <returns>List of FrequencyPoints.</returns>
    public List<TDRCFrequencyPoint> getFrequencyPointListForCurrentlySelectedTimeIndex()
    {
        return getFrequencyPointListForIndex(tdrcInterface.stt_selectedTimeIndex);
    }

    /// <summary>
    /// Retrieve references to the FrequencyPoint data structures of each FrequencyPolygon at a given index.
    /// </summary>
    /// <param name="index">Index for which the FrequencyPoints are asked for.</param>
    /// <returns>List of FrequencyPoints.</returns>
    public List<TDRCFrequencyPoint> getFrequencyPointListForIndex(int index)
    {
        List<TDRCFrequencyPoint> fpList = new List<TDRCFrequencyPoint>();
        foreach (GameObject fpg in freqPolyGOList)
        {
            fpList.Add(fpg.GetComponent<TDRCFrequencyPolygon>().getFrequencyPointForIndex(index));
        }
        return fpList;
    }

    /// <summary>
    /// Function to receive the time label for the currently selected time index.
    /// </summary>
    /// <returns>String representing the date value for the currently selected time index.</returns>
    public string getCurrentlySelectedTimeLabel()
    {
        return getTimeLabelForIndex(tdrcInterface.stt_selectedTimeIndex);
    }

    /// <summary>
    /// Function to receive the time label for a given time index.
    /// </summary>
    /// <param name="index">Index for which the time is asked for.</param>
    /// <returns>String representing the time value for the queried time index.</returns>
    public string getTimeLabelForIndex(int index)
    {
        return freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointForIndex(index).time;
    }

    #endregion


    #region Y_POSITION_AND_TIME_INDEX_TRANSLATION

    /// <summary>
    /// Helper function providing the y-position for the currently selected time index.
    /// </summary>
    /// <returns>Float representing the y-position for the currently selected time index.</returns>
    public float getYPosForCurrentlySelectedTimeIndex()
    {
        // Note: Refer to x (instead of y) based on how FrequencyPolygon is setup.
        return freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>().getFrequencyPointComponentForCurrentSelectedTimeIndex().transform.localPosition.x * tdrcInterface.cnfg3d_scale;
    }

    /// <summary>
    /// Function to translate a given y-value (world coordinate) to the corresponding (closest) selectable index (Frequency Point) in the 3D Radar Chart.
    /// </summary>
    /// <param name="y">y-input value that requires translation to the Frequency Point index.</param>
    /// <returns>Index of the closest selectable Frequency Point index.</returns>
    public int getFrequencyPointIndexForY(float yWorld)
    {
        // Note: All Frequency Polygons of a 3D Radar Chart have the same length --> simply get the first one.
        TDRCFrequencyPolygon fp = freqPolyGOList[0].GetComponent<TDRCFrequencyPolygon>();

        // translate world y-value to local 3D Radar Chart y-value
        float yMinusRadarChartYPosition = yWorld - tdrcInterface.transform.localPosition.y;

        // translate y-value to index
        float calculatedIndexBasedOnY = yMinusRadarChartYPosition / tdrcInterface.cnfg3d_scale;
        int roundedIndex = Mathf.RoundToInt(calculatedIndexBasedOnY);

        // border handling
        if (roundedIndex < 0) roundedIndex = 0;
        else if (roundedIndex > fp.length) roundedIndex = Mathf.RoundToInt(fp.length);

        // return calculated index
        return roundedIndex;
    }

    /// <summary>
    /// Function to translate a time index (x in the Frequency Polygon) to its corresponding y-value in the 3D Radar Chart.
    /// </summary>
    /// <param name="index">Index that requires translation to the y-value.</param>
    /// <returns>Y-position value of the translated index.</returns>
    public float getYValueForIndex(int index)
    {
        return (float)index * tdrcInterface.cnfg2d_frequencyPointDistance * tdrcInterface.cnfg3d_scale;
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Function to display all FrequencyPoints at a specified index for all FrequencyPolygons.
    /// </summary>
    /// <param name="index">Index in the FrequencyPolygon specifing the to be displayed FrequencyPoint.</param>
    /// <param name="isActivated">Flag to determine whether the FrequencyPoint should be displayed (true) or hidden (false).</param>
    public void activateFrequencyPointsForIndex(int index, bool isActivated)
    {
        for (int i = 0; i < freqPolyGOList.Count; i++)
        {
            TDRCFrequencyPolygon fp = freqPolyGOList[i].GetComponent<TDRCFrequencyPolygon>();
            fp.displayFrequencyPointForIndex(index, isActivated);
        }
    }

    /// <summary>
    /// Function to display only a specified range (subset) of indices of the FrequencyPolygons.
    /// </summary>
    /// <param name="startIndex">Start index (including).</param>
    /// <param name="endIndex">End index (including).</param>
    public void displayRange(int startIndex, int endIndex)
    {
        if (startIndex != endIndex)
        {
            for (int i = 0; i < freqPolyGOList.Count; i++)
            {
                TDRCFrequencyPolygon fp = freqPolyGOList[i].GetComponent<TDRCFrequencyPolygon>();
                fp.displayRange(startIndex, endIndex);
            }
        }
    }

    #endregion
}
