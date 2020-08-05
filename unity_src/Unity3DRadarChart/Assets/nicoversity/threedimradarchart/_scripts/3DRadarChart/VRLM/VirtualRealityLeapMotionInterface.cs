using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

/// <summary>
/// Class responsible for the configuration and interaction with a 3D Radar Chart visualization GameObject using virtual reality and the Leap Motion hand controller (= main interface between VR and Leap Motion features and the 3D Radar Chart).
/// </summary>
public class VirtualRealityLeapMotionInterface : MonoBehaviour
{
    // Attached to GameObject in Hierarchy:
    // VRLM_Player

    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;                                          // reference to overall interface

    [Header("Scene Hierarchy References")]
    private InteractionBehaviour ibTimeSliceGO;                                                 // reference to the Leap Motion Interaction Behaviour component attached to this 3D Radar Chart's Time Slice (only set if 3D Radar Chart is configured for VR an Leap Motion interaction)
    public VRLMTimeSliceMovementHandMenu timeSliceMovementHandMenu;                             // reference to the Leap Motion Time Movement (left) hand menu this 3D Radar Chart is currently linked to
    public VRLMTimeRangeSelectionHandMenu timeRangeSelectionHandMenu;                           // reference to the Leap Motion Time Range Selection (right) hand menu this 3D Radar Chart is currently linked to
    public VRLMTimeRangeSelectionTwoHandPinch timeRangeSelectionTwoHandPinch;                   // reference to the Leap Motion Time Range Selection two-hand pinch interaction manager this 3D Radar Chart is currently linked to

    [Header("VR and Leap Motion Configuration")]
    public string cnfgVRLM_LeapMotionAttachmentGameObjectName;                                  // name of the GameObject in the Scene Hierarchy that has all Leap Motion hand controller attachments (i.e., the hand menus), default: "Leap Attachment Controller - Hand UI"
    public string cnfgVRLM_LeapMotionTimeRangeSelectionTwoHandPinchGameObjectName;              // name of the GameObject in the Scene Hierarchy that has the Time Range Two-Hand Pinch Selection component attached, default: "Time Range Selection Two Hand Pinch"
    public bool cnfgVRLM_twoHandPinchForTimeRangeSelectionIsEnabled;                            // indicator whether or not the two-hand pinch gesture for Time Range Selection is enabled
    public float cnfgVRLM_twoHandPinchDetectorActivationProximityThreshold;                     // threshold representing the distance between detected pinch and this 3D Radar Chart (in order to determine if the pinch was detected at a certain proximity; default: 1.0f)
    public float cnfgVRLM_pinchDetectionTimeThreshold;                                          // threshold (in seconds) to avoid immediate pinch state change after the previous one has finished (default: 2.0f seconds)


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
    /// Method to set up all necessary GameObjects references for VR and Leap Motion Interaction.
    /// </summary>
    /// <param name="tdrcInterfaceRef">Reference to the ThreeDimRadarChartInterface instance.</param>
    /// <returns>True once everything has been set up.</returns>
    public bool setupForVRAndLeapMotionInteraction(ThreeDimRadarChartInterface tdrcInterfaceRef)
    {
        tdrcInterface = tdrcInterfaceRef;

        // Generally, the setup of GameObjects for Leap Motion interaction follows the same routine of:
        // (1) Attach an InteractionBehaviour component to the corresponding GameObject.
        // (2) Configure the attached InteractionBehaviour component if necessary.
        // (3) Setup EventListerners in the attached InteractionBehaviour component.

        if (tdrcInterface.interact_activationToggle != null)
        {
            InteractionBehaviour ibActivationToggle = tdrcInterface.interact_activationToggle.gameObject.AddComponent<InteractionBehaviour>();
            ibActivationToggle.ignoreGrasping = true;
            ibActivationToggle.OnPerControllerContactBegin = vrlmTriggerActivationToggleOnPerControllerContactBegin;
        }

        if (tdrcInterface.interact_rotationHandle != null)
        {
            InteractionBehaviour ibRotationHandle = tdrcInterface.interact_rotationHandle.gameObject.AddComponent<InteractionBehaviour>();
            ibRotationHandle.moveObjectWhenGrasped = true;      // Note: Movement properties can be adjusted by editing the RigidBody component of the TDRCInteractionInteractionRotationHandle GameObject
        }

        if (tdrcInterface.interact_timeSliceGO != null)
        {
            ibTimeSliceGO = tdrcInterface.interact_timeSliceGO.AddComponent<InteractionBehaviour>();
            ibTimeSliceGO.moveObjectWhenGrasped = false;
            ibTimeSliceGO.OnGraspStay = vrlmGraspStayTimeSlice;
        }

        return true;
    }

    #endregion


    #region INTERACTION

    private float helper_timeSlice_lastYUpdate = 0.0f;      // helper value to keep track of the last y-position update of the Time Slice when moved through Leap Motion Grasp Interaction

    /// <summary>
    /// Method to identify the Activation Toggle of a 3D Radar Chart (from potentially multiple in the Scene) the Leap Motion controller collided with, and set link interface references accordingly.
    /// </summary>
    /// <param name="controller">InteractionController representing the Leap Motion hand controller a contact event occured with.</param>
    private void vrlmTriggerActivationToggleOnPerControllerContactBegin(InteractionController controller)
    {
        // Developer Note: This is basically required to support interaction with multiple 3D Radar Charts in the Scene, assuming interaction with each charts' Activation Toggle switches the focus.

        // Generally, the routine goes as follows
        // (1) Identify collided (contacted) GameObject (potentially one or multiple).
        // (2) Identify Activation Toggle component among collided GameObjects, and retrieve reference to its 3D Radar Chart's main interface.
        // (3) Link references between VRLM and TDRC interfaces accordingly.
        // (4) Continue with interaction.

        // (1) 
        HashSet<IInteractionBehaviour>.Enumerator em = controller.contactingObjects.GetEnumerator();
        IInteractionBehaviour ib = null;
        while(em.MoveNext())
        {
            ib = em.Current;

            // (2) 
            TDRCInteractionActivationToggle iatComponent = ib.transform.GetComponent<TDRCInteractionActivationToggle>();
            if(iatComponent != null) break;
        }

        // (3)
        ThreeDimRadarChartInterface tdrcInterfaceRef = ib.transform.parent.parent.GetComponent<ThreeDimRadarChartInterface>();
        tdrcInterface = tdrcInterfaceRef;

        // (4)
        vrlmTriggerActivationToggle();
    }

    /// <summary>
    /// Wrapper method to trigger the Activation Toggle using Leap Motion interaction.
    /// </summary>
    private void vrlmTriggerActivationToggle()
    {
        bool hasBeenActivated = tdrcInterface.triggerActivationToggle();

        // if this 3D Radar Chart has been activated: link this 3D Radar Chart to the user's Leap Motion hand menus
        if (hasBeenActivated)
        {
            // update reference to Time Slice GameObject
            ibTimeSliceGO = tdrcInterface.interact_timeSliceGO.GetComponent<InteractionBehaviour>();

            // find GameObject instance holding the user's Leap Motion Hand Menu components
            GameObject vrlmAttachmentControllerGO = GameObject.Find(cnfgVRLM_LeapMotionAttachmentGameObjectName);

            // set Hand Menu references and update them
            if (vrlmAttachmentControllerGO != null)
            {
                timeSliceMovementHandMenu = vrlmAttachmentControllerGO.GetComponent<VRLMTimeSliceMovementHandMenu>();
                if (timeSliceMovementHandMenu != null)
                {
                    timeSliceMovementHandMenu.setTDRCInterface(tdrcInterface);
                    timeSliceMovementHandMenu.updateTimeLabel(tdrcInterface.freqPolyManager.getFrequencyPointListForCurrentlySelectedTimeIndex()[0].time);
                    timeSliceMovementHandMenu.updateButtonSprites();
                }

                timeRangeSelectionHandMenu = vrlmAttachmentControllerGO.GetComponent<VRLMTimeRangeSelectionHandMenu>();
                if (timeRangeSelectionHandMenu != null)
                {
                    timeRangeSelectionHandMenu.setTDRCInterface(tdrcInterface);
                }
            }

            // find GameObject instance holding the user's Leap Motion Time Range Selection Two-Hand Pinch component
            GameObject vrlmTimeRangeSelectionTwoHandPinchGO = GameObject.Find(cnfgVRLM_LeapMotionTimeRangeSelectionTwoHandPinchGameObjectName);

            // set reference and update
            if (vrlmTimeRangeSelectionTwoHandPinchGO != null)
            {
                timeRangeSelectionTwoHandPinch = vrlmTimeRangeSelectionTwoHandPinchGO.GetComponent<VRLMTimeRangeSelectionTwoHandPinch>();
                if (timeRangeSelectionTwoHandPinch != null)
                {
                    timeRangeSelectionTwoHandPinch.setTDRCInterface(tdrcInterface);
                }
            }
        }
    }

    /// <summary>
    /// Method to move the Time Slice based on the onGraspStay event of Leap Motion Interaction. 
    /// </summary>
    private void vrlmGraspStayTimeSlice()
    {
        // get grasp point
        Vector3 currentGraspPoint = ibTimeSliceGO.GetGraspPoint(ibTimeSliceGO.graspingController);

        // helper valid to determine validity of "new" selected time index
        int updatedTimeIndex = 0;

        // calculate distance in order to handle "snapping" of the Time Slice to the individual indices of the 3D Radar Chart's FrequencyPolygons
        float distance = currentGraspPoint.y - helper_timeSlice_lastYUpdate;
        if (Mathf.Abs(distance) > tdrcInterface.cnfg3d_scale)     // distance of "snapping" based on the overall 3D Radar Chart's scale
        {
            // update latest y-position
            helper_timeSlice_lastYUpdate = currentGraspPoint.y;

            // determine direction of position update
            if (distance > 0.0f)
            {
                updatedTimeIndex = tdrcInterface.stt_selectedTimeIndex + 1;
            }
            else if (distance < 0.0f)
            {
                updatedTimeIndex = tdrcInterface.stt_selectedTimeIndex - 1;
            }

            // try to conduct time index update
            tdrcInterface.tryTimeSliceUpdateForIndex(updatedTimeIndex);
        }
    }

    /// <summary>
    /// Helper method to update all hand menu related properties.
    /// </summary>
    public void updateHandMenuIntefaces()
    {
        // update Time Slice Movement Hand Menu
        if (timeSliceMovementHandMenu != null)
        {
            timeSliceMovementHandMenu.updateTimeLabel(tdrcInterface.freqPolyManager.getFrequencyPointListForCurrentlySelectedTimeIndex()[0].time);
            timeSliceMovementHandMenu.updateButtonSprites();
        }

        // update Time Range Selection Hand Menu
        if (timeRangeSelectionHandMenu != null)
        {
            timeRangeSelectionHandMenu.updateButtonSprite();
        }
    }

    #endregion

}
