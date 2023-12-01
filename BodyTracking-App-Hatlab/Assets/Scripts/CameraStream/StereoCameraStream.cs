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

public class StereoCameraStream : MonoBehaviour
{
    TCPClient tcp_client;
    TCPServer tcp_server;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif

    private Queue<byte[]> SpatialImageFrames;
    void Start()
    {
        UnityDebug.Log("StereoCameraStream :: Starting Camera Stream...");

        try
        {
#if ENABLE_WINMD_SUPPORT
            UnityDebug.Log("StereoCameraStream :: Initializing HL2ResearchMode Plugin...");
            researchMode = new HL2ResearchMode();

            researchMode.InitializeSpatialCamerasFront();
            researchMode.StartSpatialCamerasFrontLoop();
#if WINDOWS_UWP && XR_PLUGIN_OPENXR
            researchMode.SetReferenceCoordinateSystem(_spatialCoordinateSystem);
#endif
#endif
            UnityDebug.Log("StereoCameraStream :: HL2ResearchMode Plugin Initialized.");
        }
        catch (Exception e) { UnityDebug.Log("StereoCameraStream :: ERROR :: Error initializing HL2ResearchMode Plugin.\n"+e); }

        tcp_client = gameObject.GetComponent<TCPClient>();
        tcp_server = gameObject.GetComponent<TCPServer>();

        UnityDebug.Log("StereoCameraStream :: Starting TCP Client Connection...");
        tcp_client.start_tcp_client_connection();

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
        _spatialCoordinateSystem = Microsoft.MixedReality.OpenXR.PerceptionInterop.GetSceneCoordinateSystem(UnityEngine.Pose.identity) as SpatialCoordinateSystem;
#endif

        SpatialImageFrames = new Queue<byte[]>();

    }


    int garbage_count = 1;
    byte[] sendBytes = null;
    byte[] LRFImage = null;
    long ts_unix_left = 0;
    long ts_unix_right = 0;
    void Update()
    {
        if (garbage_count % 60 == 0)
        {
            garbage_count++;
            GC.Collect();
        }
#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
        SaveSpatialImageEvent();
        if (SpatialImageFrames.Count < 3) SpatialImageFrames.Enqueue(LRFImage);

        if (!TCPClient.tcp_client_connected || TCPClient.client_sending_image_bytes) return;
        UnityDebug.Log("StereoCameraStream :: Update :: TCP image queue >> " + SpatialImageFrames.Count);
        if (SpatialImageFrames.Count > 0)
        {
            sendBytes = SpatialImageFrames.Dequeue();
            if (sendBytes.Length > 0) tcp_client.SendSpatialImageAsync(sendBytes, ts_unix_left, ts_unix_right);
            UnityDebug.Log("StereoCameraStream :: Update :: Image data sent.");
        }

        LRFImage = null;
        sendBytes  = null;
        ts_unix_left = 0;
        ts_unix_right = 0;
#endif
    }


    public void ConnectToRemoteServer()
    {
        UnityDebug.Log("StereoCameraStream :: Starting TCP Client Connection...");
        tcp_client.start_tcp_client_connection();
    }


    public void SendCameraFrameAsync(byte[] Image, long ts_left, long ts_right)
    {
        tcp_client.SendSpatialImageAsync(Image, ts_left, ts_right);
    }


#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
    public void SaveSpatialImageEvent()
    {
        UnityDebug.Log("StereoCameraStream :: Front Spatial Cameras :: Saving Left/Right front camera image buffers...");

        long ts_ft_left = 0;
        long ts_ft_right = 0;

        try 
        { 
            //LRFImage = researchMode.GetLRFCameraBuffer(out ts_ft_left, out ts_ft_right); // MEMORY LEAK ISSUE
            byte[] leftImage = researchMode.GetLFCameraBuffer(out ts_ft_left);
            byte[] rightImage = researchMode.GetRFCameraBuffer(out ts_ft_right);
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
            UnityDebug.Log("StereoCameraStream :: Error :: GetLRFCameraBuffer() issue with method\n" + e); 
            LRFImage = new byte[0];
            ts_unix_left = 0;
            ts_unix_right = 0;
        }

        UnityDebug.Log("StereoCameraStream :: Front Spatial Cameras :: Left/Right image buffer saved.");
    }
#endif
}
