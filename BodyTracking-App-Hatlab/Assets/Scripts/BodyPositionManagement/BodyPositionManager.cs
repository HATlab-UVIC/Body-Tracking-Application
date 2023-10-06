using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BodyPositionManager : MonoBehaviour
{ 
    LimbComponents _limbComponents;
    BodyJointCoordinates _bodyJointCoordinates;
    
    
    // initialize classes upon startup of the application
    private void Start()
    {
        _limbComponents = LimbComponents.Instance;
        _limbComponents.InitLimbs(gameObject);
        _bodyJointCoordinates = BodyJointCoordinates.Instance;
    }


    // on execution of each frame, update the body position data
    void Update()
    {
        if (TCPServer.tcp_server_connected && _bodyJointCoordinates._coordinateDataSet)
        {
            _limbComponents.UpdateBodyComponents(_bodyJointCoordinates._bodyJointCoordinateVectors);
            _bodyJointCoordinates._coordinateDataSet = false;
        }
    }
}
