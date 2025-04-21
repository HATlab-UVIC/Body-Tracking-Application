# miscellaneous Classes  

1. Class : PoseManager  
2. Class : BodyAlignmentUpdater  
3. Class : TCPReconnectionHandler  
4. Voice Command List  

# 1. Class : PoseManager  

## Method List  

```C#
public void Start();
public void UpdatePoseComponents(LimbStruct[]);
public void InitPoseLimbs(GameObject);
public void CapturePose();
public void TurnOffPose();
```  

## Class Description  

The `class : PoseManager` class is used as a reference pose capture component. It is used in conjunction with voice commands in the body tracking scene to capture and display a static pose to the user. Both the `method : UpdatePoseComponents()` and `method : InitPoseLimbs()` work equivalently to their `class : LimbComponents` counterpart (*see LimbComponents doc for functionality clarification*).  

While the body tracking scene is running, the voice commands `command : "Pose"` and `command : "Pose Off"` can be said to trigger `class : PoseManager` functionality.  

|Command|Description|
|-------|-----------|
|"**Pose**"|the "pose" command is used to capture a single joint coordinate frame at the time of the command and display the static captured pose to the user in a transparent blue game object material. Each time the "pose" command is said, a new still patient pose is captured and displayed to the user.|
|"**Pose Off**"|If the user does not want to display the captured pose, the command "pose off" will deactivate the game objects associated to the pose, hiding/removing the displayed pose from the scene.|  

# 2. Class : BodyalignmentUpdater  

## Method List  

```C#
public void MoveBodyAlignment(int);
```  

## Class Description  

The `class : BodyAlignmentUpdater` class is used for the modification of the `TCPStreamCoordinateHandler : variable : BodyAlignment_Position` variable during runtime of the application. In the case that the patient model is not displayed directly ontop of the reflection of the user, the `method : MoveBodyAlignment()` method can be used in conjuction with the voice commands `commands : "Up", "Down", "Left", "Right", "In", "Out"` to move the scene position with which the patient model is displayed. This allows the user to move the patient model to better overlay on their reflection.  

# 3. Class : TCPReconnectionHandler  

## Method List  

```C#
void Start();
public void ConnectToRemoteServer();
```  

## Class Description  

The `class : TCPReconnectionHandler` class is used for getting a reference to the `class : TCPClient` class from within the body tracking scene so that the `TCPClient : method : start_tcp_client_connection()` can be accessed and called.  

If the remote TCP Server ever encounters an disconnects or encounters an error, we can use the voice command `command : "Connect Server"` to call the `TCPClient : method : start_tcp_client_connection()` method and re-establish a connection to the remote server without needing to restart the unity application.  

# 4. Voice Command List  

|Command|Description|
|-------|-----------|
|"**Connect Server**"|Reconnects the local TCP Client with the remote TCP Server if a connection is ever lost.|
|"**Pose**"|Captures a static patient model pose at the time of the command.|
|"**Pose Off**"|Turns off/removes the displayed static patient model pose.|
|"**Up**"|Moves the patient model render position up.|
|"**Down**"|Moves the patient model render position down.|
|"**Left**"|Moves the patient model render position left.|
|"**Right**"|Moves the patient model render position right.|
|"**In**"|Moves the patient model render position in.|
|"**Out**"|Moves the patient model render position out.|
