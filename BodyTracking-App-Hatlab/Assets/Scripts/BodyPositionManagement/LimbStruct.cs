using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public struct LimbStruct 
{
    public string name;
    public Vector3 limbOrigin;
    public Vector3 limbVector;
    public int[] jointKey;
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
