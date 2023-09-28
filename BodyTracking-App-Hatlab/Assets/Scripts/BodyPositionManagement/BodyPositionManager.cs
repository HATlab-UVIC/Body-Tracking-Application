using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BodyPositionManager : MonoBehaviour
{
    BodyJointCoordinates _bodyJointCoordinates;
    public static float testVar = 0;
    private void Start()
    {
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
        testVar = jointCoordinateVectors[0].x;
    }


    
}
