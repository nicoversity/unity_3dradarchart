using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for handling the state and the interaction with the 3D Radar Chart's Activation Toggle, i.e., a (graphical) user interface element for the purpose of activating and deactivating additional user interface elements and mechanisms for its associated 3D Radar Chart.
/// </summary>
public class TDRCInteractionActivationToggle : MonoBehaviour
{
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
    /// Method to set the the Activation Toggle's y-position and scale.
    /// </summary>
    /// <param name="yPos">Float representing the new y-position.</param>
    /// <param name="scale">Vector3 representing the new scale.</param>
    /// <returns>True if the y-position and scale have been updated.</returns>
    public bool updateYPositionAndScale(float yPos, Vector3 scale)
    {
        this.transform.localPosition = new Vector3(this.transform.localPosition.x, yPos, this.transform.localPosition.z);
        this.transform.localScale = scale;
        return true;
    }

    /// <summary>
    /// Method to adjust the state of the Activation Toggle based on the entered state.
    /// </summary>
    /// <param name="state">Boolean value representing the state of the Activation Toggle (true = activated, false = deactivated).</param>
    public void updateState(bool state)
    {
        // activated
        if (state == true)
        {
            this.GetComponent<Renderer>().material.color = tdrcInterface.cnfgActTggl_colorActivated;
        }
        // deactivated
        else
        {
            this.GetComponent<Renderer>().material.color = tdrcInterface.cnfgActTggl_colorDeactivated;
        }
    }

    #endregion
}
