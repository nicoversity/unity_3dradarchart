/*
 * Demo_3DRadarChart.cs
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
/// This class is responsible for demonstrating the use of (and interaction with) a 3D Radar Chart (TDRC).
/// </summary>
public class Demo_3DRadarChart : MonoBehaviour
{
    [Header("DEMO - Interaction with a 3D Radar Chart")]
    public Camera mainCamera;
    public Camera vrCamera;
    public bool demo_isEnabledForLeapMotionVRUsage;
    private ThreeDimRadarChartInterface demo_TDRCInterface;
    private static readonly string demo_TDRCInterfacePrefab = "3D_Radar_Chart-resources/Prefabs/TDRCInterface";
    public Vector3 demo_TDRCPosition;


    /// <summary>
    /// Method to handle all instantiation related tasks.
    /// </summary>
    private void Awake()
    {
        // set up references for interaction in VR using Leap Motion
        if (demo_isEnabledForLeapMotionVRUsage == true)
        {
            // override reference of default main camera (non VR) to refer to assigned VR camera (previously set via Unity Inspector)
            if (vrCamera != null) mainCamera = vrCamera;
        }

        // check if main camera instance is assigned
        if (mainCamera == null) Debug.LogError("[Demo_3DRadarChart] Please assign a Camera to the mainCamera property.");
    }

    /// <summary>
    /// General update routine.
    /// </summary>
    private void Update()
    {
        // KEYBOARD INTERACTION for demonstration purposes
        //

        // Q - init and load data
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q pressed");

            // only instantiate if not already existing
            if (demo_TDRCInterface == null && mainCamera != null)
            {
                // instantiate new TDRCInterface GameObject (attached as a Child to the GameObject this demo script is attached to) and get reference to ThreeDimRadarChartInterface component
                demo_TDRCInterface = Instantiate(Resources.Load(demo_TDRCInterfacePrefab) as GameObject, this.transform).GetComponent<ThreeDimRadarChartInterface>();
                demo_TDRCInterface.name = "TDRCInterface_1";

                // AFTER instantiation _AND_ BEFORE INITIALIZATION: do additional custom configurations to the ThreeDimRadarChartInterface, e.g., set data source and data source color coding properties, configure data transformation related values (normalization, scaling), etc.
                // Note: Default configuration possible beforehand by editing the Prefab directly in the Resources directory using the Unity Inspector. (Recommendation: If done so, create a copy of the original Prefab for save keeping of original configuration.)
                //
                // Some configuration examples:
                //demo_TDRCInterface.dataSourceLocalFilePath = "3D_Radar_Chart-resources/data/sample_data-3dtimevis";
                //demo_TDRCInterface.isDataLoadedFromServer = false;
                //demo_TDRCInterface.cnfg2d_isFrequencyPolygonVisualizationNormalized = true;
                //demo_TDRCInterface.cnfg2d_frequencyPolygonMinimumDataValueForNormalization = 1;     // based on Resources/3D_Radar_Chart-resources/data/sample_data-3dtimevis
                //demo_TDRCInterface.cnfg2d_frequencyPolygonMaximumDataValueForNormalization = 36;    // based on Resources/3D_Radar_Chart-resources/data/sample_data-3dtimevis
                //demo_TDRCInterface.cnfg2d_frequencyPolygonNormalizationScaleFactor = 10.0f;
                //demo_TDRCInterface.cnfg2d_frequencyPolygonDataValueScaling = TDRCFrequencyPolygon.DataValueScaling.Linear;
                //demo_TDRCInterface.cnfg2d_linearScaleFactor = 0.25f;
                // ...

                // configuration for VR and Leap Motion
                if (demo_isEnabledForLeapMotionVRUsage) demo_TDRCInterface.cnfgVRLM_isEnabled = true;
                else demo_TDRCInterface.cnfgVRLM_isEnabled = false;

                // initialize 3D Radar Chart
                demo_TDRCInterface.initWithCameraAndPosition(mainCamera, demo_TDRCPosition);
            }
        }

        // if 3D Radar Chart has been set up
        if (demo_TDRCInterface != null)
        {
            // A - toggle activation state
            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("A pressed");
                demo_TDRCInterface.triggerActivationToggle();
            }

            // RIGHT / LEFT ARROW KEY - manual rotation of the Rotation Handle
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Debug.Log("RIGHT Arrow pressed");
                demo_TDRCInterface.interact_rotationHandle.rotateRight(demo_TDRCInterface.cnfgRotHndl_manualRotationEulerAngle);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Debug.Log("LEFT Arrow pressed");
                demo_TDRCInterface.interact_rotationHandle.rotateLeft(demo_TDRCInterface.cnfgRotHndl_manualRotationEulerAngle);
            }

            // UP / DOWN ARROW KEY - move Time Slice up and down by one index along the time-series axis
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Debug.Log("UP Arrow pressed");
                demo_TDRCInterface.timeSliceMoveUpOneIndex();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Debug.Log("DOWN Arrow pressed");
                demo_TDRCInterface.timeSliceMoveDownOneIndex();
            }

            // S - iterate through Time Range Selection states
            if (Input.GetKeyDown(KeyCode.S))
            {
                Debug.Log("S pressed");
                demo_TDRCInterface.timeRangeSelector.iterateTimeRangeSelectionState();
            }
        }
    }
}
