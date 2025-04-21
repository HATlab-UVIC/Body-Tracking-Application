# Body Tracking Application Documentation 

## Table of Contents  

|Document|Folder|Description|
|--------|------|-----------|  
|Application_Execution.pdf|2 \| Application|* How to run the application on the HoloLens/PC <br> * File/Folder locations and how to find them <br> * Notes on error/issues when running or building the app|
|Unity_Editor.pdf||* Opening project in Unity Editor <br> * Body tracking Unity project components breakdown <br> * How to build the application in Unity|  

|Document|Folder|Description|
|--------|------|-----------| 
|HLCameraCalibration.pdf|1 \| Code -> <br> 2 \| Python Modules|* module : HLCameraCalibration <br> * module : ImageRectification <br> * module : Triangulation|
|TCP_Client_Server_Py.pdf||* module : TCPServer <br> - Receiving data sent from the HoloLens <br> * module : TCPClient <br> - Sending joint coordinate data to the HoloLens|  

|Document|Folder|Description|
|--------|------|-----------| 
|BodyPositionManager.pdf|1 \| Code -> <br> 1 \| C# Classes|* class : BodyPositionManager <br> * Managing the data received from the TCP stream <br> * Displaying the pose coordinate data|
|LimbComponents.pdf||* class : LimbComponents <br> - Container for storing joint coordinate data <br> * struct : LimbStruct <br> * class : LimbAssignmentKey <br> - Dictionary for faster coordinate processing|
|StereoCameraStream.pdf||* class : StereoCameraStream <br> * Spatial camera parameters <br> * Initializing cameras <br> * Capturing images from the headset <br> * Links to resources|
|TCP_Client_Server.pdf||* class : TCPServer <br> - Receiving data sent from the PC <br> * class : TCPClient <br> - Sending encoded images to the PC|
|TCPStreamCoordinateHandler.pdf||* class : TCPStreamCoordinateHandler <br> * Converting coordinates from string to float <br> * Positioning the patient game object|
|Other_Classes.pdf||* class : PoseManager <br> * class : BodyAlignmentUpdater <br> * class : TCPReconnectionHandler <br> * Voice Commands List|
