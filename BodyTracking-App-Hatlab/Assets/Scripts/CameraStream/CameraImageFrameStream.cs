using UnityEngine;
using System;
using UnityEngine.UI;
using HoloLensCameraStream;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
using Windows.Perception.Spatial;
#endif

#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
using UnityEngine.Windows;
#endif

/// </summary>
/// **Add Define Symbols:**
/// Open **File > Build Settings > Player Settings > Other Settings** and add the following to `Scripting Define Symbols` depending on the XR system used in your project;
/// - Legacy built-in XR: `BUILTIN_XR`';
/// - XR Plugin Management (Windows Mixed Reality): `XR_PLUGIN_WINDOWSMR`;
/// - XR Plugin Management (OpenXR):`XR_PLUGIN_OPENXR`.
/// </summary>
public class CameraImageFrameStream : MonoBehaviour
{
    // Image variable definitions
    byte[] _latestImageBytes;
    public RawImage _depthImageDisplayed;
    public Image _depthImageTexture;
    HoloLensCameraStream.Resolution _resolution;

    // External class object definitions
    VideoCapture _videoCapture;
    TCPClient tcp_client;
    TCPServer tcp_server;

    IntPtr _spatialCoordiateSystem_Ptr;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode _HL2ResearchMode;
#endif


    // --------------------------------------
    // Initialize components on program start
    // --------------------------------------
    Queue<Action> _mainThreadActions;
    void Start()
    {
        Debug.Log("Starting Program...");

        // initializing the 3D depth sensor
#if ENABLE_WINMD_SUPPORT
        _HL2ResearchMode = new HL2ResearchMode();
        _HL2ResearchMode.InitializeLongDepthSensor();

        _HL2ResearchMode.SetPointCloudDepthOffset(0);
        _HL2ResearchMode.StartLongDepthSensorLoop(true);

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
        _HL2ResearchMode.SetReferenceCoordinateSystem(_spatialCoordinateSystem);
#endif
#endif

        // get the TCP functionality from client and server scripts
        tcp_client = gameObject.GetComponent<TCPClient>();
        tcp_server = gameObject.GetComponent<TCPServer>();

        _mainThreadActions = new Queue<Action>();

        // TODO: may move this to a voice command later
#if WINDOWS_UWP
        tcp_client.start_tcp_client_connection();
#endif

        // initializing the coordinate system reference
#if WINDOWS_UWP && XR_PLUGIN_WINDOWSMR
        _spatialCoordinateSystem_Ptr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;

#elif WINDOWS_UWP && XR_PLUGIN_OPENXR
        _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;

#elif WINDOWS_UWP && BUILTIN_XR
#if UNITY_2017_2_OR_NEWER
        _spatialCoordinateSystem_Ptr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

#else
        _spatialCoordinateSystem_Ptr = UnityEngine.VR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

#endif
#endif
        // starts the initialization process for the camera
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
    }


    private void Update()
    {
        // Handle the camera transform action updates stored in the queue
        lock (_mainThreadActions)
        {
            while (_mainThreadActions.Count > 0)
            {
                _mainThreadActions.Dequeue().Invoke();
            }
        }

        if (!TCPClient.tcp_client_connected) return;

        // send image frame over TCP to computer.
        // do this every frame if TCP is connected //Note: may move this to FrameSampleAcquired handler
        SendSingleFrameAsync();
    }


    // used as a local helper method the add an action to our main thread queue
    private void Enqueue(Action action)
    {
        lock(_mainThreadActions)
        {
            _mainThreadActions.Enqueue(action);
        }
    }


    // on shutdown of the application, perform teardown of attached subscriber method and
    // dispose of the video capture object
    private void OnDestroy()
    {
        if (_videoCapture != null)
        {
            _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            _videoCapture.Dispose();
        }
    }

    // TODO: looks like fixedupdate and savepointcloud are only being used in a debug context
    public int counter = 0;
    private void FixedUpdate()
    {
        if (counter % 40 == 0)
        {
            SavePointCloudPLY();
        }
        counter++;
    }


    public void SavePointCloudPLY()
    {
#if ENABLE_WINMD_SUPPORT
        var longpointCloud = researchMode.GetLongThrowPointCloudBuffer();
        var longpointMap = researchMode.GetLongDepthMapTextureBuffer();
        DebugText.LOG(longpointMap[5040].ToString() + ", " + longpointMap[4440].ToString()+ ", " + longpointMap[4740].ToString());
#endif
    }


    // a local method reference for sending the image byte data over TCP
    public void SendSingleFrameAsync()
    {
#if ENABLE_WINMD__SUPPORT && WINDOWS_UWP
    tcp_client.SendPVImageAsync(_latestImageBytes);
#endif
    }


    // initializes the necessary components to start video mode
    // - Callback method -
    void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            Console.WriteLine("ERROR: Did not fund VideoCapture object...");
            return;
        }

        this._videoCapture = videoCapture;

        // requestion spacial coordinate information and storing to defined variables
#if WINDOWS_UWP && XR_PLUGIN_OPENXR
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystem(_spatialCoordinateSystem);

#elif WINDOWS_UWP && (XR_PLUGIN_WINDOWSMR || BUILTIN_XR)
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(_spatialCoordinateSystem_Ptr);
#endif

        // setup of camera parameters for video capture
        _resolution = CameraStreamHelper.Instance.GetLowestResolution();
        float _frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(_resolution);

        CameraParameters _cameraParameters = new CameraParameters();
        _cameraParameters.cameraResolutionHeight = _resolution.height;
        _cameraParameters.cameraResolutionWidth = _resolution.width;
        _cameraParameters.frameRate = Mathf.RoundToInt(_frameRate);
        _cameraParameters.pixelFormat = CapturePixelFormat.BGRA32;
        _cameraParameters.rotateImage180Degrees = false;
        _cameraParameters.enableHolograms = false;

        // subscribe the OnFrameSampleAcquired() method to the event
        // class:VideoCapture:FrameSampleAcquired. When the event is 
        // triggered, execute the above method
        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        // start the video mode with the specified camera parameters
        videoCapture.StartVideoModeAsync(_cameraParameters, OnVideoModeStarted);
    }


    // log the status of whether video mode has been started successfully or not
    // - Callback method -
    void OnVideoModeStarted(VideoCaptureResult VCResult)
    {
        if (VCResult.success == false)
        {
            Console.WriteLine("ERROR: Could not start video mode...");
            return;
        }

        Console.WriteLine("Video Mode Started!");
    }


    // when the VideoCapture:FrameSampleAcquired event is triggered, this method
    // handles the event and stores the image frame for further use
    // - Event handler method -
    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        // Limits the number of actions that can be added to the queue.
        // TODO: not sure if this is necessary to be done in a queue
        lock (_mainThreadActions)
        {
            if (_mainThreadActions.Count > 2)
            {
                sample.Dispose();
                return;
            }
        }

        // if byte array is null or not big enough to hold all the image bytes then
        // define a new array that is large enough, otherwise, use the existing array
        if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
        {
            _latestImageBytes = new byte[sample.dataLength];
        }

        // save the frame image to _latestImageBytes
        sample.CopyRawImageDataIntoBuffer(_latestImageBytes);

        // gets the camera to world matrix at the time of frame capture.
        // multiply with your gameObjects local transformation matrix to
        // position correctly in the scene.
        // Note: cameraToWorldMatrix_float is being passed by reference
        if (!sample.TryGetCameraToWorldMatrix(out float[] cameraToWorldMatrix_float)) ; // return
        // convert float[] to matrix that can be used by Unity
        Matrix4x4 _cameraToWorldMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(cameraToWorldMatrix_float);

        // gets the projection matrix at the time of frame capture.
        // use this to convert 2D image coordinates to 3D
        // (not sure how this works...)
        // Note: projectionMatrix_float is being passed by referene
        if (!sample.TryGetProjectionMatrix(out float[] projectionMatrix_float)) ; // return
        Matrix4x4 _projectionMatrix = LocatableCameraUtils.ConvertFloatArrayToMatrix4x4(projectionMatrix_float);

        Enqueue(() =>
        {
#if XR_PLUGIN_WINDOWSMR || XR_PLUGIN_OPENXR
            // Note: This is from the VideoPanelApp file. Not sure the end significance of this yet whether it is needed or not.
            // This is the only use for the Queue as all other functionality in the VideoPanelApp is for updating debug text data.
            //
            // It appears that the Legacy built-in XR environment automatically applies the Holelens Head Pose to Unity camera transforms,
            // but not to the new XR system (XR plugin management) environment.
            // Here the cameraToWorldMatrix is applied to the camera transform as an alternative to Head Pose,
            // so the position of the displayed video panel is significantly misaligned. If you want to apply a more accurate Head Pose, use MRTK.

            Camera unityCamera = Camera.main;
            Matrix4x4 invertZScaleMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));
            Matrix4x4 localToWorldMatrix = _cameraToWorldMatrix * invertZScaleMatrix;
            unityCamera.transform.localPosition = localToWorldMatrix.GetColumn(3);
            unityCamera.transform.localRotation = Quaternion.LookRotation(localToWorldMatrix.GetColumn(2), localToWorldMatrix.GetColumn(1));
#endif
        });

        // memory management
        sample.Dispose();
    }



}
