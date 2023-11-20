using System.Text.RegularExpressions;
using UnityEngine;
using bug = System.Diagnostics.Debug;
using UnityDebug = UnityEngine.Debug;




// BodyJointCoordinates class is used as an intermediary point for TCP to store the joint coordinates
// read from TCP, and, for the BodyPositionManager to access and assign the joint data to the appropriate
// limb objects
public class BodyJointCoordinates
{
    private static BodyJointCoordinates _instance;
    private CameraImageFrameStream _frameStream;
    public Vector3[] _jointCoordinateVectors;
    public Vector3 BodyAlignmentPosition;
    public Vector3 _bodyAlignmentOffset;

    public bool _coordinateDataSet { get; set; } = false;
    private bool _initJointCoordinatesCompleted = false;


    // private constructor method
    private BodyJointCoordinates() 
    { 
        _jointCoordinateVectors = new Vector3[25]; 
        _frameStream = new CameraImageFrameStream();
    }

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
            //UnityDebug.Log("BodyJointCoordinates :: Getting coordinates from TCPStream...");


        OP_Body_Coordinates = OP_Body_Coordinates.Substring(3, OP_Body_Coordinates.Length - 5);
        OP_Body_Coordinates = OP_Body_Coordinates.Replace("\n  ", "");

        string[] _coordinateVectors = OP_Body_Coordinates.Split("][");


        string[] _vectorComponents;
        float[] xyz_component = new float[3];
        Vector3 _pointVector;
            //UnityDebug.Log("BodyJointCoordinates :: Number of Vectors -> " + _coordinateVectors.Length.ToString());
        for (int i = 0; i < _coordinateVectors.Length; i++)
        {
            // split based on any number of whitespaces
            _vectorComponents = Regex.Split(_coordinateVectors[i], @"\s+");

            // store the x, y, z components as float values
            for (int j = 0; j < 3; j++) xyz_component[j] = float.Parse(_vectorComponents[j]) / 85; // / 50; // note: uses 35 in old TCPServer class

            // save components as Vector3 and store to body joint coordinate vectors variable
            _pointVector = new Vector3(xyz_component[0], xyz_component[1], xyz_component[2]);
            _jointCoordinateVectors[i] = _pointVector;
        }

        // apply offset if initialization has been completed. Initialization is required
        // to define the body alignment offset value
        if (_initJointCoordinatesCompleted)
        {
            Apply_BodyAlignmentOffset();
            _coordinateDataSet = true;
        }

            //UnityDebug.Log("BodyJointCoordinates :: End of Getting coordinates from TCPStream.");

    }


    // initialize the body alignment offset that defines the position of the hologram
    // character in the game space. Then apply the offset to all of the coordinates
    private static readonly string initBodyPose_string = "[[[1.03118134e+02 6.99873962e+01 2.88619578e-01]\n  [1.03757507e+02 8.75215988e+01 7.35780954e-01]\n  [8.55773468e+01 9.07415543e+01 7.16933608e-01]\n  [6.34864426e+01 6.54687424e+01 7.07642257e-01]\n  [8.04082489e+01 4.20597572e+01 7.24343121e-01]\n  [1.21941612e+02 8.62053604e+01 7.26475894e-01]\n  [1.27820770e+02 6.41296692e+01 7.71827757e-01]\n  [1.08954285e+02 4.01150093e+01 7.46629119e-01]\n  [1.03761574e+02 1.38190353e+02 6.38162971e-01]\n  [9.07512207e+01 1.38192856e+02 6.32528305e-01]\n  [6.02510033e+01 1.58949036e+02 8.16145182e-01]\n  [6.47959747e+01 2.27809174e+02 5.01409650e-01]\n  [1.17400948e+02 1.38182007e+02 6.20658875e-01]\n  [1.37524857e+02 1.60911041e+02 7.70069838e-01]\n  [1.23897186e+02 2.15476379e+02 6.96041703e-01]\n  [9.92138443e+01 6.54317474e+01 3.01264435e-01]\n  [1.06361214e+02 6.54366455e+01 2.87769794e-01]\n  [9.07777481e+01 6.86885452e+01 3.72277319e-01]\n  [1.11543053e+02 6.93356628e+01 1.21816687e-01]\n  [1.42713959e+02 2.30408524e+02 6.66928828e-01]\n  [1.43365540e+02 2.27155731e+02 7.03941524e-01]\n  [1.18062080e+02 2.20677872e+02 5.86193681e-01]\n  [7.12915802e+01 2.32383850e+02 2.03017890e-01]\n  [6.41454773e+01 2.37560364e+02 2.09143758e-01]\n  [6.60891037e+01 2.32355682e+02 2.48061493e-01]]";
    public void InitJointCoordinates(Transform _bodyAlignmentPosition)
    {
            //UnityDebug.Log("BodyJointCoordinates :: initializing joint coordinates...");

        getBodyCoordinatesFromTCPStream(initBodyPose_string);

        //UnityDebug.Log("BodyJointCoordinates :: define offset from transform: " + _bodyAlignmentPosition.name);

        BodyAlignmentPosition = _bodyAlignmentPosition.position;

        UnityDebug.Log($"BodyJointCoordinates :: Body Alignment Position: \n({BodyAlignmentPosition.x:F3}, {BodyAlignmentPosition.y:F3}, {BodyAlignmentPosition.z:F3})");
        
        //UnityDebug.Log("BodyJointCoordinates :: apply offset to coordinates");

        Apply_BodyAlignmentOffset();

        _initJointCoordinatesCompleted = true;
            //UnityDebug.Log("BodyJointCoordinates :: body joint coordinates set.");
    }


    private void CalculateBodyAlignmentOffset()
    {
        _bodyAlignmentOffset.x = _jointCoordinateVectors[0].x - BodyAlignmentPosition.x;
        _bodyAlignmentOffset.y = _jointCoordinateVectors[0].y + BodyAlignmentPosition.y;
        _bodyAlignmentOffset.z = _jointCoordinateVectors[0].z - BodyAlignmentPosition.z;
    }


    // Change DIMENSION_TYPE integer to switch between alignment modes
    // '0': 2D only   '1': 3D Depth
    private readonly int DIMENSION_TYPE = 0; 
    private float[] jointDepthValues = new float[25];
    private string coordinateDebugLog = "";
    public void Apply_BodyAlignmentOffset()
    {
        //UnityDebug.Log("BodyJointCoordinates :: applying offset to coordinates... \n");

        CalculateBodyAlignmentOffset();
        UnityDebug.Log($"BodyJointCoordinates :: Body Alignment Offset Vector: \n({_bodyAlignmentOffset.x:F3}, {_bodyAlignmentOffset.y:F3}, {_bodyAlignmentOffset.z:F3})");

        // calculate the depth values of the joint coordinates
        //if (DIMENSION_TYPE == 1) jointDepthValues = _frameStream.Apply_DepthPositionFromSensor(_jointCoordinateVectors);

        _jointCoordinateVectors[0].x = BodyAlignmentPosition.x;
        _jointCoordinateVectors[0].y = - BodyAlignmentPosition.y;
        _jointCoordinateVectors[0].z = BodyAlignmentPosition.z;

        //coordinateDebugLog = $"Joint 0: ({_jointCoordinateVectors[0].x:F3}, {_jointCoordinateVectors[0].y:F3}, {_jointCoordinateVectors[0].z:F3})\n";

        switch (DIMENSION_TYPE) 
        {
            // using only 2D image processing
            case 0:
                    //UnityDebug.Log("BodyJointCoordinates :: applying 2D offset...");

                for (int i = 1; i < _jointCoordinateVectors.Length; i++)
                {
                    // calculate re-aligned position of joint coordinate
                    _jointCoordinateVectors[i] -= _bodyAlignmentOffset;
                    _jointCoordinateVectors[i].y *= -1;

                    coordinateDebugLog += $"Joint {i}: ({_jointCoordinateVectors[i].x:F3}, {_jointCoordinateVectors[i].y:F3}, {_jointCoordinateVectors[i].z:F3})\n";
                }

                UnityDebug.Log("BodyJointCoordinates :: Post-Offset Joint Coordinates...\n\n" + coordinateDebugLog);
                coordinateDebugLog = "";
                break;

            // enable use of 3D depth camera for calculating depth coordinate
            case 1:
                

                for (int i = 0; i < _jointCoordinateVectors.Length; i++)
                {
                    if (i == 0)
                    {
                        _jointCoordinateVectors[i].z = jointDepthValues[i];
                        coordinateDebugLog = $"Joint 0: ({_jointCoordinateVectors[i].x:F3}, {_jointCoordinateVectors[i].y:F3}, {_jointCoordinateVectors[i].z:F3})\n";
                        continue;
                    }
                    
                    // calculate re-aligned position of joint cordinate
                    _jointCoordinateVectors[i].x -= _bodyAlignmentOffset.x;
                    _jointCoordinateVectors[i].y -= _bodyAlignmentOffset.y;
                    _jointCoordinateVectors[i].y *= -1;

                    // assign calculated depth value to joint
                    _jointCoordinateVectors[i].z = jointDepthValues[i];

                    coordinateDebugLog += $"Joint {i}: ({_jointCoordinateVectors[i].x:F3}, {_jointCoordinateVectors[i].y:F3}, {_jointCoordinateVectors[i].z:F3})\n";
                }

                UnityDebug.Log("BodyJointCoordinates :: Post-Offset Joint Coordinates...\n\n" + coordinateDebugLog);
                coordinateDebugLog = "";
                break;

        }
    }

}
