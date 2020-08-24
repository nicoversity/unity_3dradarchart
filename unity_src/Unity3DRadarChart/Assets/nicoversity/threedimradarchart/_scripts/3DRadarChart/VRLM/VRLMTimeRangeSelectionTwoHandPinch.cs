/*
 * VRLMTimeRangeSelectionTwoHandPinch.cs
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
using UnityEngine.UI;
using Leap.Unity;

/// <summary>
/// Class to manage Time Range Selection with the currently linked 3D Radar Chart using a two-hand pinch gesture.
/// </summary>
public class VRLMTimeRangeSelectionTwoHandPinch : MonoBehaviour
{
    // Attached to GameObject in Hierarchy:
    // VRLM_Player -> Leap Hand Controller -> Time Range Selection Two Hand Pinch

    // Leap Motion Pinch Detector: Leap Motion default properties (in brackets: TDRC default values):
    // Activate Distance = 0.03 (0.02)
    // Deactivate Distance = 0.04 (0.04)

    // (dynamic) reference to a 3D Radar Chart's main interface (the user is currently interacting with)
    private ThreeDimRadarChartInterface tdrcInterface;

    // references to Leap Motion Pinch Detectors (using the Leap Motion Interaction Engine assets)
    [Header("Pinch Detectors")]
    public PinchDetector leftHandPinchDetector;
    public PinchDetector rightHandPinchDetector;

    // implementation of a timer threshold in order to avoid immediate pinch state after the previous has ended
    private float timer;

    [Header("Feedback Properties")]
    public GameObject rangeSelectionPreviewLabel;                   // reference to GameObject representing a simple time range selection preview label (default: VRLM_Time_Range_Selection_Pinch_Preview_Display)
    public Text rangeSelectionPreviewLabelText;                     // reference to the Text component of the GameObject representing a simple time range selection preview label (see above)
    public bool shouldTimeRangeSelectionLabelBeDisplayed;           // indicator whether or not the time range selection preview label should be used or not
    private bool isTimeRangeApplicationOnGoing;                     // indicator (at run-time) whether or a two-hand pinch gesture for the time range selection is currently ongoing
    private int currentTimeRangeSelectionIndexLeft;                 // helper value: identified time index for left hand pinch
    private int currentTimeRangeSelectionIndexRight;                // helper value: identifioed time index for right hand pinch


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


    #region UNITY_EVENT_FUNCTIONS

    /// <summary>
    /// General awake routine.
    /// </summary>
    private void Awake()
    {
        // initialize properties
        isTimeRangeApplicationOnGoing = false;
        timer = 0.0f;
    }

    /// <summary>
    /// General update routine.
    /// </summary>
    private void Update()
    {
        if (tdrcInterface != null)
        {
            // only handle if two-hand pinch has been enabled for Time Range Selection
            if (tdrcInterface.vrlmInterface.cnfgVRLM_twoHandPinchForTimeRangeSelectionIsEnabled)
            {
                // enable pinch interaction only of the user pinches with both hands simultaneously
                if (leftHandPinchDetector.IsPinching && rightHandPinchDetector.IsPinching)
                {
                    // reset visual interface of Time Range Selection via one-buttoned menu (if neccessary)
                    if (tdrcInterface.stt_timeRangeSelectionState == TDRCTimeRangeSelector.RangeSelectionState.FirstTimeRangeIndexSelected)
                        tdrcInterface.timeRangeSelector.abortTimeRangeSelection();

                    // retreive y position values for both left and right hand pinch
                    float yHeightLeft  = leftHandPinchDetector.Position.y;
                    float yHeightRight = rightHandPinchDetector.Position.y;

                    // retrieve selectable indices based on each pinch's y position
                    currentTimeRangeSelectionIndexLeft  = tdrcInterface.freqPolyManager.getFrequencyPointIndexForY(yHeightLeft);
                    currentTimeRangeSelectionIndexRight = tdrcInterface.freqPolyManager.getFrequencyPointIndexForY(yHeightRight);

                    // allow selection only if at least a range of 1 is selected
                    if (currentTimeRangeSelectionIndexLeft != currentTimeRangeSelectionIndexRight)
                    {
                        // pinch selection is automatically applied ("sculpting")
                        //

                        // only apply pinch selection if...
                        // a) timer threshold expired
                        // b) initial pinch is close in proximity
                        if (timer <= 0.0f)  // a)
                        {
                            // handle time range selection label display if necessary
                            if (shouldTimeRangeSelectionLabelBeDisplayed)
                            {
                                // activate if necessary
                                if (rangeSelectionPreviewLabel.activeInHierarchy == false) rangeSelectionPreviewLabel.SetActive(true);

                                // up date time range preview selection label position and rotation
                                rangeSelectionPreviewLabel.transform.localPosition = ((leftHandPinchDetector.Position - rightHandPinchDetector.Position) * 0.5f) + rightHandPinchDetector.Position;
                                rangeSelectionPreviewLabel.transform.eulerAngles = new Vector3(tdrcInterface.cam.transform.eulerAngles.x, tdrcInterface.cam.transform.eulerAngles.y, 0.0f);

                                // update time range preview selection label text
                                string leftHandDate = tdrcInterface.freqPolyManager.getTimeLabelForIndex(currentTimeRangeSelectionIndexLeft);
                                string rightHandDate = tdrcInterface.freqPolyManager.getTimeLabelForIndex(currentTimeRangeSelectionIndexRight);
                                rangeSelectionPreviewLabelText.text = leftHandDate + "  --  " + rightHandDate;
                                if (currentTimeRangeSelectionIndexLeft > currentTimeRangeSelectionIndexRight) rangeSelectionPreviewLabelText.text = rightHandDate + "  --  " + leftHandDate;                 
                            }

                            // ignore pinch distance if time range application is ongoing, else check for (initial distance)
                            if (isTimeRangeApplicationOnGoing)
                            {
                                // apply time range selection
                                bool wasUpdateSuccessful = displayTimeRangeForCurrentTwoHandPinchSelection(currentTimeRangeSelectionIndexLeft, currentTimeRangeSelectionIndexRight, true);
                                if (wasUpdateSuccessful) displayTimeSliceRelatedGameObjects(false);   // chartInteraction.displayTimeSliceSelector(false);
                            }
                            else
                            {
                                // allow interaction only if both pinches are close the 3D Radar Chart
                                bool isLeftPinchCloseToChart = isPinchCloseTo3DRadarChartForProximity(tdrcInterface.vrlmInterface.cnfgVRLM_twoHandPinchDetectorActivationProximityThreshold, leftHandPinchDetector.Position);
                                bool isRighPinchCloseToChart = isPinchCloseTo3DRadarChartForProximity(tdrcInterface.vrlmInterface.cnfgVRLM_twoHandPinchDetectorActivationProximityThreshold, rightHandPinchDetector.Position);
                                if (isLeftPinchCloseToChart && isRighPinchCloseToChart)     // b)
                                {
                                    // apply time range selection
                                    isTimeRangeApplicationOnGoing = true;
                                    bool wasUpdateSuccessful = displayTimeRangeForCurrentTwoHandPinchSelection(currentTimeRangeSelectionIndexLeft, currentTimeRangeSelectionIndexRight, true);
                                    if (wasUpdateSuccessful) displayTimeSliceRelatedGameObjects(false);
                                }
                            }
                        }
                    }
                }
                // check if pinch selection has ended
                else
                {
                    if (isTimeRangeApplicationOnGoing)
                    {
                        isTimeRangeApplicationOnGoing = false;

                        // update UI
                        if (shouldTimeRangeSelectionLabelBeDisplayed) rangeSelectionPreviewLabel.SetActive(false);
                        displayTimeSliceRelatedGameObjects(true);

                        // set timer
                        timer = tdrcInterface.vrlmInterface.cnfgVRLM_pinchDetectionTimeThreshold;
                    }
                }

                // handle timer
                if (isTimeRangeApplicationOnGoing == false)
                {
                    if (timer > 0.0f) timer = timer - Time.deltaTime;
                }
            }
        }
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Method to trigger the display update for the 3D Radar Chart's data based on the entered pinch-related parameters.
    /// </summary>
    /// <param name="indexLeft">Int value representing the identified time index of the left hand pinch.</param>
    /// <param name="indexRight">Int value representing the identified time index of the right hand pinch.</param>
    /// <param name="setTimeSliceToMiddle">Boolean value indicating whether (true) or not (false) to automatically set the selectected time index, and thus the Time Slice, to the middle of the newly selected time range.</param>
    /// <returns></returns>
    private bool displayTimeRangeForCurrentTwoHandPinchSelection(int indexLeft, int indexRight, bool setTimeSliceToMiddle)
    {
        // apply time range selection
        tdrcInterface.timeRangeSelector.applyTimeRangeSelection(indexLeft, indexRight);

        // set time slice to middle
        bool wasUpdateSuccessful = false;
        if (setTimeSliceToMiddle) wasUpdateSuccessful = tdrcInterface.tryTimeSliceUpdateForIndex(tdrcInterface.stt_minSelectableTimeIndex + ((tdrcInterface.stt_maxSelectableTimeIndex - tdrcInterface.stt_minSelectableTimeIndex) / 2));
        return wasUpdateSuccessful;
    }

    /// <summary>
    /// Function to determine whether the pinch position is close to the currently interacting 3D Radar Chart, ignoring the y-dimension.
    /// </summary>
    /// <param name="proximity">Float value determining the proximity threshold for the evaluation whether the pinch position is close or not.</param>
    /// <param name="pinchPos">Vector3 representing the position of the detected pinch.</param>
    /// <returns>True if the distance between pinch and the 3D Radar Chart is less than the entered proximity, and false otherwise.</returns>
    private bool isPinchCloseTo3DRadarChartForProximity(float proximity, Vector3 pinchPos)
    {
        // determine distance between pinch and 3D Radar Chart (ignore y-axis)
        float distance = Vector2.Distance(new Vector2(tdrcInterface.transform.localPosition.x, tdrcInterface.transform.localPosition.z),
                                  new Vector2(pinchPos.x, pinchPos.z));

        if (distance < proximity) return true;
        else return false;
    }

    /// <summary>
    /// Helper method to handle the display/hide of the Time Slice related GameObjects.
    /// </summary>
    /// <param name="isDisplayed"></param>
    public void displayTimeSliceRelatedGameObjects(bool isDisplayed)
    {
        tdrcInterface.interact_timeSliceGO.SetActive(isDisplayed);                                                              // handle display of Time Slice
        tdrcInterface.freqPolyManager.activateFrequencyPointsForIndex(tdrcInterface.stt_selectedTimeIndex, isDisplayed);        // handle display of the Frequency Points at the currently selected time index

        if (isDisplayed) tdrcInterface.timeSliceRadarUI.updateScaleAndRotation();                                               // handle automatic rotation of Time Slice Radar UI (only upon activation)
        tdrcInterface.timeSliceRadarUI.gameObject.SetActive(isDisplayed);                                                       // handle display of Time Slice Radar UI
    }

    #endregion
}
