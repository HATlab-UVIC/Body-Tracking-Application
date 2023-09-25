using System;
using System.Net.Sockets;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

// Use for the transmission of data from the HoloLens to the computer
public class TCPClient : MonoBehaviour
{
    // data fields in Unity editor
    [SerializeField]
    string HL_host_IP_address, connection_port;

    // TCP connection status variable
    public static bool tcp_connected { get; private set; } = false;


    // when the application goes into the background (not being used)
    // stop the TCP client connection
    private bool startup_check = true;
    private void OnApplicationFocus(bool appInUse)
    {
        // wait for user to say "connect" to start TCP
        if (startup_check)
        {
            startup_check = false;
            return;
        }

        // auto connect/disconnect to TCP on app focus
#if WINDOWS_UWP
        if (!appInUse) stop_tcp_client_connection();
        else if (appInUse && !tcp_connected) start_tcp_client_connection();
#endif
    }


#if WINDOWS_UWP
    
    // defining necessary variables for data communication over TCP
    StreamSocket _dataStreamSocket = null;
    public DataWriter _dataWriter;
    public DataReader _dataReader;


    // initializes/starts the connection between the local client and remote server
    private async void start_tcp_client_connection()
    {
        if (_dataStreamSocket != null) _dataStreamSocket.Dispose();

        try
        {
            _dataStreamSocket = new StreamSocket();
            var _hostName = new Windows.Networking.HostName(HL_host_IP_address);

            // establish an asynchronous connection with a remote server
            await _dataStreamSocket.ConnectAsync(_hostName, connection_port);
            
            // initializing the read and write objects for TCP
            _dataWriter = new DataWriter(_dataStreamSocket.OutputStream);
            _dataReader = new DataReader(_dataStreamSocket.InputStream);

            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            tcp_connected = true;
            Debug.Log("Connected to TCP Server.");
        } 
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
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
        tcp_connected = false;
        Debug.Log("Disconnected from TCP Server.");
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
            // writes the header "v" to stream to specify type of data being passed.
            // this is used for decoding on the computer side of the process
            _dataWriter.WriteString("v");

            // write timestamp and byte length of image to stream
            _dataWriter.WriteInt32(image_data.Length);

            // write image byte data to stream
            _dataWriter.WriteBytes(image_data);

            // send image data over TCP
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();
        }
        catch (Exception e)
        {
            SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : e.Message);
        }

        _lastMessageSent = true;
    }

#endif
}