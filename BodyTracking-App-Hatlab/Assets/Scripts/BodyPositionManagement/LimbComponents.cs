using UnityEngine;
using System;

/*
Summary:
Class is used to store the relevant limb data used for updating the patient model game object
*/
public class LimbComponents
{
    private static LimbComponents _instance;
    public LimbStruct[] Patient_Limbs;

    /*
    Summary:
    LimbComponents class constructor.
    */
    private LimbComponents() {}


    /*
    Summary:
    Singleton constructor. Used to define a single instance that can be accessed from different classes.
    */
    public static LimbComponents Instance
    {
        get
        {
            if (_instance == null) _instance = new LimbComponents();
            return _instance;
        }
    }


    // ------------------------------------------------------------------------------

    /*
    Summary:
    Method is used to update the LimbStruct elements with the appropriate joint coordinates

    Parameters:
    Vector3[] >> The joint vector array produced by the TCPStreamCoordinateHandler class
    */
    public void UpdateBodyComponents(Vector3[] Joint_Vectors)
    {
        for (int i = 0; i < Patient_Limbs.Length; i++)
        {
            Patient_Limbs[i].limbOrigin = Joint_Vectors[Patient_Limbs[i].jointKey[0]];
            Patient_Limbs[i].limbEnd = Joint_Vectors[Patient_Limbs[i].jointKey[1]];
            Patient_Limbs[i].limbVector = Joint_Vectors[Patient_Limbs[i].jointKey[1]] - Joint_Vectors[Patient_Limbs[i].jointKey[0]];
        }
    }


    /*
    Summary:
    Method is used to initialize the LimbStruct array, assigning each limb in the array...
    - a name specifying the limb
    - a joint key array specifying the indexes of the Joint_Vectors array that match the joint
    vector to the appropriate limb
    - an object reference to the specific limb object in the scene
    Method also updates the the stored coordinate values with the initializing body pose.
    
    Parameters:
    GameObject >> The game object attached to the BodyPositionManager object
    Vector3[] >> The array of joint vectors specifying the initializing pose
    */
    public bool InitLimbs(GameObject parentObj, Vector3[] Joint_Vectors)
    {
        int i = 0;
        Patient_Limbs = new LimbStruct[16];
        foreach (var key in LimbAssignmentKey._limbKeys)
        {
            Patient_Limbs[i].name = key.name;
            Patient_Limbs[i].jointKey = key.i;
            // get game object reference to specific limb objects in the scene
            Patient_Limbs[i++].obj = parentObj.transform.Find(key.name).gameObject;
        }

        UpdateBodyComponents(Joint_Vectors);

        return true;
    }

    /*
    Summary:
    Method is used to access the game objects associated to each limb and update their transform
    properties with that of the updated joint coordinates stored into the limb struct element.
    
    game object properties set:
    - sets object transform position to center point of limb 
    - sets the object orientation along limbOrigin -> limbEnd vector
    - sets the object length to be the distance between the origin and end coordinates

    Parameters:
    LimbStruct[] >> The array of limb objects that contains the coordinate data for each joint

    Return:
    bool >> Returns the success status of the method
    */
    public static readonly float DEFAULT_LIMB_SIZE = 0.1f;
    public static bool AlignLimbObjects(LimbStruct[] limbs)
    {
        try
        {
            for (int i = 0; i < limbs.Length; i++)
            {
                limbs[i].obj.transform.SetPositionAndRotation(Vector3.Lerp(limbs[i].limbOrigin, limbs[i].limbEnd, 0.5f),
                                                          Quaternion.LookRotation(limbs[i].limbEnd - limbs[i].limbOrigin));

                float zScale = Vector3.Distance(limbs[i].limbOrigin, limbs[i].limbEnd);
                limbs[i].obj.transform.localScale = new Vector3(LimbComponents.DEFAULT_LIMB_SIZE, LimbComponents.DEFAULT_LIMB_SIZE, zScale);
            }
            return true;
        }
        catch { return false; }
    }
}

// ------------------------------------------------------------------------------
//                              Helper Struct/Class
// ------------------------------------------------------------------------------

/*
Summary:
A helper struct used as a container for managing all the data relavent to
each limb. This data is used for updating the limb game objects in the scene.
*/
public struct LimbStruct
{
    public string name;
    public int[] jointKey;
    public GameObject obj;

    public Vector3 limbOrigin;
    public Vector3 limbEnd;
    public Vector3 limbVector;
}


/*
Summary:
class is used for the purpose of holding the names of the limbs as well as holding 
the array index keys that relate the joint coordinates to the limb.

ie. 
_jointCoordinateVectors[1] and _jointCoordinateVectors[2]
make up the start and end coordinates of the left shoulder.

This should be used only as a reference dictionary to initialize the LimbStruct fields.
*/
public class LimbAssignmentKey
{
    public static readonly (string name, int[] i)[] _limbKeys = new[]
    {
        (name: "neck", i: new int[] {0,1}),
        (name: "leftShoulder", i: new int[] {1,2}),
        (name: "upperLeftArm", i: new int[] {2,3}),
        (name: "lowerLeftArm", i: new int[] {3,4}),
        (name: "rightShoulder", i: new int[] {1,5}),
        (name: "upperRightArm", i: new int[] {5,6}),
        (name: "lowerRightArm", i: new int[] {6,7}),
        (name: "chest", i: new int[] {1,8}),
        (name: "leftHip", i: new int[] {8,9}),
        (name: "leftThigh", i: new int[] {9,10}),
        (name: "leftCalf", i: new int[] {10,11}),
        (name: "rightHip", i: new int[] {8,12}),
        (name: "rightThigh", i: new int[] {12,13}),
        (name: "rightCalf", i: new int[] {13,14}),
        (name: "leftFoot", i: new int[] {24,22}),
        (name: "rightFoot", i: new int[] {21,19})
    };
}
