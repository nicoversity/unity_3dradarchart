using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for the configuration and initialization of a 3D Radar Chart visualization GameObject (= main interface to the 3D Radar Chart).
/// </summary>
public class ThreeDimRadarChartInterface : MonoBehaviour
{
    [Header("Data Source")]
    public string dataSourceURL;                                                                // data url for loading data from a server
    public string dataSourceLocalFilePath;                                                      // data filepath for loading data from the application's local "Assets/Resources" directory
    public bool isDataLoadedFromServer;                                                         // flag to indicate whether or not the data is loaded from the server (true) or from the local directory (false)

    [Header("Data Source Color Coding")]
    public string dataColorsURL;                                                                // data url for loading the data variable (= dimension) color coding from a server
    public string dataColorLocalFilePath;                                                       // data filepath for loading the data variable (= dimension) color coding from the application's local "Assets/Resources" directory
    public bool isDataColorCodingLoadedFromServer;                                              // flag to indicate whether or not the data variable (= dimension) color coding is loaded from the server (true) or from the local directory (false)

    [Header("Scene Hierarchy References")]
    public Camera cam;                                                                          // reference to the scene's (main) camera
    public TDRCDataLoader dataLoader;                                                           // reference to this 3D Radar Chart's Data Loader component
    public Transform frequencyPolygonParent;                                                    // referance to a GameObject's Transform that is going to have all created Frequency Polygons (= individual data variable axes) attached
    public TDRCFrequencyPolygonManager freqPolyManager;                                         // reference to this 3D Radar Chart's Frequency Polygon Manager component
    public TDRCGuidanceAxis guidanceAxis;                                                       // reference to this 3D Radar Chart's Guidance Axiscomponent
    public TDRCInteractionActivationToggle interact_activationToggle;                           // reference to this 3D Radar Chart's Interaction Activation Toggle component
    public TDRCInteractionRotationHandle interact_rotationHandle;                               // reference to this 3D Radar Chart's Interaction Rotation Handle component
    public GameObject interact_timeSliceGO;                                                     // reference to this 3D Radar Chart's interactable GameObject representing the 3D Radar Chart's Time Slice
    public TDRCTimeSliceRenderer timeSliceRenderer;                                             // reference to this 3D Radar Chart's Time Slice Renderer component
    public TDRCTimeSliceRadarUI timeSliceRadarUI;                                               // reference to this 3D Radar Chart's Time Slice Radar UI component
    public TDRCTimeRangeSelector timeRangeSelector;                                             // reference to this 3D Radar Chart's Time Range Selector component

    [Header("State")]
    public bool stt_isInitialized;                                                              // indication whether or not the 3D Radar Chart has been initialized
    public Vector3 stt_position;                                                                // configured position of the 3D Radar Chart in the 3D space
    public bool stt_isActivated;                                                                // indication whether or not the 3D Radar Chart is currently activated (for interaction)
    public bool stt_isRotating;                                                                 // indication whether or not the 3D Radar Chart is currently rotating
    public int stt_selectedTimeIndex;                                                           // currently selected time index
    public int stt_previousSelectedTimeIndex;                                                   // helper value to keep track of the previously selected time index
    public int stt_minSelectableTimeIndex;                                                      // currently minimum selectable time index
    public int stt_maxSelectableTimeIndex;                                                      // currently maximum selectable time index
    private int stt_datasetMinTimeIndex;                                                        // absolute minimum time index in the dataset
    private int stt_datasetMaxTimeIndex;                                                        // absolute maximum time index in the dataset
    public TDRCTimeRangeSelector.RangeSelectionState stt_timeRangeSelectionState;               // current state of the time range selection interaction

    [Header("3D Radar Chart Configuration")]
    public float cnfg3d_scale;                                                                  // scale of the overall 3D Radar Chart visualization

    [Header("2D Frequency Polygon Configuration")]
    public bool cnfg2d_isFrequencyPolygonVisualizationNormalized;                               // indication whether or not to normalize the data variables' values for visualization, based on configured min and max values (Developer Note: Either normalization _OR_ scaling is applied, but not both at the same time. If normalization is not applied (false), then scaling will be.)
    public float cnfg2d_frequencyPolygonMinimumDataValueForNormalization;                       // minimum value for data variable value normalization range
    public float cnfg2d_frequencyPolygonMaximumDataValueForNormalization;                       // maximum value for data variable value normalization range
    public float cnfg2d_frequencyPolygonNormalizationScaleFactor;                               // helper value: data variable value normalization results in values between 0 and 1, so that additional (linear) scaling is required for better visualization (default: 10.0f)
    public TDRCFrequencyPolygon.DataValueScaling cnfg2d_frequencyPolygonDataValueScaling;       // configured scaling method for the data variable's values transformation (Developer Note: Only applied if normalization is not applied.)
    public float cnfg2d_linearScaleFactor;                                                      // multiplier to be applied within the linear scaling option (default: 1.0f --> no scaling, takes originam data vraible's value as is.)
    public float cnfg2d_frequencyPointDistance;                                                 // value representing the spacial distance between two data varaible values (= Frequency Points) in the 2D space (default: 1.0f)
    public bool cnfg2d_isFrequencyPolygonEnabled;                                               // indicator whether or not to display the Mesh (= area) of a data variable's Frequency Polygon
    public bool cnfg2d_isFrequencyPolygonLineEnabled;                                           // indicator whether or not to display the Line Renderer (= outline) of a data variable's Frequency Polygon
    [Range(0, 255)]
    public int cnfg2d_frequencyPolygonTransparency;                                             // value representing the alpha (= transparency) component of a Frequency Polygon's Mesh
    [Range(0.0f, 1.0f)]
    public float cnfg2d_frequencyPolygonLineWidth;                                              // value representing the line width of a Frequency Polygon's Line Renderer
    [Range(0, 255)]
    public int cnfg2d_frequencyPointTransparency;                                               // value representing the alpha (= transparency) component of a Frequency Polygon's individual Frequency Points
    public float cnfg2d_frequencyPointScale;                                                    // scale for the individual Frequency Points of a Frequency Polygon

    [Header("Guidance Axis Configuration")]
    public float cnfgGdncAxs_axisScale;                                                         // scale for the visual representation of the 3D Radar Chart's time axis
    public float cnfgGdncAxs_pointScale;                                                        // scale for the visual representation of the 3D Radar Chart's time axis' start and end points
    public Color32 cnfgGdncAxs_color;                                                           // color for the visual representation of the 3D Radar Chart's time axis (incl. start and end points)

    [Header("Activation Toggle Configuration")]
    public float cnfgActTggl_yOffset;                                                           // offset along the y-axis for placement of the 3D Radar Chart's Activation Toggle component
    public float cnfgActTggl_scale;                                                             // scale of the 3D Radar Chart's Activation Toggle component
    public float cnfgActTggl_timeThreshold;                                                     // threshold (in seconds) to limit continous interaction with the 3D Radar Chart's Activation Toggle component
    public float activationToggleTimer;                                                         // helper value: indicating (remaining) waiting time (in second) until interaction with the 3D Radar Chart's Activation Toggle component is possible again
    public Color32 cnfgActTggl_colorActivated;                                                  // color for the "active" state of the 3D Radar Chart's Activation Toggle component
    public Color32 cnfgActTggl_colorDeactivated;                                                // color for the "inactive" state of the 3D Radar Chart's Activation Toggle component

    [Header("Rotation Handle Configuration")]
    public bool cnfgRotHndl_isAutoRotatingToUserOnActivation;                                   // indicator whether or not to automatically align the 3D Radar Chart's rotation to the user (= main camera) upon activation
    public float cnfgRotHndl_autoRotationSpeed;                                                 // speed for auto-rotation
    public float cnfgRotHndl_autoRotationEndAngleThreshold;                                     // helper value: threshold (as euler angle) for quitting the auto-rotation
    public float cnfgRotHndl_manualRotationEulerAngle;                                          // rotation (as euler angle) for manual (step-wise) rotation
    public float cnfgRotHndl_radius;                                                            // radius of the visual representation of the 3D Radar Chart's Rotation Handle component
    public float cnfgRotHndl_yOffset;                                                           // offset along the y-axis for the placement of the visual representation of the 3D Radar Chart's Rotation Handle component 
    public float cnfgRotHndl_scale;                                                             // scale of the visual representation of the 3D Radar Chart's Rotation Handle component
    public Color32 cnfgRotHndl_color;                                                           // color of the visual representation of the 3D Radar Chart's Rotation Handle component
    public Material cnfgRotHndl_material;                                                       // reference to the Material of the visual representation of the 3D Radar Chart's Rotation Handle

    [Header("Time Slice Configuration")]
    public Color32 cnfgTmSlc_radarColor;                                                        // color for the Mesh representing the 3D Radar Chart's (interactable) Time Slice component
    public float cnfgTmSlc_boxColliderScale;                                                    // scale of the Time Slice GameObject's box collider component along the x and z axes

    [Header("Time Slice Radar UI Configuration")]
    public float cnfgTmSlcRdrUI_horizontalPositionOffset;                                       // horizontal offset for the placement of the 3D Radar Chart's Time Slice Radar UI component
    public bool cnfgTmSlcRdrUI_isGuidanceDisplayed;                                             // indication whether or not to display visual guidance elements in the 3D Radar Chart's Time Slice Radar UI component
    public Color32 cnfgTmSlcRdrUI_radarColor;                                                   // color for the Mesh of the 3D Radar Chart's Time Slice Radar UI component

    [Header("Time Range Selector Configuration")]
    public float cnfgTmRngSlctr_axisScale;                                                      // scale for the visual feedback axis during the time range selection in a 3D Radar Chart
    public Color32 cnfgTmRngSlctr_visualFeedbackColor;                                          // color for the visual feedback components during the time range selection in a 3D Radar Chart

    [Header("VR and Leap Motion Configuration")]
    public bool cnfgVRLM_isEnabled;                                                             // indicator whether or not to enable interaction with the 3D Radar Chart in VR using the Leap Motion hand controller
    public VirtualRealityLeapMotionInterface vrlmInterface;                                     // reference to main interface responsible for the VR and Leap Motion interaction in regards to the 3D Radar Chart
    public string VRLMGameObjectName;                                                           // name of the GameObject in the Scene Hierarchy that has the VirtualRealityLeapMotionInterface component attached (default: VRLM_Player)

    #region UNITY_EVENT_FUNCTIONS

    /// <summary>
    /// Instantiation and dynamic reference set up.
    /// </summary>
    public void Awake()
    {
        // set up (and attach) required references
        dataLoader = this.gameObject.AddComponent<TDRCDataLoader>();
        if (dataLoader == null) Debug.LogError("[ThreeDimRadarChartInterface] TDRCDataLoader could not be added as component.");
        dataLoader.setTDRCInterface(this);

        freqPolyManager = this.gameObject.AddComponent<TDRCFrequencyPolygonManager>();
        if (freqPolyManager == null) Debug.LogError("[ThreeDimRadarChartInterface] TDRCFrequencyPolygonManager could not be added as component.");
        freqPolyManager.setTDRCInterface(this);

  
        // check if required references have been set
        if (frequencyPolygonParent == null) Debug.LogError("[ThreeDimRadarChartInterface] frequencyPolygonParent in Scene Hierarchy References has not been assigned (using Inspector).");

        if (guidanceAxis == null) Debug.LogError("[ThreeDimRadarChartInterface] guidanceAxis in Scene Hierarchy References has not been assigned (using Inspector).");
        else guidanceAxis.setTDRCInterface(this);

        if (interact_activationToggle == null) Debug.LogError("[ThreeDimRadarChartInterface] interact_activationToggle in Scene Hierarchy References has not been assigned (using Inspector).");
        else interact_activationToggle.setTDRCInterface(this);

        if (interact_rotationHandle == null) Debug.LogError("[ThreeDimRadarChartInterface] interact_rotationHandle in Scene Hierarchy References has not been assigned (using Inspector).");
        else interact_rotationHandle.setTDRCInterface(this);

        if (interact_timeSliceGO == null) Debug.LogError("[ThreeDimRadarChartInterface] interact_timeSliceGO in Scene Hierarchy References has not been assigned (using Inspector).");

        if (timeSliceRenderer == null) Debug.LogError("[ThreeDimRadarChartInterface] timeSliceRenderer in Scene Hierarchy References has not been assigned (using Inspector).");
        else
        {
            timeSliceRenderer.setTDRCInterface(this);
            timeSliceRenderer.setTDRCFreqPolyManager(freqPolyManager);
        }

        if (timeSliceRadarUI == null) Debug.LogError("[ThreeDimRadarChartInterface] timeSliceRadarUI in Scene Hierarchy References has not been assigned (using Inspector).");
        else timeSliceRadarUI.setTDRCInterface(this);

        if (timeRangeSelector == null) Debug.LogError("[ThreeDimRadarChartInterface] timeRangeSelector in Scene Hierarchy References has not been assigned (using Inspector).");
        else timeRangeSelector.setTDRCInterface(this);
    }

    /// <summary>
    /// General update routine.
    /// </summary>
    private void Update()
    {
        // Activation Toggle: handle timer if necessary
        if (activationToggleTimer > 0.0f) activationToggleTimer = activationToggleTimer - Time.deltaTime;
    }

    #endregion


    #region INITIALIZATION

    /// <summary>
    /// Constructor method to initiate the creation of a new 3D Radar Chart with reference to the scene's (main) camera and under consideration of a given position in the 3D space.
    /// </summary>
    /// <param name="cam">Reference to the Scene's (main) camera.</param>
    /// <param name="pos">Position of the 3D Radar Chart in the 3D space.</param>
    public void initWithCameraAndPosition(Camera cam, Vector3 pos)
    {
        this.cam = cam;
        bool hasPositionBeenUpdated = updatePosition(pos);

        // dynamically scale Time Slice GameObject's BoxCollider component based on overall 3D Radar Chart's scale
        BoxCollider bcTimeSliceGO = interact_timeSliceGO.GetComponent<BoxCollider>();
        float bcXZScale = cnfgTmSlc_boxColliderScale * cnfg3d_scale;
        float bcYScale = 0.1f * cnfg3d_scale;
        bcTimeSliceGO.size = new Vector3(bcXZScale, bcYScale, bcXZScale);

        // check whether or not VR and Leap Motion setup need to be performed
        if (cnfgVRLM_isEnabled)
        {
            GameObject vrlmInterfaceGO = GameObject.Find(VRLMGameObjectName);
            if (vrlmInterfaceGO != null)
            {
                vrlmInterface = vrlmInterfaceGO.GetComponent<VirtualRealityLeapMotionInterface>();
                if (vrlmInterface != null)
                {
                    bool hasVRLMBeenSetup = vrlmInterface.setupForVRAndLeapMotionInteraction(this);

                    // initialize data loading once both position and VRLM support have been setup
                    if (hasPositionBeenUpdated && hasVRLMBeenSetup) dataLoader.initiateDataLoading();
                }
                else Debug.LogError("[ThreeDimRadarChartInterface] Unable to find VRLMInterface component of found GameObject.");
            }
            else Debug.LogError("[ThreeDimRadarChartInterface] Unable to find GameObject with VRLMInterface attached.");
        }
        // initialize data loading once position has been set up
        else if (hasPositionBeenUpdated) dataLoader.initiateDataLoading();
    }

    #endregion


    #region STATE_CHANGING

    /// <summary>
    /// Method to update the 3D Radar Chart's position in the 3D space. The position's anchor is at the vertical (y-axis) bottom of the 3D Radar Chart.
    /// </summary>
    /// <param name="pos">Position of the 3D Radar Chart in the 3D space.</param>
    /// <returns>True if the position has been updated.</returns>
    public bool updatePosition(Vector3 pos)
    {
        stt_position = pos;                                 // update (and keep reference as) state property
        this.transform.localPosition = stt_position;        // apply state property to Transform (to actually change the position of the GameObject)
        return true;
    }

    /// <summary>
    /// Method to change the activation state of the 3D Radar Chart, and handle all related interaction and user interface components accordingly.
    /// </summary>
    /// <param name="isActivated">Bool value indicating the new activation state (true = activated, false = deactivated).</param>
    public void setActivationState(bool isActivated)
    {
        stt_isActivated = isActivated;                                                                  // keep track of the new activation state

        guidanceAxis.gameObject.SetActive(stt_isActivated);                                             // set activation state of the Guidance Axis GameObject
        interact_activationToggle.updateState(stt_isActivated);                                         // handle manipulation of Activation Toggle GameObject
        interact_rotationHandle.updateState(stt_isActivated);                                           // handle manipulation of Rotation Handle GameObject

        interact_timeSliceGO.SetActive(stt_isActivated);                                                // handle display of Time Slice
        freqPolyManager.activateFrequencyPointsForIndex(stt_selectedTimeIndex, stt_isActivated);        // handle display of the Frequency Points at the currently selected time index

        if (stt_isActivated) timeSliceRadarUI.updateScaleAndRotation();                                 // handle automatic rotation of Time Slice Radar UI (only upon activation)
        timeSliceRadarUI.gameObject.SetActive(stt_isActivated);                                         // handle display of Time Slice Radar UI

        if (stt_isActivated == false && stt_timeRangeSelectionState == TDRCTimeRangeSelector.RangeSelectionState.FirstTimeRangeIndexSelected) timeRangeSelector.abortTimeRangeSelection();      // handle display of time range selection visual feedback (if necessary)
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Method called to toggle between the activation states of the 3D Radar Chart.
    /// </summary>
    /// <returns>Boolean value representing the new state (true = activated, false = deactivated).</returns>
    public bool triggerActivationToggle()
    {
        // allow interaction only after a certain amount of time after the last interaction
        if (activationToggleTimer <= 0.0f)
        {
            setActivationState(!stt_isActivated);                   // apply new activation state state (by reversing the current one)
            activationToggleTimer = cnfgActTggl_timeThreshold;      // reset activation toggle timer
        }
        return stt_isActivated;
    }

    /// <summary>
    /// Method to move the time slice up by one index (increase index by one).
    /// </summary>
    public void timeSliceMoveUpOneIndex()
    {
        // allow time slice interaction only if chart is activated
        if(stt_isActivated)
        {
            // determine direction of position update
            int updatedTimeIndex = stt_selectedTimeIndex + 1;

            // conduct update
            tryTimeSliceUpdateForIndex(updatedTimeIndex);
        }
    }

    /// <summary>
    /// Function to move the time slice down by one index (decrease index by one).
    /// </summary>
    public void timeSliceMoveDownOneIndex()
    {
        // allow time slice interaction only if chart is activated
        if (stt_isActivated)
        {
            // determine direction of position update
            int updatedTimeIndex = stt_selectedTimeIndex - 1;

            // conduct update
            tryTimeSliceUpdateForIndex(updatedTimeIndex);
        }
    }

    #endregion


    #region TIME_SLICE_MANIPULATION

    private Vector3 helper_timeSlice_lastRigidBodyPositionUpdate;   // helper value to keep track of the last position update of the Time Slice GameObject's RigidBody component (used for "snapping" behaviour when moving the Time Slice)

    /// <summary>
    /// Method to attempt to update the set time slice index based on new input parameters.
    /// </summary>
    /// <param name="updatedTimeIndex">Index of attempted update.</param>
    /// <returns>True if time slice could be updated (input parameters were valid), false if not.</returns>
    public bool tryTimeSliceUpdateForIndex(int updatedTimeIndex)
    {
        // helper value: determine whether time slice update was successful or not
        bool updateSuccessful = false;

        // border handling: do not move outside the valid range of indices (-> restrict interaction to inside the 3D Radar Chart)
        if (updatedTimeIndex != stt_selectedTimeIndex &&
           updatedTimeIndex >= stt_minSelectableTimeIndex && updatedTimeIndex <= stt_maxSelectableTimeIndex)
        {
            // keep track of newly selected (and valid) time index
            bool isTimeIndexUpdated = updateSelectedTimeIndex(updatedTimeIndex);

            // update was possible
            if(isTimeIndexUpdated) updateSuccessful = forceTimeSliceUpdateForIndex(stt_selectedTimeIndex);
        }

        return updateSuccessful;
    }

    /// <summary>
    /// Method to (force) update the set time slice index based on new input parameters.
    /// </summary>
    /// <param name="timeIndex">Index of forced update.</param>
    /// <returns>True once the time slice was updated.</returns>
    public bool forceTimeSliceUpdateForIndex(int timeIndex)
    {
        // time slice selector position update
        helper_timeSlice_lastRigidBodyPositionUpdate = new Vector3(0.0f, freqPolyManager.getYValueForIndex(timeIndex), 0.0f);
        interact_timeSliceGO.transform.localPosition = helper_timeSlice_lastRigidBodyPositionUpdate;

        // BUG BEHAVIOUR: Sometimes the slice renderer is somewhat offset in regards to the Frequency Polygons. 
        // BUG IDENTIFIED: (to re-produce) TimeSlice position changed via HandUI, then rotate chart, then change TimeSlice position again via HandUI
        // BUG FIX: LINE BELOW
        interact_timeSliceGO.transform.localEulerAngles = Vector3.zero;

        // update Frequency Point visualization
        freqPolyManager.activateFrequencyPointsForIndex(timeIndex, stt_isActivated);

        // update Time Slice Renderer
        Mesh radarChartMesh = timeSliceRenderer.displayRadar();

        // update Time Slice Radar UI
        if (radarChartMesh != null)
        {
            timeSliceRadarUI.updateDisplay(radarChartMesh, freqPolyManager.getFrequencyPointListForCurrentlySelectedTimeIndex());
            timeSliceRadarUI.updateLocalPositionY(interact_timeSliceGO.transform.localPosition.y);
        }

        // update Time Range Selection (only if time range selection is currently ongoing)
        if(stt_timeRangeSelectionState == TDRCTimeRangeSelector.RangeSelectionState.FirstTimeRangeIndexSelected)
        {
            timeRangeSelector.updateVisualFeedback(freqPolyManager.getYPosForCurrentlySelectedTimeIndex());
        }

        // handle update of VR and Leap Motion related references if necessary
        if (cnfgVRLM_isEnabled) vrlmInterface.updateHandMenuIntefaces();

        return true;
    }

    /// <summary>
    /// Method to initiate all time index selection related properties.
    /// </summary>
    /// <param name="selectedTimeIndex">Int value representing the currently selected time index.</param>
    /// <param name="minSelectableTimeIndex">Int value representing the currently selectable minimum time index.</param>
    /// <param name="maxSelectableTimeIndex">Int value representing the currently selectable maximum time index.</param>
    /// <param name="datasetMinTimeIndex">Int value representing the minimum time index available in the dataset.</param>
    /// <param name="datasetMaxTimeIndex">Int value representing the maximum time index available in the dataset.</param>
    /// <returns></returns>
    public bool initiateTimeIndicesFromData(int selectedTimeIndex, int minSelectableTimeIndex, int maxSelectableTimeIndex, int datasetMinTimeIndex, int datasetMaxTimeIndex)
    {
        updateSelectedTimeIndex(selectedTimeIndex);
        stt_minSelectableTimeIndex    = minSelectableTimeIndex;
        stt_maxSelectableTimeIndex    = maxSelectableTimeIndex;
        stt_datasetMinTimeIndex       = datasetMinTimeIndex;
        stt_datasetMaxTimeIndex       = datasetMaxTimeIndex;

        return true;
    }

    /// <summary>
    /// Method to update the currently selected index in the time-series data of the 3D Radar Chart.
    /// </summary>
    /// <param name="updatedTimeIndex">Int value representing the new selected time index.</param>
    /// <returns>True once the time index has been updated.</returns>
    public bool updateSelectedTimeIndex(int updatedTimeIndex)
    {
        stt_previousSelectedTimeIndex = stt_selectedTimeIndex;  // keep track of old time index
        stt_selectedTimeIndex = updatedTimeIndex;               // update currently selected time index

        return true;
    }

    /// <summary>
    /// Method to reset the currently selectable minimum and maximum time indices.
    /// </summary>
    /// <returns>True once the indices are reset.</returns>
    public bool resetSelectableTimeIndices()
    {
        stt_minSelectableTimeIndex = stt_datasetMinTimeIndex;
        stt_maxSelectableTimeIndex = stt_datasetMaxTimeIndex;

        return true;
    }

    #endregion
}
