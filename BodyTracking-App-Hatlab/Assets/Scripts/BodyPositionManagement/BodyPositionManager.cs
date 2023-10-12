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
    public static Transform BodyAlignmentPosition; // transform properties of BodyAlignmentPosition gameObject

    
    // initialize classes upon startup of the application
    private void Start()
    {
        
        _bodyJointCoordinates = BodyJointCoordinates.Instance;
        _bodyJointCoordinates.InitJointCoordinates(BodyAlignmentPosition);
        _limbComponents = LimbComponents.Instance;
        _limbComponents.InitLimbs(gameObject);
    }


    // on execution of each frame, update the body position data
    void Update()
    {
        if (TCPServer.tcp_server_connected && _bodyJointCoordinates._coordinateDataSet)
        {
            _limbComponents.UpdateBodyComponents(_bodyJointCoordinates._jointCoordinateVectors);
            _bodyJointCoordinates._coordinateDataSet = false;

            AlignLimbObjects(_limbComponents._limbs);

        }
    }


    public void AlignLimbObjects(LimbStruct[] limbs)
    {
        foreach (var limb in limbs)
        {
            // calculate the position and rotation of the limb object
            limb.obj.transform.SetPositionAndRotation(Vector3.Lerp(limb.limbOrigin, limb.limbEnd, 0.5f),
                                                      Quaternion.LookRotation(limb.limbEnd - limb.limbOrigin));

            float zScale = Vector3.Distance(limb.limbOrigin, limb.limbEnd);
            limb.obj.transform.localScale = new Vector3(LimbComponents.DEFAULT_LIMB_SIZE, LimbComponents.DEFAULT_LIMB_SIZE, zScale);
        }

    }
}
