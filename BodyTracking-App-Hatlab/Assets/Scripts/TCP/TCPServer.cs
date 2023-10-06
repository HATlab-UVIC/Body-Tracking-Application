using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
#endif

public delegate void TCP_Data_Read_EventHandler(string _bodyCoordinatesData);

// Use to receive data from the computer
public class TCPServer : MonoBehaviour
{
    // Variable definitions
    Stream _dataStreamIn;
    byte[] OP_Bytes_Data;
    uint OP_Bytes_Length;
    string _bodyCoordinatesData;
    BodyJointCoordinates _bodyJointCoordinates;
    public event TCP_Data_Read_EventHandler TCP_Data_Read;
    public static bool tcp_server_connected { get; set; } = false; // TODO: Change set back to private | Changed for testing


    private static int DATAREADER_TIMOUT = 30000; // 30 seconds

    public Vector3[] op_BodyPositionCoordinates;
    Vector3 coordinates_offset = new Vector3(0, 0, 0);
    Transform _alignment;

    // logging boolean variables


    // defining necessary variables for reading data over TCP
#if !UNITY_EDITOR
    StreamSocket _dataStreamSocket = null;
    StreamSocketListener _listener = null;
#endif
    string connection_port;


    // initialize the TCP server to listen for incoming data from
    // the client on the computer
    private void Start()
    {
        _bodyJointCoordinates = BodyJointCoordinates.Instance;
        TCP_Data_Read += _bodyJointCoordinates.getBodyCoordinatesFromTCPStream;

        coordinates_offset = _alignment.position;

#if !UNITY_EDITOR

        // must match the port number defined in the TCPClient.py file
        connection_port = "8080";
        _listener = new StreamSocketListener();

        // subscribe the tcp_server_connection_established() method to the
        // StreamSocketListener:event:ConnectionReceived event
        _listener.ConnectionReceived += tcp_server_connection_established;

        _listener.Control.KeepAlive = false;
        start_tcp_server_listener();
#endif
    }


#if !UNITY_EDITOR

    // connect the TCP server listener to the data port to
    // start receiving data over TCP
    private async void start_tcp_server_listener()
    {
        try
        {
            // attach the listener to the data port
            await _listener.BindServiceNameAsync(connection_port);
            Debug.Log("TCP Server listener started.");
        }
        catch (Exception e) { Debug.Log("ERROR: " + e.Message); }
    }


    // connection is established with the TCP client on the computer. 
    // method is used to retrieve openpose data from the data stream
    private async void tcp_server_connection_established(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        try
        {
            tcp_server_connected = true;
            // create a data reader object to read data from the input stream
            using (var _dataReader = new DataReader(args.Socket.InputStream))
            {
                while (true)
                {
                    // read the input steam data in chunks rather than all at once
                    _dataReader.InputStreamOptions = InputStreamOptions.Partial;

                    // load data from the input stream
                    await _dataReader.LoadAsync(DATAREADER_TIMOUT);
                    var _inputStream_OP_Bytes_Length = _dataReader.ReadString(5);

                    // if input data buffer contains "b" as a suffix character, remove it
                    if (_inputStream_OP_Bytes_Length.EndsWith("b"))
                    {
                        _inputStream_OP_Bytes_Length = _inputStream_OP_Bytes_Length.Substring(0, _inputStream_OP_Bytes_Length.Length - 1);
                    }

                    // convert string of number of image bytes to unsinged 32-bit integer
                    // to be used to retreive the image bytes from the data stream
                    OP_Bytes_Length = Convert.ToUInt32(_inputStream_OP_Bytes_Length);
                    var _inputStream_OP_Bytes_Data = _dataReader.ReadString(OP_Bytes_Length);

                    _inputStream_OP_Bytes_Data = _inputStream_OP_Bytes_Data.Substring(1, _inputStream_OP_Bytes_Data.Length - 1) + "==";

                    OP_Bytes_Data = Convert.FromBase64String(_inputStream_OP_Bytes_Data);
                    _bodyCoordinatesData = Encoding.UTF8.GetString(OP_Bytes_Data);

                    // store the joint data to the body position manager
                    TCP_Data_Read?.Invoke(_bodyCoordinatesData);
                }
            }
        }
        catch (Exception e) 
        {
            tcp_server_connected = false;
            Debug.Log("TCP Server socket connection closed.");
            Debug.Log("ERROR: " + e.Message);
        }
    }

#endif
}