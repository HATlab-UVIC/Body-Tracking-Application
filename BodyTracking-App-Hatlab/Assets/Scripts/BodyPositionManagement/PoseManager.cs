using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoseManager : MonoBehaviour
{
    LimbComponents _limbComponents;
    public LimbStruct[] _poseLimbs;

    // Start is called before the first frame update
    public void Start()
    {
        _limbComponents = LimbComponents.Instance;
        InitPoseLimbs(gameObject);
        gameObject.SetActive(false);
    }


    // ------------------------------------------------------------------------------


    // Takes in the joint vectors from BodyJointCoordinates and assigns/calculates their
    // associated limb positions and vectors
    public void UpdatePoseComponents(LimbStruct[] _limbs)
    {
        for (int i = 0; i < _poseLimbs.Length; i++)
        {
            _poseLimbs[i].limbOrigin = _limbs[i].limbOrigin;
            _poseLimbs[i].limbEnd = _limbs[i].limbEnd;
            _poseLimbs[i].limbVector = _limbs[i].limbVector;
        }
    }


    // initializes the LimbStruct[] array, assigning each limb a name
    // and an int[] jointKey that identifies the two limb endpoint indices
    // for accessing the associated jointCoordinateVectors data array index.
    // Also initialize the limb game objects to the starting position using
    // the joint coordinates
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


    public void CapturePose()
    {
        UpdatePoseComponents(_limbComponents.Patient_Limbs);
        AlignPoseObjects(_poseLimbs);

        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }


    public void TurnOffPose()
    {
        if (gameObject.activeSelf) gameObject.SetActive(false);
    }


    private void AlignPoseObjects(LimbStruct[] limbs)
    {
        for (int i = 0; i < limbs.Length; i++)
        {
            // calculate the position and rotation of the limb object
            limbs[i].obj.transform.SetPositionAndRotation(Vector3.Lerp(limbs[i].limbOrigin, limbs[i].limbEnd, 0.5f),
                                                      Quaternion.LookRotation(limbs[i].limbEnd - limbs[i].limbOrigin));

            float zScale = Vector3.Distance(limbs[i].limbOrigin, limbs[i].limbEnd);
            limbs[i].obj.transform.localScale = new Vector3(LimbComponents.DEFAULT_LIMB_SIZE, LimbComponents.DEFAULT_LIMB_SIZE, zScale);
        }
    }
}
