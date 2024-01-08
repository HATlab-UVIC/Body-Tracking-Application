using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

/*
Summary:
Class is used to convert the data read from the TCP server into Vector3 objects that
can be used to update the limb components of the person game object displayed for the user.
*/
public static class TCPStreamCoordinateHandler
{
    /*
    Summary: 
    Method is an event handler for the 'TCP_Data_Read' event in the 'TCPServer' class.
    This method takes a string input in the specific format '[[[X Y Z][X Y Z]...[X Y Z]]]'
    and converts each of the cooridnates to Vector3 objects. The Vector3 objects are collected
    and added to a queue in the body position manager to be used to update the person game object.

    Parameters:
    string >> The string containing the coordinate data

    Return:
    bool >> Returns the completion/success status of the method
    */
    private static readonly float SCALE_FACTOR_DEFAULT = 85.0f;
    private static readonly float OFFSET_FACTOR = 5f; 
    public static float SCALE_FACTOR_OFFSET { get; set; } = 0.0f;

    public static bool TCPDataReadHandler(string OP_Body_Coordinates_String)
    {
        try
        {
            Vector3[] JointCoordinate_Vectors = new Vector3[25];

            // remove outer braces and '\n'
            OP_Body_Coordinates_String = OP_Body_Coordinates_String.Substring(3, OP_Body_Coordinates_String.Length - 6);
            OP_Body_Coordinates_String = OP_Body_Coordinates_String.Replace("\n  ", "");

            // split string into individual coordinate strings
            string[] CoordinateVectors_Array = OP_Body_Coordinates_String.Split("][");

            string[] XYZ_VectorComponents_Strings;
            float[] XYZ_VectorComponents_Floats = new float[3];
            Vector3 Joint_Vector;
            // convert each joint coordinate into a Vector3
            for (int i = 0; i < CoordinateVectors_Array.Length; i++)
            {
                // split based on any number of whitespaces
                XYZ_VectorComponents_Strings = Regex.Split(CoordinateVectors_Array[i], @"\s+");

                float SCALE_FACTOR = SCALE_FACTOR_DEFAULT + (OFFSET_FACTOR * SCALE_FACTOR_OFFSET);
                // store the x, y, z components as float values
                for (int j = 0; j < 3; j++) XYZ_VectorComponents_Floats[j] = float.Parse(XYZ_VectorComponents_Strings[j]) / SCALE_FACTOR;

                // save components as Vector3 and store to body joint coordinate vectors variable
                Joint_Vector = new Vector3(XYZ_VectorComponents_Floats[0], XYZ_VectorComponents_Floats[1], XYZ_VectorComponents_Floats[2]);
                JointCoordinate_Vectors[i] = Joint_Vector;
            }

            BodyPositionManager.AddPoseToQueue(Apply_BodyAlignmentOffset(JointCoordinate_Vectors));

            return true; // successfully processed data
        }
        catch { return false; }// error processing data 
    }


    /*
    Summary:
    Method is used to apply the coordinate offset to the person game object so it is displayed in 
    front of the user at the initial offset distance specified by the BodyAlignmentPosition game object.
    The method also updated the position of the game object relative to the movement of the user.
        ie. game object moves left, right, up, down, in, out with the user

    Parameters:
    Vector3[] >> The converted joint vectors from TCPDataReadHandler

    Return:
    Vector3[] >> Returns array of joint vectors with offset transformations applied
    */
    public static Vector3 BodyAlignment_Position { get; set; }
    private static Vector3 BodyAlignment_Offset;
    public static Vector3[] Apply_BodyAlignmentOffset(Vector3[] JointCoordinate_Vectors)
    {
        Vector3 TrackedAlignment_Position = TrackSpatialMovement();
        BodyAlignment_Offset = JointCoordinate_Vectors[0] - TrackedAlignment_Position;

        // re-align joint 0 to the alignment position
        JointCoordinate_Vectors[0] = TrackedAlignment_Position;
        JointCoordinate_Vectors[0].y *= -1;

        for (int i = 1; i < JointCoordinate_Vectors.Length; i++)
        {
            // calculate re-aligned position of joint coordinate
            JointCoordinate_Vectors[i] -= BodyAlignment_Offset;
            JointCoordinate_Vectors[i].y *= -1;
        }

        return JointCoordinate_Vectors;
    }


    /*
    Summary:
    Method is used to calculate the offset to be applied to the person game object to mimic 
    the positional movement of the user.

    Return:
    Vector3 >> Returns a vector object containing the alignment position with the user
    movement offset applied
    */
    private static Vector3 TrackSpatialMovement()
    {
        Vector3 _alignment = BodyAlignment_Position;

        _alignment.x += BodyPositionManager.HoloLens_Transform_position.x;
        _alignment.y -= BodyPositionManager.HoloLens_Transform_position.y;
        _alignment.z -= BodyPositionManager.HoloLens_Transform_position.z;

        return _alignment;
    }


    /*
    Summary:
    Method is used to initialize the person game object with an initial static pose prior to
    the application starting.

    Parameters:
    Transform >> Takes in the BodyAlignmentPosition game object to get the position coordinates
    for where to display the person game object
    */
    private static readonly string INIT_BODY_POSE_STRING = "[[[1.03118134e+02 6.99873962e+01 2.88619578e-01]\n  [1.03757507e+02 8.75215988e+01 7.35780954e-01]\n  [8.55773468e+01 9.07415543e+01 7.16933608e-01]\n  [6.34864426e+01 6.54687424e+01 7.07642257e-01]\n  [8.04082489e+01 4.20597572e+01 7.24343121e-01]\n  [1.21941612e+02 8.62053604e+01 7.26475894e-01]\n  [1.27820770e+02 6.41296692e+01 7.71827757e-01]\n  [1.08954285e+02 4.01150093e+01 7.46629119e-01]\n  [1.03761574e+02 1.38190353e+02 6.38162971e-01]\n  [9.07512207e+01 1.38192856e+02 6.32528305e-01]\n  [6.02510033e+01 1.58949036e+02 8.16145182e-01]\n  [6.47959747e+01 2.27809174e+02 5.01409650e-01]\n  [1.17400948e+02 1.38182007e+02 6.20658875e-01]\n  [1.37524857e+02 1.60911041e+02 7.70069838e-01]\n  [1.23897186e+02 2.15476379e+02 6.96041703e-01]\n  [9.92138443e+01 6.54317474e+01 3.01264435e-01]\n  [1.06361214e+02 6.54366455e+01 2.87769794e-01]\n  [9.07777481e+01 6.86885452e+01 3.72277319e-01]\n  [1.11543053e+02 6.93356628e+01 1.21816687e-01]\n  [1.42713959e+02 2.30408524e+02 6.66928828e-01]\n  [1.43365540e+02 2.27155731e+02 7.03941524e-01]\n  [1.18062080e+02 2.20677872e+02 5.86193681e-01]\n  [7.12915802e+01 2.32383850e+02 2.03017890e-01]\n  [6.41454773e+01 2.37560364e+02 2.09143758e-01]\n  [6.60891037e+01 2.32355682e+02 2.48061493e-01]]";
    public static void InitJointCoordinates(Transform BoadyAlignment_Transform)
    {
        BodyAlignment_Position = BoadyAlignment_Transform.position;
        TCPDataReadHandler(INIT_BODY_POSE_STRING);
    }

}
