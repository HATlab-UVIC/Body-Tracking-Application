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
    void Start()
    {
        Console.WriteLine("Starting Program...");

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
        if (!TCPClient.tcp_connected) return;

        // send image frame over TCP to computer.
        // do this every frame if TCP i connected //Note: may move this to FrameSampleAcquired handler
        SendSingleFrameAsync();
    }


    private void OnDestroy()
    {
        if (_videoCapture != null)
        {
            _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            _videoCapture.Dispose();
        }
    }


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

        // memory management
        sample.Dispose();
    }



}
