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
            _limbComponents.UpdateBodyComponents(_bodyJointCoordinates._jointCoordinateVectors);
            _bodyJointCoordinates._coordinateDataSet = false;
        }
    }


    public void AlignObjectToLimb(Vector3 origin_coord, Vector3 end_coord, LimbStruct limb)
    {
        // calculate the position and rotation of the limb object
        limb.obj.transform.position = Vector3.Lerp(origin_coord, end_coord, 0.5f);
        limb.obj.transform.rotation = Quaternion.LookRotation(end_coord - origin_coord);

        float distance = Vector3.Distance(origin_coord, end_coord);
        limb.obj.transform.localScale = new Vector3(limb.obj.transform.localScale.x, limb.obj.transform.localScale.y, distance);


    }
}
