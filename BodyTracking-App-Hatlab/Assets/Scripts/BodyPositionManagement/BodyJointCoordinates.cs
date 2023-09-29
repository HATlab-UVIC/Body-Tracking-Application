using System.Text.RegularExpressions;
using UnityEngine;


// BodyJointCoordinates class is used as an intermediary point for TCP to store the joint coordinates
// read from TCP, and, for the BodyPositionManager to access and assign the joint data to the appropriate
// limb objects
public class BodyJointCoordinates
{
    private static BodyJointCoordinates _instance;
    public Vector3[] _bodyJointCoordinateVectors;
    public bool _coordinateDataSet { get; set; } = false;

    // private constructor method
    private BodyJointCoordinates() { _bodyJointCoordinateVectors = new Vector3[25]; }

    // singleton constructor. Used for defining only a single instance that can be referenced
    // from different classes
    public static BodyJointCoordinates Instance
    {
        get
        {
            if (_instance == null) _instance = new BodyJointCoordinates();
            return _instance;
        }
    }


    // ------------------------------------------------------------------------------


    // method takes the openpose joint coordinate data string from the data reader 
    // and stores the data into a Vector3[] array for further use of joint data
    public void getBodyCoordinatesFromTCPStream(string OP_Body_Coordinates)
    {
        // still using joint data in BodyPositionManager.
        // Will return false when update is done
        if (_coordinateDataSet) return;

        OP_Body_Coordinates = OP_Body_Coordinates.Substring(3, OP_Body_Coordinates.Length - 6);
        OP_Body_Coordinates = OP_Body_Coordinates.Replace("\n  ", "");

        string[] _coordinateVectors = OP_Body_Coordinates.Split("][");


        string[] _vectorComponents;
        float[] xyz_component = new float[3];
        Vector3 _pointVector;
        for (int i = 0; i < _coordinateVectors.Length; i++)
        {
            // split based on any number of whitespaces
            _vectorComponents = Regex.Split(_coordinateVectors[i], @"\s+");

            // store the x, y, z components as float values
            for (int j = 0; j < 3; j++) xyz_component[j] = float.Parse(_vectorComponents[j]); // / 50; // note: uses 35 in old TCPServer class

            // save components as Vector3 and store to body joint coordinate vectors variable
            _pointVector = new Vector3(xyz_component[0], xyz_component[1], xyz_component[2]);
            _bodyJointCoordinateVectors[i] = _pointVector;
        }
        _coordinateDataSet = true;
    }
}
