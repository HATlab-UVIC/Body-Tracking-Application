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

    private Queue<byte[]> SpatialImageFrames;

    /*
    Summary: 
    Initialize the components required for image capture and transmission.
    */
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
#endif
#endif
        }
        catch (Exception e) { UnityDebug.Log("StereoCameraStream :: ERROR :: Error initializing HL2ResearchMode Plugin.\n"+e); }

        // init the TCP Client for data transmission
        tcp_client = gameObject.GetComponent<TCPClient>();
        tcp_client.start_tcp_client_connection();

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
        _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;
#endif
        // init frame sending queue
        SpatialImageFrames = new Queue<byte[]>();

    }


    /*
    Summary: 
    Update is used for capturing spatial image frames and sending the image data over TCP.
    */
#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
    byte[] sendBytes = null;
    byte[] LRFImage = null;
    long ts_unix_left = 0;
    long ts_unix_right = 0;
#endif
    int garbage_count = 1;
    void Update()
    {
        if (garbage_count % 120 == 0) GC.Collect();
        garbage_count++;

#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
        SaveSpatialImageEvent();
        if (SpatialImageFrames.Count < 2) SpatialImageFrames.Enqueue(LRFImage);

        if (!TCPClient.tcp_client_connected || TCPClient.client_sending_image_bytes) return;
        
        // check that there are images to be sent
        if (SpatialImageFrames.Count > 0)
        {
            sendBytes = SpatialImageFrames.Dequeue();
            if (sendBytes.Length > 0) tcp_client.SendSpatialImageAsync(sendBytes, ts_unix_left, ts_unix_right);
        }

        LRFImage = null;
        sendBytes  = null;
        ts_unix_left = 0;
        ts_unix_right = 0;
#endif
    }


    /*
    Summary: 
    The handler method for the voice command 'connect server' to reconnect to the
    remote TCP Server if it closes or disconnects.
    */
    public void ConnectToRemoteServer()
    {
        tcp_client.start_tcp_client_connection();
    }


    /*
    Summary: 
    Method is used to save image frames from the front left and right spatial cameras
    on the HoloLens device.
    */
#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
    public void SaveSpatialImageEvent()
    {
        long ts_ft_left = 0;
        long ts_ft_right = 0;

        try 
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
        } 
        catch (Exception e) 
        { 
            LRFImage = new byte[0];
            ts_unix_left = 0;
            ts_unix_right = 0;
        }
    }
#endif
}