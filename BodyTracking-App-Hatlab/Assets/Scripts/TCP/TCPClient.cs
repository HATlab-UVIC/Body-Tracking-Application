using System;
using System.Net.Sockets;
//using System.Net.Sockets;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

/*
Summary: 
TCP Client, once connected to the remote TCP Server, is used to transmit the images
captured on the HoloLens to a computer for processing.
*/
public class TCPClient : MonoBehaviour
{
    [SerializeField] // data fields in Unity editor
    string remoteHost_IP_address, remote_connection_port;

    public static bool tcp_client_connected { get; private set; } = false;
    public static bool client_sending_image_bytes { get; private set; } = false;

#if WINDOWS_UWP
    // objects for data communication over TCP
    StreamSocket _dataStreamSocket = null;
    public DataWriter _dataWriter;
    bool _lastMessageSent = true;
#endif


    /*
    Summary:
    When the application state is opened and closed while running, the TCP
    connection is either restarted or stopped.
    */
    public void OnApplicationFocus(bool appInUse)
    {
        if (!appInUse) stop_tcp_client_connection(0);
        else if (appInUse && !tcp_client_connected) start_tcp_client_connection();
    }


    /*
    Summary:
    On final close of the application, send shutdown message to remote TCP Server
    to stop execution of back-end.
    */
    public void OnApplicationQuit()
    {
        stop_tcp_client_connection(1);
    }


    /*
    Summary:
    Performs the setup tasks to establish a connection between the local TCP Client
    and the remote TCP Server on a computer.
    */
    public async void start_tcp_client_connection()
    {
#if WINDOWS_UWP
        if (_dataStreamSocket != null) _dataStreamSocket.Dispose();

        try
        {
            _dataStreamSocket = new StreamSocket();
            var _hostName = new Windows.Networking.HostName(remoteHost_IP_address);

            // establish a connection with the remote server
            await _dataStreamSocket.ConnectAsync(_hostName, remote_connection_port);
            
            // initializing the TCP Client data writer
            _dataWriter = new DataWriter(_dataStreamSocket.OutputStream);

            tcp_client_connected = true;
        } 
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
            UnityDebug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }
#endif
    }


    /*
    Summary:
    Tear down the connection to the remote TCP Server. If the app is being shut down,
    send a shutdown message to stop the operation of the remote TCP Server.
    */
    private async void stop_tcp_client_connection(int closed_state)
    {
#if WINDOWS_UWP
        if (closed_state == 1)
        {
            try
            {
                // send stop operation message to TCP Server
                _dataWriter.WriteString("e");
                await _dataWriter.StoreAsync();
                await _dataWriter.FlushAsync();
            }
            catch (Exception e)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
                UnityDebug.Log("Local TCP Client :: ERROR :: Error sending PV image to remote TCP server.\n" + e.Message);
                UnityDebug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
            }
        }

        // close TCP Server connection
        _dataWriter?.DetachStream();
        _dataWriter?.Dispose();
        _dataWriter = null;

        _dataStreamSocket?.Dispose();
        tcp_client_connected = false;
#endif
    }


    /*
    Summary:
    Async method for sending the image byte data for images from the front
    Photo/Video camera on the HoloLens. Image bytes are sent via the local
    TCP Client and received by the remote TCP Server. (PV = Photo/Video)

    Parameters:
    byte[] >> The array of image bytes that define the captured image frame
    */
    
    public async void SendPVImageAsync(byte[] image_data)
    {
#if WINDOWS_UWP
        if (!_lastMessageSent) return;
        _lastMessageSent = false;

        try
        {
            client_sending_image_bytes = true;

            // writes the header "v" to stream to specify type of data being passed.
            // this is used for decoding on the computer side of the process.
            // [1] byte -> single char is 1 byte of data
            _dataWriter.WriteString("v");

            // write byte length of image to stream.
            // [4] bytes -> Int32 is 4 bytes of data
            _dataWriter.WriteInt32(image_data.Length);

            // write image byte data to stream.
            // [image_data.Length] bytes -> writes the number of bytes as specified
            //                              by image_data.Length (# of bytes which
            //                              represent the image)
            _dataWriter.WriteBytes(image_data);

            // send image data over TCP
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();   
        }
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
            UnityDebug.Log("Local TCP Client :: ERROR :: Error sending PV image to remote TCP server.\n" + e.Message);
            UnityDebug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }
        client_sending_image_bytes = false;
        _lastMessageSent = true;
#endif
    }


    /*
    Summary:
    Async method for sending the image byte data for images from the front
    left and right spatial cameras on the HoloLens. Image bytes are sent 
    via the local TCP Client and received by the remote TCP Server.

    Parameters:
    byte[] >> The array of combined image bytes that define the captured image frames
    long >> The timestamp of the left image
    long >> The timestamp of the right image
    */
    public async void SendSpatialImageAsync(byte[] LRFImage, long ts_left, long ts_right)
    {
#if WINDOWS_UWP
        if (!_lastMessageSent) return;
        _lastMessageSent = false;
        client_sending_image_bytes = true;
        try
        {
            // Write single byte header (1 byte)
            _dataWriter.WriteString("f");

            // Write Timestamp and Length
            UnityDebug.Log("Local TCP CLient :: SendSpatialImageAsync() :: LRI length: " + LRFImage.Length);
            _dataWriter.WriteInt32(LRFImage.Length); // (4 bytes)
            _dataWriter.WriteInt64(ts_left); // (8 bytes)
            _dataWriter.WriteInt64(ts_right); // (8 bytes)

            // Write image byte data (LRFImage.Length # bytes)
            _dataWriter.WriteBytes(LRFImage);

            // Send data to remote TCP Server
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();

        }
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
            UnityDebug.Log("Local TCP Client :: ERROR :: Error sending spatial image to remote TCP server.\n" + e.Message);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }
        _lastMessageSent = true;
        client_sending_image_bytes = false;
#endif
    }


}