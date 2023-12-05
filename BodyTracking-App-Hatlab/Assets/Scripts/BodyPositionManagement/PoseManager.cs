using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Summary:
Pose manager is used to store and display a single reference pose (ie. pose snapshot)
from the pose stream produced by the BodyPositionManager.
*/
public class PoseManager : MonoBehaviour
{
    LimbComponents _limbComponents;
    public LimbStruct[] _poseLimbs;

    /*
    Summary:
    Initializes the pose limb struct with the name and joint key values. Then sets
    game object to inactive. Only display reference pose when the 'pose' keyword is said.
    */
    public void Start()
    {
        _limbComponents = LimbComponents.Instance;
        InitPoseLimbs(gameObject);
        gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------------------

    /*
    Summary:
    Makes a copy of the patient limbs coordinate values and stores them to the pose limbs.

    Parameters:
    LimbStruct[] >> The Patient limbs, limb array at the time of execution
    */

    public void UpdatePoseComponents(LimbStruct[] _limbs)
    {
        for (int i = 0; i < _poseLimbs.Length; i++)
        {
            _poseLimbs[i].limbOrigin = _limbs[i].limbOrigin;
            _poseLimbs[i].limbEnd = _limbs[i].limbEnd;
            _poseLimbs[i].limbVector = _limbs[i].limbVector;
        }
    }


    /*
    Summary:
    Method is used to initialize the LimbStruct array, assigning each limb in the array...
    - a name specifying the limb
    - a joint key array specifying the indexes of the Joint_Vectors array that match the joint
    vector to the appropriate limb
    - an object reference to the specific limb object in the scene
    */
    public void InitPoseLimbs(GameObject parentObj)
    {
        int i = 0;
        _poseLimbs = new LimbStruct[16];
        foreach (var key in LimbAssignmentKey._limbKeys)
        {
            _poseLimbs[i].name = key.name;
            _poseLimbs[i].jointKey = key.i;
            _poseLimbs[i++].obj = parentObj.transform.Find(key.name).gameObject;
        }
    }


    /*
    Summary:
    Method is a handler for the voice command 'pose' which captures the body pose at
    the time of the command and displays the frozen pose to the user.
    */
    public void CapturePose()
    {
        // get copy of patient limb pose
        UpdatePoseComponents(_limbComponents.Patient_Limbs);
        LimbComponents.AlignLimbObjects(_poseLimbs);

        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }


    /*
    Summary:
    Method is a handler for the voice command 'pose off' which turns off the frozen body pose
    removing it from the users view.
    */
    public void TurnOffPose()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }
}
