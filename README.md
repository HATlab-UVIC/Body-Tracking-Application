# HATlab Body Tracking Application  

The following project was done in conjunction with the Health Assistive Technology Lab at the University of Victoria. The background research for this project focused on the problem area of physiotherapy patients not performing their prescribed exercise programs and thus not achieving an optimal recovery from their injury or ailment. Patients don't perform their exercises for a multitude of reasons, for example, lack of motivation, or mental health factors, however, one of the more prominent reasons related to patients having difficulty performing exercises on their own at home. This is something I have also had first hand experience with.

To combat this, we looked to develop a system that leverages Augmented Reality Technology to help provide 3D visualized feedback and guidence overlaid in your environment to better support the independence of patients.

---  
## Documentation Table of Contents  

* [Application](./Documentation/Application/)  
    * [Running Application](./Documentation/Application/Running_Application/Application_Execution.md)  
    * [Unity Editor](./Documentation/Application/Unity_Editor/Unity_Editor.md)  
* [Camera Calibration](./Documentation/Camera_Calibration/Camera-Calibration.md)  
* [Photo-Video Camera Stream](./Documentation/PV_Camera_Stream/CameraStream-FrameCapture.md)  
* [Python Modules](./Documentation/Python_Modules/)  
    * [TCP Client-Server](./Documentation/Python_Modules/TCP_Client_Server_Py.md)  
    * [HL Camera Calibration](./Documentation/Python_Modules/HLCameraCalibration.md)  
* [Unity Classes](./Documentation/Unity_Classes/)  
    * [Body Position Manager](./Documentation/Unity_Classes/BodyPositionManager.md)  
    * [Limb Components](./Documentation/Unity_Classes/LimbComponents.md)  
    * [Stereo Camera Stream](./Documentation/Unity_Classes/StereoCameraStream.md)  
    * [TCP Client-Server](./Documentation/Unity_Classes/TCP_Client_Server.md)  
    * [TCP Stream Coordinate Handler](./Documentation/Unity_Classes/TCPStreamCoordinateHandler.md)  
    * [Other Classes](./Documentation/Unity_Classes/Other_Classes.md)

---

The project itself is built ontop of an [example Unity project (link)](https://github.com/EnoxSoftware/HoloLensCameraStream/tree/master/HoloLensVideoCaptureExample/Assets/CamStream). The projet makes use of a custom plugin `HoloLensCameraStream` which makes the HoloLens video camera frames available to a Unity app in real time.