/*
 * VRLMTimeSliceMovementHandMenu.cs
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

/// <summary>
/// Class responsible for handling display and interaction with the two-buttoned menu (attached to the user's left hand) in order to move step-wise the Time Slice (thus the selected time index) of the 3D Radar Chart.
/// </summary>
public class VRLMTimeSliceMovementHandMenu : MonoBehaviour
{
    // Attached to GameObject in Hierarchy:
    // VRLM_Player -> Leap Hand Controller -> Leap Attachment Controller - Hand UI

    // (dynamic) reference to a 3D Radar Chart's main interface (the user is currently interacting with)
    private ThreeDimRadarChartInterface tdrcInterface;

    // references to user iterface items (set via Unity Inspector)
    [Header("Hand Menu Interface Items")]
    public Text timeCurrentText;
    public Image buttonNextImage;
    public Image buttonPrevImage;

    // UI Sprite references (set via Unity Inspector; alternatively load from "Resources/3D_Radar_Chart-resources/Sprites")
    [Header("UI Sprite References")]
    public Sprite upSprite;
    public Sprite upEndSprite;
    public Sprite downSprite;
    public Sprite downEndSprite;


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
    /// Method to update the text of the time / description label.
    /// </summary>
    /// <param name="text">String representing the new label text.</param>
    public void updateTimeLabel(string text)
    {
        timeCurrentText.text = text;
    }

    /// <summary>
    /// Method to potentially update the hand menu's button sprites in order to provide visual feedback, e.g., if the no further time indices are selectable in a certain time direction.
    /// </summary>
    public void updateButtonSprites()
    {
        if (tdrcInterface.stt_selectedTimeIndex >= tdrcInterface.stt_maxSelectableTimeIndex)
            buttonNextImage.sprite = upEndSprite;
        else
            buttonNextImage.sprite = upSprite;

        if (tdrcInterface.stt_selectedTimeIndex <= tdrcInterface.stt_minSelectableTimeIndex)
            buttonPrevImage.sprite = downEndSprite;
        else
            buttonPrevImage.sprite = downSprite;
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Function called each time the "up" button is pressed.
    /// </summary>
    public void buttonStepUp()
    {
        if (tdrcInterface != null) tdrcInterface.timeSliceMoveUpOneIndex();
    }

    /// <summary>
    /// Function called each time the "down" button is pressed.
    /// </summary>
    public void buttonStepDown()
    {
        if (tdrcInterface != null) tdrcInterface.timeSliceMoveDownOneIndex();
    }

    #endregion
}
