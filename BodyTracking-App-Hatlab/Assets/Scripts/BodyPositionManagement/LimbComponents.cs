using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbComponents
{
    private static LimbComponents _instance;
    public LimbStruct[] _limbs;


    // private constructor method
    private LimbComponents() {}

    // singleton constructor. Used for defining only a single instance that can be referenced
    // from different classes
    public static LimbComponents Instance
    {
        get
        {
            if (_instance == null) _instance = new LimbComponents();
            return _instance;
        }
    }


    // ------------------------------------------------------------------------------


    // Takes in the joint vectors from BodyJointCoordinates and assigns/calculates their
    // associated limb positions and vectors
    public void UpdateBodyComponents(Vector3[] jointCoordinateVectors)
    {
        for (int i = 0; i < _limbs.Length; i++)
        {
            _limbs[i].limbOrigin = jointCoordinateVectors[_limbs[i].jointKey[0]];
            _limbs[i].limbOrigin = jointCoordinateVectors[_limbs[i].jointKey[1]];
            _limbs[i].limbVector = jointCoordinateVectors[_limbs[i].jointKey[1]] - jointCoordinateVectors[_limbs[i].jointKey[0]];
        }
    }


    // initializes the LimbStruct[] array, assigning each limb a name
    // and an int[] jointKey that identifies the two limb endpoint indices
    // for accessing the associated jointCoordinateVectors data array index
    public void InitLimbs(GameObject parentObj)
    {
        int i = 0;
        _limbs = new LimbStruct[16];
        foreach (var key in LimbAssignmentKey._limbKeys)
        {
            _limbs[i].name = key.name;
            _limbs[i].jointKey = key.i;

            GameObject obj = parentObj.transform.Find(key.name).gameObject;
            if (obj) _limbs[i].obj = obj;

        }
    }


}


// -----------------------------------------------------------------
// ---------------- Helper Struct / Class --------------------------


// used as a container for storing data associated to a limb element
public struct LimbStruct
{
    public string name;
    public int[] jointKey;
    public GameObject obj;

    public Vector3 limbOrigin;
    public Vector3 limbEnd;
    public Vector3 limbVector;
}


// class is used for the purpose of holding the names of the limbs
// as well as holding the array index keys that relate the joint
// coordinates to the limb.
//
// ie. _bodyJointCoordinateVectors[1] and _bodyJointCoordinateVectors[2]
// make up the start and end coordinates of the left shoulder.
//
// Use these to update the body component values
public class LimbAssignmentKey
{
    public static readonly (string name, int[] i)[] _limbKeys = new[]
    {
        (name: "neck", i: new int[] {0,1}),
        (name: "leftShoulder", i: new int[] {1,2}),
        (name: "upperLeftArm", i: new int[] {2,3}),
        (name: "lowerLeftArm", i: new int[] {3,4}),
        (name: "rightShoulder", i: new int[] {1,5}),
        (name: "upperRightArm", i: new int[] {5,6}),
        (name: "lowerRightArm", i: new int[] {6,7}),
        (name: "chest", i: new int[] {1,8}),
        (name: "leftHip", i: new int[] {8,9}),
        (name: "leftThigh", i: new int[] {9,10}),
        (name: "leftCalf", i: new int[] {10,11}),
        (name: "rightHip", i: new int[] {8,12}),
        (name: "rightThigh", i: new int[] {12,13}),
        (name: "rightCalf", i: new int[] {13,14}),
        (name: "leftFoot", i: new int[] {24,22}),
        (name: "rightFoot", i: new int[] {21,19})
    };
}
