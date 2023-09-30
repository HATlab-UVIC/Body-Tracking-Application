using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BodyPositionManager : MonoBehaviour
{
    public static LimbStruct[] _limbs; 

    BodyJointCoordinates _bodyJointCoordinates;
    public static float testVar = 0;
    private void Start()
    {
        init_limbs();
        _bodyJointCoordinates = BodyJointCoordinates.Instance;
    }

    void Update()
    {
        if (TCPServer._TCP_Connected && _bodyJointCoordinates._coordinateDataSet)
        {
            UpdateBodyComponents(_bodyJointCoordinates._bodyJointCoordinateVectors);
            _bodyJointCoordinates._coordinateDataSet = false;
        }
    }


    // Takes in the joint vectors from BodyJointCoordinates and assigns/calculates their
    // associated limb positions and vectors
    private void UpdateBodyComponents(Vector3[] jointCoordinateVectors)
    {
        for (int i = 0; i < _limbs.Length; i++) 
        {
            _limbs[i].limbOrigin = jointCoordinateVectors[_limbs[i].jointKey[0]];
            _limbs[i].limbVector = jointCoordinateVectors[_limbs[i].jointKey[1]] - jointCoordinateVectors[_limbs[i].jointKey[0]];
        }
    }


    // initializes the LimbStruct[] array, assigning each limb a name
    // and an int[] jointKey that identifies the two limb endpoint indices
    // for accessing the associated jointCoordinateVectors data array index
    private void init_limbs()
    {
        int i = 0;
        _limbs = new LimbStruct[16];
        foreach (var key in LimbAssignmentKey._limbKeys) 
        { 
            _limbs[i].name = key.name;
            _limbs[i++].jointKey = key.i;
        }
    }
}
