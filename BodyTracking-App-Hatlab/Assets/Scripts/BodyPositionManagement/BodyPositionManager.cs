using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using System.Collections.Generic;

/*
Summary:
The body position manager is used for managing and updating the joint coordinates to the game object
for the user to get visual feedback from their movements.
*/
public class BodyPositionManager : MonoBehaviour
{ 
    LimbComponents _limbComponents;
    public Transform BodyAlignmentPosition_Object; // transform properties of BodyAlignment_Position gameObject
    public GameObject HoloLens_Object; // the main camera game object
    public static Vector3 HoloLens_Transform_position;

    public static Queue<Vector3[]> JointCoordinates_Frames;

    /*
    Summary:
    Initialize body position manager components.
    */
    public void Start()
    {
        // joint vectors queue for updating patient game object
        JointCoordinates_Frames = new Queue<Vector3[]>(); 
        // get main camera tranform position
        HoloLens_Transform_position = HoloLens_Object.transform.position;

        bool limb_setup_status = false;
        _limbComponents = LimbComponents.Instance;
        while (!limb_setup_status)
        {
            // initialize the joint coordinates to starting pose
            TCPStreamCoordinateHandler.InitJointCoordinates(BodyAlignmentPosition_Object);
            // set the patient limb components to init pose
            if (JointCoordinates_Frames.Count > 0) limb_setup_status = _limbComponents.InitLimbs(gameObject, JointCoordinates_Frames.Dequeue());
        }

        // display init pose
        LimbComponents.AlignLimbObjects(_limbComponents.Patient_Limbs);
    }


    /*
    Summary:
    Update is used to get the transform position property of the main camera for tracking the
    translational movement to be applied to the patent game object.
    */
    public void Update()
    {
            HoloLens_Transform_position = HoloLens_Object.transform.position;
            TCPStreamCoordinateHandler.SCALE_FACTOR_OFFSET = HoloLens_Transform_position.z;
    }


    /*
    Summary:
    LateUpdate is used for setting the patient game object Vector3 values to display the new coordinates
    to the user. LateUpdate makes this the last method to run during a frame.
    */
    public void LateUpdate()
    { 
        if (JointCoordinates_Frames.Count > 0) _limbComponents.UpdateBodyComponents(JointCoordinates_Frames.Dequeue());
        LimbComponents.AlignLimbObjects(_limbComponents.Patient_Limbs);
    }


    /*
    Summary:
    Method takes in a joint coordinate vector array and adds it to the frame queue to update
    the patient game object coordinates.

    Parameters:
    Vector3[] >> The joint coordinate vector array produced by the TCPStreamCoordinateHandler
    */
    public static void AddPoseToQueue(Vector3[] jointCoordinateVectors)
    {
        JointCoordinates_Frames.Enqueue(jointCoordinateVectors);
    }
}
