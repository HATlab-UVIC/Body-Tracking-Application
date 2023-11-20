using System;
//using System.Net.Sockets;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

// Use for the transmission of data from the HoloLens to the computer
public class TCPClient : MonoBehaviour
{
    // data fields in Unity editor
    [SerializeField]
    string remoteHost_IP_address, remote_connection_port;

    // TCP connection status variable
    public static bool tcp_client_connected { get; private set; } = false;
    public static bool client_sending_image_bytes { get; private set; } = false;


    // when the application goes into the background (not being used)
    // stop the TCP client connection
    private static bool startup_check = true;
    public void OnApplicationFocus(bool appInUse)
    {
        // auto connect/disconnect to TCP on app focus
#if WINDOWS_UWP
        if (!appInUse) stop_tcp_client_connection();
        else if (appInUse && !tcp_client_connected) start_tcp_client_connection();
#endif
    }


    public void OnApplicationQuit()
    {
#if WINDOWS_UWP
        stop_tcp_client_connection();
#endif
    }


#if WINDOWS_UWP

    // defining necessary variables for data communication over TCP
    StreamSocket _dataStreamSocket = null;
    public DataWriter _dataWriter;
    public DataReader _dataReader;


    // initializes/starts the connection between the local client and remote server
    public async void start_tcp_client_connection()
    {
        if (_dataStreamSocket != null) _dataStreamSocket.Dispose();

        try
        {
            UnityDebug.Log("Local TCP Client :: Attempting to connect to remote TCP Server...");
            _dataStreamSocket = new StreamSocket();
            var _hostName = new Windows.Networking.HostName(remoteHost_IP_address);
            UnityDebug.Log("Remote Host IP :: (" + _hostName + ") Remote Connection Port :: (" + remote_connection_port + ")");

            // establish an asynchronous connection with a remote server
                //UnityDebug.Log("Local TCP Client :: pre-ConnectAsync()");
            await _dataStreamSocket.ConnectAsync(_hostName, remote_connection_port);
                //UnityDebug.Log("Local TCP Client :: ConnectAsync() Completed");
            
            // initializing the read and write objects for TCP
            _dataWriter = new DataWriter(_dataStreamSocket.OutputStream);
            _dataReader = new DataReader(_dataStreamSocket.InputStream);

            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            tcp_client_connected = true;
                //UnityDebug.Log("Local TCP Client :: Connected to remote TCP Server.");
        } 
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
            UnityDebug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }
    }


    // tears down the connection between the local client and remote server
    private void stop_tcp_client_connection()
    {
        _dataWriter?.DetachStream();
        _dataWriter?.Dispose();
        _dataWriter = null;

        _dataReader?.DetachStream();
        _dataReader.Dispose();
        _dataReader = null;

        _dataStreamSocket?.Dispose();
        tcp_client_connected = false;
        UnityDebug.Log("Local TCP Client :: Disconnected from TCP Server.");
    }


    // async process for sending image from HoloLens to computer over TCP.
    // PV -> photo-video | LRF -> laser range finder ?? maybe
    bool _lastMessageSent = true;
    public async void SendPVImageAsync(byte[] image_data)
    {
        if (!_lastMessageSent) return;
        _lastMessageSent = false;

        try
        {
            UnityDebug.Log("Local TCP Client :: SendPHImageAsync() :: Writing data for TCP message.");
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

                //UnityDebug.Log("Local TCP Client :: SendPHImageAsync() :: TCP message data...\n" + "Header: v\n" + "Data Length: " + image_data.Length);

            // send image data over TCP
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();

            client_sending_image_bytes = false;
                //UnityDebug.Log("Local TCP Client :: Image Data Sent.");
        }
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
            UnityDebug.Log("Local TCP Client :: ERROR :: Error sending PV image to remote TCP server.\n" + e.Message);
            UnityDebug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }

        _lastMessageSent = true;
    }


    public async void SendSpatialImageAsync(byte[] LRFImage, long ts_left, long ts_right)
    {
        if (!_lastMessageSent) return;
        _lastMessageSent = false;
        try
        {
            UnityDebug.Log("Local TCP Client :: SendSpatialImageAsync() :: Writing spatial camera data for TCP message.");
            client_sending_image_bytes = true;

            // Write header
            _dataWriter.WriteString("f"); // header "f"

            // Write Timestamp and Length
            _dataWriter.WriteInt32(LRFImage.Length);
            _dataWriter.WriteInt64(ts_left);
            _dataWriter.WriteInt64(ts_right);

            // Write actual data
            _dataWriter.WriteBytes(LRFImage);

            // Send out
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();

            client_sending_image_bytes = false;
        }
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
            UnityDebug.Log("Local TCP Client :: ERROR :: Error sending spatial image to remote TCP server.\n" + e.Message);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }
        _lastMessageSent = true;
    }

#endif
}