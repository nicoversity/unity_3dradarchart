using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class repsonsible for manipulating the 3D Radar Chart's time range selection (e.g., to display only a subset of the whole dataset).
/// </summary>
public class TDRCTimeRangeSelector : MonoBehaviour
{
    /// <summary>
    /// Enum representing the different states of interacting with the time range selection functionalities.
    /// </summary>
    public enum RangeSelectionState :int
    {
        NothingSelected = 0,                    // nothing is selected (= the whole dataset is displayed)
        FirstTimeRangeIndexSelected = 1,        // a first time index for the time range selection has been selected (= time range selection is ongoing)
        SecondTimeRangeIndexSelected = 2        // a second time index for the time range selection has been selected (= time range selection has been completed and applied, subset is displayed)
    }


    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;                                      // reference to overall interface

    private int rangeSelectionInProgress_selectedStartIndex;                                // helper value to determine selected start index during the time range selection process (= time range selection not yet applied)
    private int rangeSelectionInProgress_selectedEndIndex;                                  // helper value to determine selected end index during the time range selection process (= time range selection not yet applied)

    private GameObject visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart;     // reference to temporary created time slice radar renderer during time range selection process (as visual feedback during the time range selection process)
    private GameObject visualFeedback_rangeSelectionAxis;                                   // reference to temporary created axis during time range selection process (as visual feedback during the time range selection process)


    #region PROPERTY_SETTERS

    /// <summary>
    /// Method to keep track of a reference to the 3D Radar Chart's main interface.
    /// </summary>
    /// <param name="tdrcInterfaceRef">Reference to the ThreeDimRadarChartInterface instance.</param>
    public void setTDRCInterface(ThreeDimRadarChartInterface tdrcInterfaceRef)
    {
        tdrcInterface = tdrcInterfaceRef;
        tdrcInterface.stt_timeRangeSelectionState = RangeSelectionState.NothingSelected;
    }

    #endregion


    #region STATE_MANIPULATION

    /// <summary>
    /// Method to iterate the current time range selection state, i.e., change to the logically next one (based on order as defined in the RangeSelectionState enum).
    /// </summary>
    /// <returns>RangeSelectionState representing the new time range selection state.</returns>
    public RangeSelectionState iterateTimeRangeSelectionState()
    {
        // allow iteration only if the 3D Radar Chart is currently activated for interaction
        if (tdrcInterface.stt_isActivated)
        {
            // iterate current time range selection state to the logical next state
            switch (tdrcInterface.stt_timeRangeSelectionState)
            {
                case RangeSelectionState.NothingSelected:
                default:
                    handleFirstTimeRangeIndexSelection();
                    tdrcInterface.stt_timeRangeSelectionState = RangeSelectionState.FirstTimeRangeIndexSelected;
                    break;

                case RangeSelectionState.FirstTimeRangeIndexSelected:
                    bool isSecondTimeRangeIndexSelectionValid = handleSecondTimeRangeIndexSelection();
                    if (isSecondTimeRangeIndexSelectionValid) tdrcInterface.stt_timeRangeSelectionState = RangeSelectionState.SecondTimeRangeIndexSelected;
                    break;

                case RangeSelectionState.SecondTimeRangeIndexSelected:
                    tdrcInterface.stt_timeRangeSelectionState = RangeSelectionState.NothingSelected;
                    resetTimeRangeSelection();
                    break;
            }
        }
        
        return tdrcInterface.stt_timeRangeSelectionState;
    }

    /// <summary>
    /// Method handling the logic that needs to be performed once the first time range selection index has been selected.
    /// </summary>
    private void handleFirstTimeRangeIndexSelection()
    {
        // retrieve index of the currently selected time slice position
        rangeSelectionInProgress_selectedStartIndex = tdrcInterface.stt_selectedTimeIndex;

        // initiate visual feedback during the time range selection process
        initiateVisualFeedback();
    }

    /// <summary>
    /// Method handling the logic that needs to be performed once the second time range selection index has been selected.
    /// </summary>
    /// <returns>Bool value representing whether (true) or not (false) the selection of the second time range selection index was valid.</returns>
    private bool handleSecondTimeRangeIndexSelection()
    {
        // only allow time range selection if selected index is not equal to the first index, i.e., a time range must have at least an interval of 1
        bool isSecondTimeRangeIndexSelectionValid = true;
        if(tdrcInterface.stt_selectedTimeIndex != rangeSelectionInProgress_selectedStartIndex)
        {
            // retrieve index of the currently selected time slice position
            rangeSelectionInProgress_selectedEndIndex = tdrcInterface.stt_selectedTimeIndex;

            // automatically apply range selection once the second time range index has been selected
            applyTimeRangeSelection(rangeSelectionInProgress_selectedStartIndex, rangeSelectionInProgress_selectedEndIndex);
        }
        else
        {
            isSecondTimeRangeIndexSelectionValid = false;
        }

        return isSecondTimeRangeIndexSelectionValid;
    }

    /// <summary>
    /// Method to reset the time range selection.
    /// </summary>
    private void resetTimeRangeSelection()
    {
        // reset internal helper values
        rangeSelectionInProgress_selectedStartIndex = -1;
        rangeSelectionInProgress_selectedEndIndex   = -1;

        // update state in main interface
        bool areTimeIndicesReset = tdrcInterface.resetSelectableTimeIndices();
        if (areTimeIndicesReset)
        {
            tdrcInterface.freqPolyManager.displayRange(tdrcInterface.stt_minSelectableTimeIndex, tdrcInterface.stt_maxSelectableTimeIndex);
            tdrcInterface.timeSliceRadarUI.updateDisplayHeader();
        }
    }

    /// <summary>
    /// Method to apply the time range selection based on start and end indices.
    /// </summary>
    /// <param name="startIndex">Int value representing the selected start index in the data for the time range selection.</param>
    /// <param name="endIndex">Int value representing the selected end index in the data for the time range selection.</param>
    /// <returns></returns>
    public bool applyTimeRangeSelection(int startIndex, int endIndex)
    {
        // manage index order if necessary
        if (startIndex > endIndex)
        {
            int temp = endIndex;
            endIndex = startIndex;
            startIndex = temp;
        }

        // update state in main interface
        tdrcInterface.stt_minSelectableTimeIndex = startIndex;
        tdrcInterface.stt_maxSelectableTimeIndex = endIndex;

        // update visualization of the 3D Radar Chart
        tdrcInterface.freqPolyManager.displayRange(tdrcInterface.stt_minSelectableTimeIndex, tdrcInterface.stt_maxSelectableTimeIndex);
        tdrcInterface.timeSliceRadarUI.updateDisplayHeader();

        // remove visual feedback
        destroyVisualFeedback();

        return true;
    }

    /// <summary>
    /// Method to abort an ongoing time range selection.
    /// </summary>
    public void abortTimeRangeSelection()
    {
        tdrcInterface.stt_timeRangeSelectionState = RangeSelectionState.NothingSelected;
        resetTimeRangeSelection();
        destroyVisualFeedback();
    }

    #endregion


    #region VISUAL_FEEDBACK

    /// <summary>
    /// Method to initialize all graphical user interface components to represent an ongoing time range selection (i.e., (1) a copy of the 2D Radar Chart mesh inside the 3D Radar Chart indicating the first selected index, and (2) an axis element indicating the range between the first select index and the currently select time index).
    /// </summary>
    public void initiateVisualFeedback()
    {
        // make a copy of the current Time Slice selector GameObject
        visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart = GameObject.Instantiate(tdrcInterface.interact_timeSliceGO, tdrcInterface.interact_timeSliceGO.transform.parent);
        if (visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart != null)
        {
            // set appropriate name
            visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart.name = "temp-visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart";

            // destroy its box collider (no interaction with hands needed)
            Destroy(visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart.GetComponent<BoxCollider>());

            // create a new mesh for the canvas renderer
            TDRCTimeSliceRenderer tsr = visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart.GetComponentInChildren<TDRCTimeSliceRenderer>();
            if (tsr != null)
            {
                tsr.setTDRCInterface(tdrcInterface);
                tsr.setTDRCFreqPolyManager(tdrcInterface.freqPolyManager);
                tsr.displayRadarWithColor(tdrcInterface.cnfgTmRngSlctr_visualFeedbackColor);
            }
            else Debug.LogError("[TDRCTimeRangeSelector] Unable to acquire TDRCTimeSliceRenderer for temporary visual feedback.");
        }

        // make a copy of the guidance axis GameObject
        visualFeedback_rangeSelectionAxis = GameObject.Instantiate(tdrcInterface.GetComponentInChildren<TDRCGuidanceAxis>().timeAxis.gameObject, tdrcInterface.interact_timeSliceGO.transform.parent);
        if (visualFeedback_rangeSelectionAxis != null)
        {
            // set appropriate name
            visualFeedback_rangeSelectionAxis.name = "temp-visualFeedback_rangeSelectionAxis";

            // set render material and color
            Material vfrsaMat = visualFeedback_rangeSelectionAxis.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
            vfrsaMat.color = tdrcInterface.cnfgTmRngSlctr_visualFeedbackColor;

            // set scale properties
            visualFeedback_rangeSelectionAxis.transform.localScale = new Vector3(tdrcInterface.cnfgTmRngSlctr_axisScale, visualFeedback_rangeSelectionAxis.transform.localScale.y * 0.5f, tdrcInterface.cnfgTmRngSlctr_axisScale);

            // update position
            updateVisualFeedback(visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart.transform.localPosition.y);
        }
    }

    /// <summary>
    /// Method to update all graphical user interface components to represent an ongoing time range selection (i.e., (1) a copy of the 2D Radar Chart mesh inside the 3D Radar Chart indicating the first selected index, and (2) an axis element indicating the range between the first select index and the currently select time index).
    /// </summary>
    /// <param name="yPosCurrentTimeSliceSelection"></param>
    public void updateVisualFeedback(float yPosCurrentTimeSliceSelection)
    {
        // get y position of timeSliceSelector copy
        float yPosRangeStart = visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart.transform.localPosition.y;

        // determine top / bottom in terms of y position for better readability
        float top;
        float bottom;

        if (yPosRangeStart > yPosCurrentTimeSliceSelection)
        {
            top = yPosRangeStart;
            bottom = yPosCurrentTimeSliceSelection;
        }
        else
        {
            top = yPosCurrentTimeSliceSelection;
            bottom = yPosRangeStart;
        }

        // update axis
        if (Mathf.Approximately(top, bottom) == false)
        {
            // activate axis GameObject if necessary
            if (visualFeedback_rangeSelectionAxis.activeInHierarchy == false) visualFeedback_rangeSelectionAxis.SetActive(true);

            // calculate distance and set position and scale
            float halfDistance = (top - bottom) * 0.5f;
            float yPosAxis = bottom + halfDistance;
            float yScaleAxis = halfDistance;

            // set axis properties
            visualFeedback_rangeSelectionAxis.transform.localScale = new Vector3(visualFeedback_rangeSelectionAxis.transform.localScale.x, yScaleAxis, visualFeedback_rangeSelectionAxis.transform.localScale.z);
            visualFeedback_rangeSelectionAxis.transform.localPosition = new Vector3(visualFeedback_rangeSelectionAxis.transform.localPosition.x, yPosAxis, visualFeedback_rangeSelectionAxis.transform.localPosition.z);
        }
        else
        {
            // deactivate axis GameObject
            visualFeedback_rangeSelectionAxis.SetActive(false);
        }
    }

    /// <summary>
    /// Helper method to return the selected start index during an ongoing time range selection.
    /// </summary>
    /// <returns>Int value representing the selected start time index.</returns>
    public int getInProgressSelectedStartIndex()
    {
        return rangeSelectionInProgress_selectedStartIndex;
    }

    /// <summary>
    /// Method to remove (destroy) all graphical user interface components to represent an ongoing time range selection (i.e., (1) a copy of the 2D Radar Chart mesh inside the 3D Radar Chart indicating the first selected index, and (2) an axis element indicating the range between the first select index and the currently select time index).
    /// </summary>
    private void destroyVisualFeedback()
    {
        // destroy temporary copy of time slice selector for indicating the start range
        Destroy(visualFeedback_timeSliceRadarRendererCopyForRangeSelectionStart);

        // destroy temporary copy of visual feedback axis
        Destroy(visualFeedback_rangeSelectionAxis);
    }

    #endregion
}
