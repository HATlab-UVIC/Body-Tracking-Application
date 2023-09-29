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

    private void UpdateBodyComponents(Vector3[] jointCoordinateVectors)
    {
        for (int i = 0; i < _limbs.Length; i++) 
        {
            _limbs[i].limbOrigin = jointCoordinateVectors[_limbs[i].jointKey[0]];
            _limbs[i].limbVector = jointCoordinateVectors[_limbs[i].jointKey[1]] - jointCoordinateVectors[_limbs[i].jointKey[0]];
        }
    }


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
