using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using System;
using System.Text;
using System.Threading.Tasks;

#if !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
#endif

public delegate bool TCP_Data_Read_EventHandler(string _bodyCoordinatesData);

/*
Summary:
TCP Server, when connected with the remote TCP Client, is used to receive the encoded
openpose data that is sent by the computer.
*/
public class TCPServer : MonoBehaviour
{ 
    public static bool tcp_server_connection_status { get; private set; } = false;

#if !UNITY_EDITOR
    public event TCP_Data_Read_EventHandler TCP_Data_Read;

    // objects for data communication over TCP
    StreamSocket _dataStreamSocket = null;
    StreamSocketListener _listener = null;
#endif
    string connection_port;

    /*
    Summary:
    Start is used to initialize the local TCP Server components needed to
    establish a connection with the remote TCP Client. Event handlers are
    also subscribed for handling ConnectionReceived and TCP_Data_Read events.
    */
    public void Start()
    {
#if !UNITY_EDITOR
        // subscribe handler to event
        TCP_Data_Read += TCPStreamCoordinateHandler.TCPDataReadHandler;

        connection_port = "8080"; // must match the port number defined in the TCPClient.py file
        _listener = new StreamSocketListener();

        // subscribe handler to event
        _listener.ConnectionReceived += tcp_server_connection_established;

        _listener.Control.KeepAlive = false;
        // start the TCP Listener for receiving connection request from client
        start_tcp_server_listener();
#endif
    }


    /*
    Summary:
    When application shuts down, unsubscribe handler methods from events.
    */
    public void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        _listener.ConnectionReceived -= tcp_server_connection_established;
        TCP_Data_Read -= TCPStreamCoordinateHandler.TCPDataReadHandler;
#endif
    }

    /*
    Summary:
    Method attaches the remote connection listener to the specified data port.
    */
    private async void start_tcp_server_listener()
    {
        try
        {

#if !UNITY_EDITOR
            // attach the listener to the data port
            await _listener.BindServiceNameAsync(connection_port);
#endif
        }
        catch (Exception e) { UnityDebug.Log("Local TCP Server :: ERROR :: Error in _listener.BindServiceNameAsync \n" + e.Message); }
    }


    /*
    Summary:
    Method is used to perpetually read data from the remote TCP Client. While connected, the method
    remains in the loop, decoding the mesage data into a string that is processed by the 
    TCPStreamCoordianteHandler.
    */
#if !UNITY_EDITOR
    string _checkByte_string, data_len_string, dataBuffer, BJC_data;
    byte[] _checkByte, data_len_bytes, data_bytes;
    uint data_len;
    private async void tcp_server_connection_established(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {   
        DataReader _dataReader = null;

        try
        {
            _dataReader = new DataReader(args.Socket.InputStream);
            if (_dataReader == null) throw new InvalidOperationException("Local TCP Server :: ERROR :: DataReader object is null.");

            // read the input steam data in chunks rather than all at once
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            _dataReader.ByteOrder = ByteOrder.BigEndian;

            tcp_server_connection_status = true;
            UnityDebug.Log("Local TCP Server :: TCP Server socket connection established.");

            for (;;)
            {
                try
                {
                    while (_dataReader.UnconsumedBufferLength < 4) { await _dataReader.LoadAsync(4); }

                    // get check byte, validate it is the start of a new message
                    _checkByte_string = _dataReader.ReadString(4);
                    _checkByte = Convert.FromBase64String(_checkByte_string);
                    UnityDebug.Log("Local TCP Server :: Check Byte: " + _checkByte[0]);

                    if (_checkByte[0] != 0x01) continue; // incorrect check byte

                    await coordinate_handler(_dataReader);
                    
                }
                catch (Exception ex)
                {
                    UnityDebug.Log("Local TCP Server :: ERROR :: Error consuming data from data reader loop.\n" + ex.Message);
                    _dataReader?.Dispose();
                    _dataReader = SafeInitializeDataReader(args);
                    if (_dataReader == null) 
                    {
                        UnityDebug.Log("Local TCP Server :: ERROR :: New DataReader object could not be established. Exiting TCP Server loop.");
                        break;
                    }

                    UnityDebug.Log("Local TCP Server :: New DataReader object established.");
                }
            }
        }
        catch (Exception e)
        {
            tcp_server_connection_status = false;
            UnityDebug.Log("Local TCP Server :: ERROR :: TCP Server socket connection closed. (ERROR)\n" + e.Message);
        }
        finally
        {
            _dataReader?.Dispose();
        }
    }


    private DataReader SafeInitializeDataReader(StreamSocketListenerConnectionReceivedEventArgs args)
    {
        try
        {
            return new DataReader(args.Socket.InputStream);
        }
        catch (Exception e)
        {
            UnityDebug.Log("Local TCP Server :: ERROR :: Error initializing DataReader object.\n" + e.Message);
            return null;
        }
    }


    private async Task coordinate_handler(DataReader _dataReader)
    {
        UnityDebug.Log("Local TCP Server :: Coordinate Handler :: ID: 0x01");
        // get data length bytes
        await _dataReader.LoadAsync(8);
        data_len_string = _dataReader.ReadString(8);
        data_len_bytes = Convert.FromBase64String(data_len_string);
        if (BitConverter.IsLittleEndian) Array.Reverse(data_len_bytes);
        data_len = BitConverter.ToUInt32(data_len_bytes, 0);

        // get openpose coordinate data bytes
        await _dataReader.LoadAsync(data_len);
        dataBuffer = _dataReader.ReadString(data_len);
        data_bytes = Convert.FromBase64String(dataBuffer);
        BJC_data = Encoding.UTF8.GetString(data_bytes);

        // process and convert the joint coordinates string into Vector3 objects
        bool data_read_status = TCP_Data_Read?.Invoke(BJC_data) ?? default;
    }
#endif
}



/*
    string _checkByte_string, data_len_string, dataBuffer, BJC_data;
    byte[] _checkByte, data_len_bytes, data_bytes;
    uint data_len;
    private async void tcp_server_connection_established(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {   
        try
        {
            tcp_server_connection_status = true;
            while (true)
            {
                using (var _dataReader = new DataReader(args.Socket.InputStream))
                {
                    // read the input steam data in chunks rather than all at once
                    _dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    _dataReader.ByteOrder = ByteOrder.BigEndian;

                    // get check byte, validate it is the start of a new message
                    await _dataReader.LoadAsync(4);
                    _checkByte_string = _dataReader.ReadString(4);
                    _checkByte = Convert.FromBase64String(_checkByte_string);

                    if (_checkByte[0] != 0x01) continue; // incorrect check byte

                    // get data length bytes
                    await _dataReader.LoadAsync(8);
                    data_len_string = _dataReader.ReadString(8);
                    data_len_bytes = Convert.FromBase64String(data_len_string);
                    if (BitConverter.IsLittleEndian) Array.Reverse(data_len_bytes);
                    data_len = BitConverter.ToUInt32(data_len_bytes, 0);

                    // get openpose coordinate data bytes
                    await _dataReader.LoadAsync(data_len);
                    dataBuffer = _dataReader.ReadString(data_len);
                    data_bytes = Convert.FromBase64String(dataBuffer);
                    BJC_data = Encoding.UTF8.GetString(data_bytes);

                    _dataReader.Dispose();

                    // process and convert the joint coordinates string into Vector3 objects
                    bool data_read_status = TCP_Data_Read?.Invoke(BJC_data) ?? default;
                }
            }            
        }
        catch (Exception e) 
        {
            tcp_server_connection_status = false;
            UnityDebug.Log("Local TCP Server :: ERROR :: TCP Server socket connection closed. (ERROR)\n" + e.Message);
        }
    }
*/