using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;
using UnityEngine.UI;
using HoloLensCameraStream;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using bug = System.Diagnostics.Debug;
using UnityDebug = UnityEngine.Debug;
using System.Net;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
using Windows.Perception.Spatial;
#endif

#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
using UnityEngine.Windows;
#endif

/*
Summary:
Class is used to access and capture image frames acquired from the front left and right 
spatial cameras. The image frames are then sent over TCP to a computer to be processed
by openpose.
*/
public class StereoCameraStream : MonoBehaviour
{
    TCPClient tcp_client;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif


    void Awake() { DontDestroyOnLoad(gameObject); }


    /*
    Summary: 
    Initialize the components required for image capture and transmission.
    */
    private Queue<byte[]> SpatialImageFrames;
    int scene_state;
    void Start()
    {
        try
        {
            // init the HL2ResearchMode plugin for front spatial camera access
#if ENABLE_WINMD_SUPPORT
            researchMode = new HL2ResearchMode();

            researchMode.InitializeSpatialCamerasFront();
            researchMode.StartSpatialCamerasFrontLoop();
#if WINDOWS_UWP && XR_PLUGIN_OPENXR
            researchMode.SetReferenceCoordinateSystem(_spatialCoordinateSystem);
            _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;
#endif
#endif
        }
        catch (Exception e) { UnityDebug.Log("StereoCameraStream :: ERROR :: Error initializing HL2ResearchMode Plugin.\n"+e); }

        // init the TCP Client for data transmission
        tcp_client = gameObject.GetComponent<TCPClient>();
        tcp_client.start_tcp_client_connection();

        // init frame sending queue
        SpatialImageFrames = new Queue<byte[]>();
        scene_state = 0;
        photo_indicator.SetActive(false);

    }


    /*
    Summary: 
    Update is used for capturing spatial image frames and sending the image data over TCP.
    */
    byte[] sendBytes = null;
    byte[] LRFImage = null;
    long ts_unix_left = 0;
    long ts_unix_right = 0;
    int garbage_count = 1;
    void Update()
    {
        // check if we are in the body tracking state
        if (scene_state == 1)
        {
            garbage_count = Garbage(garbage_count, 120);
            BodyTrackingFrameCapture();
            sendBytes = null;
        }

        LRFImage = null;
        ts_unix_left = 0;
        ts_unix_right = 0;
    }


    /*
    Summary: 
    Method is used to save image frames from the front left and right spatial cameras
    on the HoloLens device.
    */
    private bool SaveSpatialImageEvent()
    {
        try 
        {
#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
            long ts_ft_left = 0;
            long ts_ft_right = 0;

            if (researchMode.LFImageUpdated() && researchMode.RFImageUpdated())
            {
                // get the front left camera image buffer
                byte[] leftImage = researchMode.GetLFCameraBuffer(out ts_ft_left);
                // get the fri=ont right camera image buffer
                byte[] rightImage = researchMode.GetRFCameraBuffer(out ts_ft_right);
            
                // combine the images into a single buffer for transmission over TCP
                LRFImage = new byte[leftImage.Length + rightImage.Length];
                Array.Copy(leftImage, 0, LRFImage, 0, leftImage.Length);
                Array.Copy(rightImage, 0, LRFImage, leftImage.Length, rightImage.Length);
            
                Windows.Perception.PerceptionTimestamp ts_left = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(DateTime.FromFileTime(ts_ft_left));
                Windows.Perception.PerceptionTimestamp ts_right = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(DateTime.FromFileTime(ts_ft_right));

                ts_unix_left = ts_left.TargetTime.ToUnixTimeMilliseconds();
                ts_unix_right = ts_right.TargetTime.ToUnixTimeMilliseconds();
                
                return true;
            }
#endif
            return false;
        }
        catch (Exception e) 
        { 
            LRFImage = new byte[0];
            ts_unix_left = 0;
            ts_unix_right = 0;
            return false;
        }
    }


    /*
    summary:
    Method is used in conjunction with 'capture' button in the Unity application for 
    capture and transmission of a single set of stereo calibration images of the chessboard 
    for calibration. A status indicator is displayed in the app while an image is being 
    captured.
    */
    [SerializeField]
    GameObject photo_indicator;
    public async void CalibrationFrameCapture()
    {
        photo_indicator.SetActive(true);

        // delay to get hands out of image frame
        await Task.Delay(2000);
        
        bool image_capture_status = SaveSpatialImageEvent();
        
        if (image_capture_status && TCPClient.tcp_client_connected)
        {
            // 'c' header specifies calibration image to remote TCP Server
            tcp_client.SendSpatialImageAsync("c", LRFImage, ts_unix_left, ts_unix_right);
        }
        photo_indicator.SetActive(false);
    }


    /*
    Summary:
    Method is used to trigger the TCPClient method that sends the status header byte to
    perform the camera calibration for the stereo cameras.
    */
    public void StartCameraCalibration() { if (TCPClient.tcp_client_connected) tcp_client.StartCameraCalibration(); }


    /*
    Summary:
    Method is used for capturing spatial image frames and sending them to the remote
    TCP Server to be processed by Openpose.
    */
    public void BodyTrackingFrameCapture()
    {
        SaveSpatialImageEvent();
        if (SpatialImageFrames.Count < 2) SpatialImageFrames.Enqueue(LRFImage);

        if (!TCPClient.tcp_client_connected || TCPClient.client_sending_image_bytes) return;

        // check that there are images to be sent
        if (SpatialImageFrames.Count > 0)
        {
            sendBytes = SpatialImageFrames.Dequeue();
            if (sendBytes.Length > 0) tcp_client.SendSpatialImageAsync("f", sendBytes, ts_unix_left, ts_unix_right);
        }
    } 


    /*
    Summary:
    Method is used with the 'Body Tracking' button to switch from the calibration
    scene to the body tracking scene. (ie. start the body tracking)
    */
    public void LoadBodyTrackingScene()
    {
        scene_state = 1;
        SceneManager.LoadScene("BodyTrackingScene");
    }


    /*
    Summary:
    Method is used for specifying reoccuring garbage collector calls.
        **NOTE** this may not be needed and can possibly be removed from
        the Update() method.

    Parameters:
    int >> the integer specifying the current frame number
    int >> the interval with which we want to call the GC (every 'i' frames)

    Return:
    int >> the incrimented frame integer
    */
    private int Garbage(int frame, int freq)
    {
        if (frame % freq == 0) GC.Collect();
        return frame++;
    }
}
