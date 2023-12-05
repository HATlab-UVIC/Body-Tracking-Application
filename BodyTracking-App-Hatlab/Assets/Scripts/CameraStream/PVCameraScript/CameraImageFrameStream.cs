using UnityEngine;
using System;
using UnityEngine.UI;
using HoloLensCameraStream;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using bug = System.Diagnostics.Debug;
using UnityDebug = UnityEngine.Debug;


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
    public RawImage _depthImageRaw;
    public Image _depthImageTexture;
    HoloLensCameraStream.Resolution _resolution;

    byte[] LRFImage;
    long ts_unix_left;
    long ts_unix_right;

    // External class object definitions
    VideoCapture _videoCapture;
    TCPClient tcp_client;
    TCPServer tcp_server;
    private bool _videoModeStarted = false;
    private bool _firstFrameCaptured = true;

    IntPtr _spatialCoordinateSystem_Ptr;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif

    public CameraImageFrameStream() {}

    // --------------------------------------
    // Initialize components on program start
    // --------------------------------------
    Queue<Action> _mainThreadActions;
    public void Start()
    {
        UnityDebug.Log("CameraImageFrameStream :: Starting Camera Stream...");

        // initializing the 3D depth sensor
#if ENABLE_WINMD_SUPPORT
        UnityDebug.Log("CameraImageFrameStream :: HL2ResearchMode Setup...");

        try
        {
            researchMode = new HL2ResearchMode();
            UnityDebug.Log("CameraImageFrameStream :: New instance of research mode created.");

            researchMode.InitializeSpatialCamerasFront();
            researchMode.StartSpatialCamerasFrontLoop();

            UnityDebug.Log("CameraImageFrameStream :: Research Mode Camera Extrinsics.\n\n" + researchMode.PrintLFExtrinsics() + "\n\n" + researchMode.PrintRFExtrinsics());

        } 
        catch (Exception e)
        {
            UnityDebug.Log("CameraImageFrameStream :: ERROR :: Error initializing research mode.\n" + e.Message);
        }
        

        /*researchMode.InitializeLongDepthSensor();
        researchMode.SetPointCloudDepthOffset(0);
        researchMode.StartLongDepthSensorLoop(true);*/

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
        researchMode.SetReferenceCoordinateSystem(_spatialCoordinateSystem);
        UnityDebug.Log("CameraImageFrameStream :: HL2ResearchMode Setup Completed.\n");
#endif
#endif

        // get the TCP functionality from client and server scripts
        tcp_client = gameObject.GetComponent<TCPClient>();
        tcp_server = gameObject.GetComponent<TCPServer>();

        _mainThreadActions = new Queue<Action>();

        // TODO: may move this to a voice command later
#if WINDOWS_UWP
            UnityDebug.Log("CameraImageFrameStream :: Starting TCP Client Connection...");
            tcp_client.start_tcp_client_connection();
#endif

        // initializing the coordinate system reference
#if WINDOWS_UWP && XR_PLUGIN_OPENXR
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

    public void ConnectToRemoteTCPServer()
    {
            UnityDebug.Log("CameraImageFrameStream :: Starting TCP Client Connection...");
            tcp_client.start_tcp_client_connection();
    }


    public void Update()
    {
        // Handle the camera transform action updates stored in the queue
        /*lock (_mainThreadActions)
        {
            while (_mainThreadActions.Count > 0)
            {
                _mainThreadActions.Dequeue().Invoke();
            }
        }*/

            //UnityDebug.Log("CameraImageFrameStream :: Update Loop :: TCP Client (" + TCPClient.tcp_client_connected.ToString() + ") Video Mode (" + _videoModeStarted.ToString() + ")");
        if (!TCPClient.tcp_client_connected || !_videoModeStarted) return;

        // send image frame over TCP to computer.
        // do this every frame if TCP is connected 
        if (!_firstFrameCaptured) SendSingleFrameAsync();
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
    public void OnDestroy()
    {
        if (_videoCapture != null)
        {
            _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            _videoCapture.Dispose();
        }
    }


    // a local method reference for sending the image byte data over TCP
    public void SendSingleFrameAsync()
    {
#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
                //UnityDebug.Log("CameraImageFrameStream :: Sending Image Frame...");
            //tcp_client.SendPVImageAsync(_latestImageBytes);
            if (LRFImage != null) tcp_client.SendSpatialImageAsync(LRFImage, ts_unix_left, ts_unix_right);
#endif
    }


    // initializes the necessary components to start video mode
    // - Callback method -
    void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            UnityDebug.Log("CameraImageFrameStream :: ERROR :: Did not find VideoCapture object...");
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
            UnityDebug.Log("CameraImageFrameStream :: ERROR :: Could not start video mode...");
            return;
        }

        UnityDebug.Log("CameraImageFrameStream :: Video Mode Started!");
        _videoModeStarted = true;
    }


    // when the VideoCapture:FrameSampleAcquired event is triggered, this method
    // handles the event and stores the image frame for further use
    // - Event handler method -
    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        // Limits the number of actions that can be added to the queue.
        // TODO: not sure if this is necessary to be done in a queue
        /*lock (_mainThreadActions)
        {
            if (_mainThreadActions.Count > 2)
            {
                sample.Dispose();
                return;
            }
        }*/

        // Dont copy image data into buffer if TCP Client is trying to send data
        if (!TCPClient.client_sending_image_bytes)
        {
            // if byte array is null or not big enough to hold all the image bytes then
            // define a new array that is large enough, otherwise, use the existing array
            if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
            {
                _latestImageBytes = new byte[sample.dataLength];
            }

            // save the frame image to _latestImageBytes
            if (_firstFrameCaptured) _firstFrameCaptured = false;

                //UnityDebug.Log("CameraImageFrameStream :: Frame Sample Acquired :: Saving frame image... \nImage bytes: " + _latestImageBytes.Length.ToString());
            //sample.CopyRawImageDataIntoBuffer(_latestImageBytes);
            SaveSpatialImageEvent();


        }
        

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

        /*Enqueue(() =>
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
        });*/

        // memory management
        sample.Dispose();
    }


    public void SaveSpatialImageEvent()
    {
        UnityDebug.Log("CameraImageframeStream :: Front Spatial Cameras :: Saving Left/Right front camera image buffers...");
#if ENABLE_WINMD_SUPPORT
#if WINDOWS_UWP
        long ts_ft_left, ts_ft_right;

        try
        {
            LRFImage = researchMode.GetLRFCameraBuffer(out ts_ft_left, out ts_ft_right);

            UnityDebug.Log("CameraImageFrameStream :: Front Spatial Cameras :: Image buffer length\n\nBytes: "+ LRFImage.Length);

            Windows.Perception.PerceptionTimestamp ts_left = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(DateTime.FromFileTime(ts_ft_left));
            Windows.Perception.PerceptionTimestamp ts_right = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(DateTime.FromFileTime(ts_ft_right));

            ts_unix_left = ts_left.TargetTime.ToUnixTimeMilliseconds();
            ts_unix_right = ts_right.TargetTime.ToUnixTimeMilliseconds();
        } catch (Exception e) { UnityDebug.Log("CameraImageFrameStream :: Error :: GetLRFCameraBuffer() issue with method"); }

        
#endif
#endif
        UnityDebug.Log("CameraImageframeStream :: Front Spatial Cameras :: Left/Right image buffer saved.");
    }
}


// TODO: looks like fixedupdate and savepointcloud are only being used in a debug context
/*private int counter = 0;
public void FixedUpdate()
{
    if (counter % 40 == 0)
    {
        SavePointCloudPLY();
    }
    counter++;
}*/

/*public void SavePointCloudPLY()
    {
#if ENABLE_WINMD_SUPPORT
        var longpointCloud = researchMode.GetLongThrowPointCloudBuffer();
        var longpointMap = researchMode.GetLongDepthMapTextureBuffer();
        // DebugText.LOG(longpointMap[5040].ToString() + ", " + longpointMap[4440].ToString()+ ", " + longpointMap[4740].ToString());
#endif
    }


    private static float[] jointDepth_z = new float[25];
    private static readonly int[] placement_offsets = {0,1,-1,320,-320};
    public float[] Apply_DepthPositionFromSensor(Vector3[] jointCoordinateVectors)
    {
        if (jointCoordinateVectors == null)
        {
            UnityDebug.Log("CameraImageFrameStream :: ERROR :: Error with joint coordinates.");
            return jointDepth_z;
        }
        UnityDebug.Log("CameraImageFrameStream :: Getting depth map texture buffer...");
#if ENABLE_WINMD_SUPPORT

        try
        {
            Byte[] depthMapTextureBuffer = researchMode.GetLongDepthMapTextureBuffer();

            /*Texture2D depthMapImageTexture = new Texture2D(256, 256);

            ImageConversion.LoadImage(depthMapImageTexture, depthMapTextureBuffer);

            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Rect spriteRect = new Rect(0, 0, depthMapImageTexture.width, depthMapImageTexture.height);
            _depthImageTexture.overrideSprite = Sprite.Create(depthMapImageTexture, spriteRect, pivot);

            _depthImageRaw.texture = depthMapImageTexture;

// TODO: code above may be unnecessary (except for depthMapTextureBuffer)
// appears to only be used for displaying depth map, not needed for other purposes.

UnityDebug.Log("CameraImageFrameStream :: Calculating joint depth values...");
float x, y;
float depthValue;
int placement;
for (int i = 0; i < jointCoordinateVectors.Length; i++)
{
    x = jointCoordinateVectors[i].x + 30.0f;
    y = jointCoordinateVectors[i].y + 10.0f;

    // possibly calculating the index offset which correlates 2D image pixel data to depth buffer
    placement = (int)(y * 320 + x);
    depthValue = 0.0f;

    // calculate the depth value
    foreach (int offset in placement_offsets)
    {
        depthValue += -float.Parse(depthMapTextureBuffer[placement + offset].ToString()) / 80;
    }

    depthValue /= placement_offsets.Length;
    jointDepth_z[i] = depthValue + 9.0f;
}

UnityDebug.Log("CameraImageFrameStream :: Depths calculated.");
return jointDepth_z;
        }
        catch (Exception e)
        {
    UnityDebug.Log("CameraImageFrameStream :: ERROR :: Error getting long depth map texture buffer.");
    return jointDepth_z;
}

#endif
return jointDepth_z;
    }*/
