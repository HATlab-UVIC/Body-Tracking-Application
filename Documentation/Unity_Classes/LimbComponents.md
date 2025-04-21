# Limb Components  

1. Class : LimbComponents  
2. Struct : LimbStruct  
3. Class : LimbAssignmentKey  

***Note**: Each of LimbComponents, LimbStruct, and LimbAssignmentKey are defined inside the **LimbComponents.cs file***

# 1. Class : LimbComponents  

## Method List  

```C#
public void UpdateBodyComponents(Vector3[]);
public bool InitLimbs(GameObject, Vector3[]);
public static bool AlignLimbObjects(LimbStruct[]);
```  

## Class Description  

The `class : LimbComponents` class is a *Singleton class* that represents the users body position data (**LimbComponents = Person/Patient**). When instantiated, is an object that is used to store and manipulate the joint coordinate data received from the input stream. The object instance contains a `struct : LimbStruct` array that is used as the storage container for the incoming joint coordinate data. The methods of the LimbComponents class are used for manipulating and updating the coordinate data withing the LimbStruct element.

> **Singleton**: meaning we can only define one object instance of this class. This is useful when we need to access/reference data from the LimbComponents instance from multiple different classes or scripts.  

> *Note:* Looking towards future developments where we may want to track and manage data for multiple persons, it may be beneficial to look into removing the singleton/single instance component so that multiple instances can be created and multiple body positions can be managed at once.    

## Initializing the LimbStruct  

Initializing the `struct : LimbStruct` element is an important step for processing the coordinate data. Once initialized, the limb struct contains keys to help with quick accessing and modification of the coordinate data as it is updated each frame. These keys are stored within the `class : LimbAssignmentKey` class and are stored to the `object : LimbStruct` objects during initialization.  

```C#
public bool InitLimbs(GameObject parentObj, Vector3[] Joint_Vectors)
{
    int i = 0;
    Patient_Limbs = new LimbStruct[16];
    foreach (var key in LimbAssignmentKey._limbKeys)
    {
        Patient_Limbs[i].name = key.name;
        Patient_Limbs[i].jointKey = key.i;
        Patient_Limbs[i++].obj = parentObj.transform.Find(key.name).gameObject;
    }

    UpdateBodyComponents(Joint_Vectors);

    return true;
}
```  

Each limb struct element holds 3 important values:  

* **name**: Used for identifying the current limb object and for accessing the associated limb game object.  
* **jointKey**: Used for direct array accessing of the origin and end point joint coordinates from the joint coordinates `object : Vector3[]` array.  
* **obj**: Used for directly accessing the associated limb game object.  

It is important to note that the `script : LimbComponents.cs` script must be attached to the game object layer that contains all of the limb game objects as child layers. The limb game object references are found by fist referencing the parent game object (`GameObject : parentObj`). Once initialized, we then have direct access the name, joint coordinate array indices, and the game object associated to each limb.  

## Updating and Displaying  

In order to update the `object : LimbComponents` object  instance with the new joint coordinate values we must call the `method : UpdateBodyComponents()` method. This method takes in the `object : Vector3[]` array and stores each of:  

* Limb origin point coordinate  
* Limb end point coordinate  
* Delta vector between the origin and end points  

To complete the update process and display the new patient pose to the user, we will use the `method : AlignLimbObjects()` method. This method takes in the `object : LimbStruct[]` array and cycles through each limb element, accessing and updating each limb game object through the `LimbStruct : variable : obj` variable.  

All of the limb position, limb rotation, and limb length are set for each limb. By updating the state of the limb game objects, we are effectively updating and displaying the new patient pose back to the user.  

# 2. Struct : LimbStruct  

# Struct Description  

The `struct : LimbStruct` struct is a defined storage container for managing all of the important data pertaining to a limb object, including data for faster data accessing.  

```C#
public struct LimbStruct
{
    public string name;
    public int[] jointKey;
    public GameObject obj;

    public Vector3 limbOrigin;
    public Vector3 limbEnd;
    public Vector3 limbVector;
}
```  

|Variable|Description|
|--------|-----------|
|name|The name of the limb element (ie. neck, leftShoulder, rightHip...). These names are used to find and access the associated limb game object in the unity scene that has the same name. The name is also an identifier to which joint coordinates should be stored within the struct element.|
|jointKey|A 2 element array containing the integer index positions of the starting and ending joint coordinates in the `object : Vector3[]` array. Since the incoming coordinate data will always be received in the same order, these index integers allow for direct accessing of the joint coordinte array.|
|obj|A game object reference to the limb game object that is associated with the data stored in the limb struct element (ie. 'rightCalf' game object reference for the 'rightCalf' LimbStruct element).| 
|-|-|
|limbOrigin|The joint coordinate associated with the starting point of the limb.|
|limbEnd|The joint coordinate associated with the end point of the limb.|
|limbVector|The vector defined by the difference between the origin point and the end point.|  

# 3. Class : LimbAssignmentKey  

## Class Description  

The `class : LimbAssignmentKey` class is used as a read-only dictionary used for initializing the `object : LimbStruct[]` array. Contained within the dictionary the names of each limb and an integer array containing the indices of the origin and end points in the joint coordinates `object : Vector3[]` array associated to the limb.  

```C#
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
```  

The `variable : _limbKeys` variable can be used and iterated through to initialize the direct accesing variables of each `object : LimbStruct` element.