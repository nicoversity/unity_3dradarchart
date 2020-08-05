using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class responsible for handling display and interaction with the one-buttoned menu (attached to the user's right hand) in order to perform a Time Range selection within the 3D Radar Chart.
/// </summary>
public class VRLMTimeRangeSelectionHandMenu : MonoBehaviour
{
    // Attached to GameObject in Hierarchy:
    // VRLM_Player -> Leap Hand Controller -> Leap Attachment Controller - Hand UI

    // (dynamic) reference to a 3D Radar Chart's main interface (the user is currently interacting with)
    private ThreeDimRadarChartInterface tdrcInterface;

    // references to user iterface items (set via Unity Inspector)
    [Header("Hand Menu Interface Items")]
    public Image buttonStateChange;

    // UI Sprite references (set via Unity Inspector; alternatively load from "Resources/3D_Radar_Chart-resources/Sprites")
    [Header("UI Sprite References")]
    public Sprite stateReset;
    public Sprite stateRangeSelectStart;
    public Sprite stateRangeSelectEndWithEndAboveStart;
    public Sprite stateRangeSelectEndWithEndBelowStart;


    #region PROPERTY_SETTERS

    /// <summary>
    /// Method to keep track of a reference to the 3D Radar Chart's main interface.
    /// </summary>
    /// <param name="tdrcInterfaceRef">Reference to the ThreeDimRadarChartInterface instance.</param>
    public void setTDRCInterface(ThreeDimRadarChartInterface tdrcInterfaceRef)
    {
        tdrcInterface = tdrcInterfaceRef;
        if (tdrcInterface != null) updateButtonSprite();
    }

    #endregion


    #region DISPLAY

    /// <summary>
    /// Method to potentially update the hand menu's button sprites in order to provide visual feedback, e.g., about next iteration button press.
    /// </summary>
    public void updateButtonSprite()
    {
        switch(tdrcInterface.stt_timeRangeSelectionState)
        {
            case TDRCTimeRangeSelector.RangeSelectionState.NothingSelected:
            default:
                buttonStateChange.sprite = stateRangeSelectStart;
                break;

            case TDRCTimeRangeSelector.RangeSelectionState.FirstTimeRangeIndexSelected:
                if (tdrcInterface.stt_selectedTimeIndex >= tdrcInterface.timeRangeSelector.getInProgressSelectedStartIndex()) buttonStateChange.sprite = stateRangeSelectEndWithEndAboveStart;
                else buttonStateChange.sprite = stateRangeSelectEndWithEndBelowStart;
                break;

            case TDRCTimeRangeSelector.RangeSelectionState.SecondTimeRangeIndexSelected:
                buttonStateChange.sprite = stateReset;
                break;
        }
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Method called once the hand menu's button is pressed.
    /// </summary>
    public void buttonPressed()
    {
        tdrcInterface.timeRangeSelector.iterateTimeRangeSelectionState();
        updateButtonSprite();
    }

    #endregion
}
