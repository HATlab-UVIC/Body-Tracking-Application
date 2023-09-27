using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BodyPositionManager // : MonoBehaviour
{
    private static BodyPositionManager _instance;

    public Vector3[] _bodyJointCoordinateVectors;


    // private constructor. Use instance to create one instance of this class
    private BodyPositionManager()
    {
         this._bodyJointCoordinateVectors = new Vector3[25];
    }

    // used to create a single instance of BodyPositionManager. We do not need
    // more than one instance while the application is running
    public static BodyPositionManager Instance
    {
        get
        {
            if (_instance == null) _instance = new BodyPositionManager();
            return _instance;
        }
    }


    void Update()
    {
    }


    // method takes the openpose joint coordinate data string from the data reader 
    // and stores the data into a Vector3[] array for further use of joint data
    public bool getBodyCoordinatesFromTCPStream(string OP_Body_Coordinates)
    {
        OP_Body_Coordinates = OP_Body_Coordinates.Substring(3, OP_Body_Coordinates.Length - 6);
        OP_Body_Coordinates = OP_Body_Coordinates.Replace("\n  ","");
        
        string[] _coordinateVectors = OP_Body_Coordinates.Split("][");

        string[] _vectorComponents;

        Vector3 _pointVector;
        for (int i=1; i < _coordinateVectors.Length - 1; i++)
        {
            // split based on any number of whitespaces
            _vectorComponents = Regex.Split(_coordinateVectors[i],@"\s+");


            float v0 = float.Parse(_vectorComponents[0]);// / 50;
            float v1 = float.Parse(_vectorComponents[1]);// / 50;
            float v2 = float.Parse(_vectorComponents[2]);// / 50; // note: uses 35 in old TCPServer class

            _pointVector = new Vector3(v0, v1, v2);
            _bodyJointCoordinateVectors[i] = _pointVector;

        }
        return true;
    }
}
