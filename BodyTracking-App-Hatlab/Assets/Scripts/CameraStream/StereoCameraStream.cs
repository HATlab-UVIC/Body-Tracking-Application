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

    IntPtr WorldOriginPtr;

#if WINDOWS_UWP && XR_PLUGIN_OPENXR
    SpatialCoordinateSystem _spatialCoordinateSystem;
#endif

#if ENABLE_WINMD_SUPPORT
    HL2ResearchMode researchMode;
#endif


    void Start()
    {
        UnityDebug.Log("StereoCameraStream :: Starting Camera Stream...");
        UnityDebug.Log("StereoCameraStream :: Initializing HL2ResearchMode Plugin...");

        try
        {
#if ENABLE_WINMD_SUPPORT
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

    }


    void Update()
    {
        byte[] spatialImageBytes = null;
        long ts_left, ts_right;
#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
        SaveSpatialImageEvent(out spatialImageBytes, out ts_left, out ts_right);

        if (TCPClient.tcp_client_connected && spatialImageBytes != null && spatialImageBytes.Length > 0) 
        {
            SendCameraFrameAsync(spatialImageBytes, ts_left, ts_right);
        }
#endif
    }


    public void ConnectToRemoteServer()
    {
        UnityDebug.Log("CameraImageFrameStream :: Starting TCP Client Connection...");
        tcp_client.start_tcp_client_connection();
    }


    public void SendCameraFrameAsync(byte[] LRFImage, long ts_unix_left, long ts_unix_right)
    {
        tcp_client.SendSpatialImageAsync(LRFImage, ts_unix_left, ts_unix_right);
    }


#if ENABLE_WINMD_SUPPORT && WINDOWS_UWP
    public void SaveSpatialImageEvent(out byte[] LRFImage, out long ts_unix_left, out long ts_unix_right)
    {
        UnityDebug.Log("CameraImageframeStream :: Front Spatial Cameras :: Saving Left/Right front camera image buffers...");

        long ts_ft_left, ts_ft_right;

        try 
        { 
            LRFImage = researchMode.GetLRFCameraBuffer(out ts_ft_left, out ts_ft_right);
            
            Windows.Perception.PerceptionTimestamp ts_left = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(DateTime.FromFileTime(ts_ft_left));
            Windows.Perception.PerceptionTimestamp ts_right = Windows.Perception.PerceptionTimestampHelper.FromHistoricalTargetTime(DateTime.FromFileTime(ts_ft_right));

            ts_unix_left = ts_left.TargetTime.ToUnixTimeMilliseconds();
            ts_unix_right = ts_right.TargetTime.ToUnixTimeMilliseconds();
        } 
        catch (Exception e) 
        { 
            UnityDebug.Log("CameraImageFrameStream :: Error :: GetLRFCameraBuffer() issue with method\n" + e); 
            LRFImage = new byte[0];
            ts_unix_left = 0;
            ts_unix_right = 0;
        }

        UnityDebug.Log("CameraImageFrameStream :: Front Spatial Cameras :: Image buffer length\n\nBytes: "+ LRFImage.Length);
        UnityDebug.Log("CameraImageframeStream :: Front Spatial Cameras :: Left/Right image buffer saved.");
    }
#endif
}
