using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using System.Collections.Generic;

public class BodyPositionManager : MonoBehaviour
{ 
    LimbComponents _limbComponents;
    public Transform BodyAlignmentPosition_Object; // transform properties of BodyAlignment_Position gameObject
    public GameObject HoloLens_object;
    public static Vector3 HoloLens_Transform_position;

    public static Queue<Vector3[]> _bodyPositions;

    
    public void Start()
    {
        _bodyPositions = new Queue<Vector3[]>();
        HoloLens_Transform_position = HoloLens_object.transform.position;

        //UnityDebug.Log("BodyPositionManager :: Starting BPM Initialization...");
        bool limb_setup_status = false;
        _limbComponents = LimbComponents.Instance;
        while (!limb_setup_status)
        {
            // create the instance of BJC and initialize the joint coordinates to starting pose
            TCPStreamCoordinateHandler.InitJointCoordinates(BodyAlignmentPosition_Object);

            // storing the starting pose joint coordinates to the limb components
            if (_bodyPositions.Count > 0) limb_setup_status = _limbComponents.InitLimbs(gameObject, _bodyPositions.Dequeue());
        }

        AlignLimbObjects(_limbComponents.Patient_Limbs);
        //UnityDebug.Log("BodyPositionManager :: Ending BPM Initialization.");
    }


    public void Update()
    {
        HoloLens_Transform_position = HoloLens_object.transform.position;
        TCPStreamCoordinateHandler.SCALE_FACTOR_OFFSET = HoloLens_Transform_position.z;
    }


    public void LateUpdate()
    {
        //UnityDebug.Log("BodyPositionManager :: Bool States :: TCP Server ("+ TCPServer.tcp_server_connected.ToString()+") Joint Coordinates Set ("+ _bodyJointCoordinates._coordinateDataSet.ToString()+")");
        //UnityDebug.Log("BodyPositionManager :: Loop :: Updating body components :: coorindate queue >> "+_bodyPositions.Count);

        if (_bodyPositions.Count > 0) _limbComponents.UpdateBodyComponents(_bodyPositions.Dequeue()); //_bodyJointCoordinates._jointCoordinateVectors
        AlignLimbObjects(_limbComponents.Patient_Limbs);

        //UnityDebug.Log("BodyPositionManager :: Loop :: Update complete.");
    }


    public void AlignLimbObjects(LimbStruct[] limbs)
    {
        //UnityDebug.Log("BodyPositionManager :: applying coordinates to limb gameObjects...");

        for (int i=0; i < limbs.Length; i++)
        {
            //UnityDebug.Log("BodyJointManager :: Limb data...\n" + limbs[i].obj.name + " | " + limbs[i].limbOrigin.ToString() + " | " + limbs[i].limbEnd.ToString());
            // calculate the position and rotation of the limb object
            limbs[i].obj.transform.SetPositionAndRotation(Vector3.Lerp(limbs[i].limbOrigin, limbs[i].limbEnd, 0.5f),
                                                      Quaternion.LookRotation(limbs[i].limbEnd - limbs[i].limbOrigin));

            float zScale = Vector3.Distance(limbs[i].limbOrigin, limbs[i].limbEnd);
            limbs[i].obj.transform.localScale = new Vector3(LimbComponents.DEFAULT_LIMB_SIZE, LimbComponents.DEFAULT_LIMB_SIZE, zScale);
        }
        //UnityDebug.Log("BodyPositionManager :: end of applying coordinates to limb gameObjects.");

    }


    public static void AddPoseToQueue(Vector3[] jointCoordinateVectors)
    {
        _bodyPositions.Enqueue(jointCoordinateVectors);
    }
}
