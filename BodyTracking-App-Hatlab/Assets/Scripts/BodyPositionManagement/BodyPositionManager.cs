using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using bug = System.Diagnostics.Debug;

public class BodyPositionManager : MonoBehaviour
{ 
    LimbComponents _limbComponents;
    BodyJointCoordinates _bodyJointCoordinates;
    public Transform BodyAlignmentPosition; // transform properties of BodyAlignmentPosition gameObject

    
    // initialize classes upon startup of the application
    public void Start()
    {
        UnityDebug.Log("Starting BPM Initialization:");
        // create the instance of BJC and initialize the joint coordinates to starting pose
        _bodyJointCoordinates = BodyJointCoordinates.Instance;
        _bodyJointCoordinates.InitJointCoordinates(BodyAlignmentPosition);

        // create the instance of LC and initialize the limb struct array components
        // storing the starting pose joint coordinates to the limb components
        _limbComponents = LimbComponents.Instance;
        _limbComponents.InitLimbs(gameObject, _bodyJointCoordinates._jointCoordinateVectors);

        
        AlignLimbObjects(_limbComponents._limbs);
        UnityDebug.Log("Ending BPM Initialization");

    }


    // on execution of each frame, update the body position data
    public void Update()
    {
        if (TCPServer.tcp_server_connected && _bodyJointCoordinates._coordinateDataSet)
        {
            UnityDebug.Log("BPM Update... (TCP Connected & BJC coordinates set)");

            _limbComponents.UpdateBodyComponents(_bodyJointCoordinates._jointCoordinateVectors);
            AlignLimbObjects(_limbComponents._limbs);

            _bodyJointCoordinates._coordinateDataSet = false;
        }
    }


    public void AlignLimbObjects(LimbStruct[] limbs)
    {
        UnityDebug.Log("    applying coordinates to limb gameObjects...");

        for (int i=0; i < limbs.Length; i++)
        {
            bug.WriteLine(limbs[i].obj.name + " | " + limbs[i].limbOrigin.ToString() + " | " + limbs[i].limbEnd.ToString());
            // calculate the position and rotation of the limb object
            limbs[i].obj.transform.SetPositionAndRotation(Vector3.Lerp(limbs[i].limbOrigin, limbs[i].limbEnd, 0.5f),
                                                      Quaternion.LookRotation(limbs[i].limbEnd - limbs[i].limbOrigin));

            float zScale = Vector3.Distance(limbs[i].limbOrigin, limbs[i].limbEnd);
            limbs[i].obj.transform.localScale = new Vector3(LimbComponents.DEFAULT_LIMB_SIZE, LimbComponents.DEFAULT_LIMB_SIZE, zScale);
        }
        UnityDebug.Log("    end of applying coordinates to limb gameObjects.");

    }
}
