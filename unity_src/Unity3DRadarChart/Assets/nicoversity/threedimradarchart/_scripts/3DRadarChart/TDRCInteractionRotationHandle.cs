using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity.Interaction;

/// <summary>
/// Class responsible for handling the state and the interaction with the 3D Radar Chart's Rotation Interactable, i.e., a (graphical) user interface element for the purpose of rotating its associated 3D Radar Chart in place in order to examine it from different angles.
/// </summary>
public class TDRCInteractionRotationHandle : MonoBehaviour
{
    // RigidBody and Interaction Behaviour configurations (used for hand interaction via Leap Motion):
    // RigidBody:
    //  - Mass: 1; Drag: 0; Angular Drag: 5; Use Gravity: True; Is Kinematic: False; Interpolate: Interpolate; Collision Detection: Discrete
    //  - Freeze Position = x: true, y: true, z: true
    //  - Freeze Rotation = x: true, y: false, z: true
    // Interaction Behaviour:
    //  - Ignore Contact: false; Ignore Grasping: false
    //  - Move Object When Grasped: True; Grasped Movement Type: Inherit

    // private properties
    private ThreeDimRadarChartInterface tdrcInterface;      // reference to overall interface
    private Quaternion destinationRotation;                 // helper value to keep track of a set destination rotation (used for rotation over time, e.g., via Lerp)
    private float manualRotationY;                          // helper value to keep track of a "manual rotation", i.e., rotation using the provided methods rather than direct (Leap Motion) interaction with the Rotation Handle's box collider                

    // GUI related internal properties, in order to dynamically build a rotation handle based on the 3D Radar Chart's properties (e.g., amount of data variables)
    private List<GameObject> vertices;                      // list holding all vertices required to create the Rotation Handle
    private List<GameObject> edges;                         // list holding all edges required to create the Rotation Handle
    public GameObject parentGameObjectHolder;               // reference to the GameObject all vertices and edges are attached to as Child GameObjects in the scene hierachy
    private BoxCollider parentBoxCollider;                  // one box collider to represent the entire Rotation Handle (used instead of, e.g., individual ones for each vertex and edge)


    /// <summary>
    /// General update loop.
    /// </summary>
    public void Update()
    {
        // handle auto rotation if necessary
        if (tdrcInterface.stt_isRotating)
        {
            // apply rotation
            transform.rotation = Quaternion.Lerp(this.transform.rotation, destinationRotation, Time.deltaTime * tdrcInterface.cnfgRotHndl_autoRotationSpeed);

            // check if rotation is (near to) finished
            float angle = Quaternion.Angle(this.transform.rotation, destinationRotation);
            if (angle <= tdrcInterface.cnfgRotHndl_autoRotationEndAngleThreshold) tdrcInterface.stt_isRotating = false;
        }
    }


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
    /// Method to adjust the state of the Rotation Handle based on the entered state.
    /// </summary>
    /// <param name="state">Boolean value representing the state of the Rotation Handle (true = activated, false = deactivated).</param>
    public void updateState(bool state)
    {
        setActivation(state);
        if(tdrcInterface.cnfgVRLM_isEnabled) this.GetComponent<InteractionBehaviour>().enabled = state;

        // automatically adjust rotation towards the user (i.e., the user's main camera) if configured and required
        if (tdrcInterface.cnfgRotHndl_isAutoRotatingToUserOnActivation && state)
        {
            manualRotationY = tdrcInterface.cam.transform.rotation.eulerAngles.y;
            initAutoRotation(Quaternion.Euler(0.0f, manualRotationY, 0.0f));
        }
    }

    /// <summary>
    /// Method to setup auto rotation related properties, and initiate auto rotation accodingly.
    /// </summary>
    /// <param name="destinationRot">The destination angle of the Rotation Handle (and its attached 3D Radar Chart) in Quaternion notation.</param>
    private void initAutoRotation(Quaternion destinationRot)
    {
        destinationRotation = destinationRot;
        tdrcInterface.stt_isRotating = true;
    }

    #endregion


    #region DISPLAY

    /// <summary>
    /// Method to initialze the visual, interactable representation of the Rotation Handle.
    /// </summary>
    /// <param name="yPos">Float value representing the Rotation Handle's y-position in the 3D space.</param>
    /// <param name="resolution">Int value representing the resolution of the handle (= amount of 3D Radar Chart's data variables).</param>
    public void initWithProperties(float yPos, int resolution)
    {
        // create an empty GameObject as parent for all 3D primitives (i.e., vertices and edges)
        parentGameObjectHolder = new GameObject();
        parentGameObjectHolder.transform.SetParent(this.transform);
        parentGameObjectHolder.transform.name = "TDRCInteractionRotationHandle-GUI";

        // init internal data structures
        vertices = new List<GameObject>();
        edges    = new List<GameObject>();

        // update material color
        tdrcInterface.cnfgRotHndl_material.color = tdrcInterface.cnfgRotHndl_color;

        // init "vertices"
        for (int i = 1; i <= resolution; i++)
        {
            // create 3D primitve, set its name and parent
            GameObject vertex = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vertex.name = "Vertex_" + i;
            vertex.transform.SetParent(parentGameObjectHolder.transform);

            // set position and rotation
            vertex.transform.localPosition = new Vector3(0.0f, 0.0f, tdrcInterface.cnfgRotHndl_radius / tdrcInterface.cnfgRotHndl_scale);
            float angle = 360.0f / resolution * (i - 1);
            vertex.transform.RotateAround(Vector3.zero, Vector3.up, angle);

            // manipulate additional components
            Destroy(vertex.GetComponent<SphereCollider>());
            vertex.GetComponent<Renderer>().material = tdrcInterface.cnfgRotHndl_material;

            // keep track of every vertex
            vertices.Add(vertex);
        }

        // init "edges"
        for (int i = 1; i <= resolution; i++)
        {
            // create 3D primitive, set its name and parent
            GameObject edge = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            edge.name = "Edge_" + i;
            edge.transform.SetParent(parentGameObjectHolder.transform);

            // determine center of edge, and set its position
            Vector3 startPoint = vertices[i - 1].transform.localPosition;
            Vector3 endPoint   = (i == resolution) ? vertices[0].transform.localPosition : vertices[i].transform.localPosition;
            Vector3 center     = Vector3.Lerp(startPoint, endPoint, 0.5f);
            edge.transform.localPosition = center;

            // set rotation
            float yRotation = (360.0f / resolution * (i - 1)) + ((360.0f / resolution) * 0.5f);
            edge.transform.localEulerAngles = new Vector3(0.0f, yRotation, 90.0f);

            // set scale
            float halfDistance = Mathf.Abs(Vector3.Distance(startPoint, endPoint)) * 0.5f;  // works for capsules and cylinders
            edge.transform.localScale = new Vector3(1.0f, halfDistance, 1.0f);

            // manipulate additional components
            Destroy(edge.GetComponent<CapsuleCollider>());
            edge.GetComponent<Renderer>().material = tdrcInterface.cnfgRotHndl_material;

            // keep track of every vertex
            edges.Add(edge);
        }

        // set position, rotation, and scale of parent game object
        parentGameObjectHolder.transform.localPosition    = new Vector3(0.0f, yPos, 0.0f);
        parentGameObjectHolder.transform.localEulerAngles = new Vector3(0.0f, ((360.0f / resolution) * 0.5f), 0.0f);
        parentGameObjectHolder.transform.localScale       = (Vector3.one * tdrcInterface.cnfgRotHndl_scale) * tdrcInterface.cnfg3d_scale;

        // setup one collider representing the entire GameObject
        parentBoxCollider         = parentGameObjectHolder.AddComponent<BoxCollider>();
        parentBoxCollider.enabled = false;
        float boxColliderWidth    = (tdrcInterface.cnfgRotHndl_radius / tdrcInterface.cnfgRotHndl_scale) * 1.8f;
        parentBoxCollider.size    = new Vector3(boxColliderWidth, 1.0f, boxColliderWidth);
    }

    #endregion


    #region INTERACTION

    /// <summary>
    /// Method to activate/deactive all components representing the visual, interactable Rotation Handle.
    /// </summary>
    /// <param name="activated">Boolean value representing the Rotation Handle's activation status (true = activated, false = deactivated).</param>
    private void setActivation(bool activated)
    {
        // manipulate all vertices
        if (vertices != null)
        {
            foreach (GameObject v in vertices)
            {
                v.SetActive(activated);
            }
        }

        // manipulate all edges
        if (edges != null)
        {
            foreach (GameObject e in edges)
            {
                e.SetActive(activated);
            }
        }

        // manipulate box collider
        if (parentBoxCollider != null) parentBoxCollider.enabled = activated;
    }

    /// <summary>
    /// Method to manually rotate the Rotation Handle (and thus all it's child GameObjects in the scene hierarchy, e.g., the 3D Radar Chart visualization) to the right.
    /// </summary>
    /// <param name="eulerAngle">Float value representing the euler angle by which the rotation should be modified.</param>
    public void rotateRight(float eulerAngle)
    {
        // inverse rotation angle for rotation to the right
        rotate(eulerAngle * -1.0f);
    }

    /// <summary>
    /// Method to manually rotate the Rotation Handle (and thus all it's child GameObjects in the scene hierarchy, e.g., the 3D Radar Chart visualization) to the left.
    /// </summary>
    /// <param name="eulerAngle">Float value representing the euler angle by which the rotation should be modified.</param>
    public void rotateLeft(float eulerAngle)
    {
        // apply rotation as is for rotation to the left
        rotate(eulerAngle);
    }

    /// <summary>
    /// Method to manually rotate the Rotation Handle (and thus all it's child GameObjects in the scene hierarchy, e.g., the 3D Radar Chart visualization) according to a given angle in euler notation.
    /// </summary>
    /// <param name="eulerAngle">Float value representing the euler angle by which the rotation should be modified.</param>
    private void rotate(float eulerAngle)
    {
        // allow manual rotation only if chart is activated
        if (tdrcInterface.stt_isActivated)
        {
            // keep track of new manual rotation angle and start rotation
            manualRotationY = manualRotationY + eulerAngle;
            if (tdrcInterface.stt_isRotating == false) initAutoRotation(Quaternion.Euler(0.0f, manualRotationY, 0.0f));
        }
    }

    #endregion
}
