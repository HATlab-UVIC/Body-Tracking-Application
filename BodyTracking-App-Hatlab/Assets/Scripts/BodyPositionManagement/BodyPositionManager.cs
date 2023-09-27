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
        float[] xyz_component = new float[3];
        Vector3 _pointVector;
        for (int i=0; i < _coordinateVectors.Length; i++)
        {
            // split based on any number of whitespaces
            _vectorComponents = Regex.Split(_coordinateVectors[i],@"\s+");

            // store the x, y, z components as float values
            for (int j = 0; j < 3; j++) xyz_component[j] = float.Parse(_vectorComponents[j]); // / 50; // note: uses 35 in old TCPServer class

            // save components as Vector3 and store to body joint coordinate vectors variable
            _pointVector = new Vector3(xyz_component[0], xyz_component[1], xyz_component[2]);
            _bodyJointCoordinateVectors[i] = _pointVector;
        }
        return true;
    }
}
