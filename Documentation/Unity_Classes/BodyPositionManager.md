# Class : BodyPositionManager  

## Method List  

```C#
public void Start();
public void Update();
public void LateUpdate();
public static void AddPoseToQueue(Vector3[])
```  

## Class Description  

The `class : BodyPositionManager` class is used for handling the overarching updating and data management of the joint coordinate data. Once a `object : Vector3[]` array of joint coordinates is stored to the `queue : JointCoordinates_Frames` queue by the `class : TCPStreamCoordinateHandler` class, the joint coordinates are then used to update a `object : LimbComponents` object with the respective coordinate data and further use the data to display the patient model to the user.  

## Initialization  

Before we can start using the body position manager, we must first initialize our `object : LimbComponents` instance. This object instance is our container used to store and organize the coordinate data so that it can be easily recalled later in the update process. To initialize our LimbComponents object we first need to call the `TCPStreamCoordinateHandler : method : InitJointCoordiantes()` method which stores a reference to the transform position of the `GameObject : BodyAlignmentPosition` game object in the unity scene and creates a joint coordinate frame with the initializing patient model pose. This coordinate frame can then be used to initialize the LimbComponents object.  

Once the initialization has been completed the `LimbComponents : method : AlignLimbObjects()` method is called to update the associated limb game objects positions in the unity scene, displaying the initializing pose to the user.  

## Frame Updates  

Two processes are occuring in two seperate update functions to help the processing and displaying of the updated patient model poses:  

### Update()  

The `Unity : method : Update()` method is used purely for capturing the transform position of the HoloLens 2 device in the unity scene during runtime of the application. This transformation position is used in the `TCPStreamCoordinateHandler : method : TrackSpatialMovement()` method to attach the translational movement of the user to the patient model in the scene.  

The update method also captures the Z position value of the HoloLens 2 device. This value is used as a scaling parameter for determining whether the joint coordinates read from the input stream should be scaled up or scaled down relative to how close or far the user has moved from the mirror. This scale factor is used in the `TCPStreamCoordinateHandler : method : TCPDataReadHandler()` method.  

### LateUpdate()  

The `Unity : method : LateUpdate()` method is used when we want the following method to be the last one to run during a frame execution. It is in this method that we perform the task of updating the `object : LimbComponents` object instance with the new coordinates from the `queue : JointCoordinates_Frames`. Once the LimbComponents instance is updated with the new set of coordinates, the `LimbComponents : method : AlignLimbObjects()` method is called to update the limb game objects and display the new pose to the user.