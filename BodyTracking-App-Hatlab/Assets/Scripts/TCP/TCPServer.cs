using UnityEngine;
using UnityDebug = UnityEngine.Debug;
using System;
using System.Text;

#if !UNITY_EDITOR
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
#endif

public delegate bool TCP_Data_Read_EventHandler(string _bodyCoordinatesData);

// Use to receive data from the computer
public class TCPServer : MonoBehaviour
{
    // Variable definitions
    
    public static bool tcp_server_connection_status { get; private set; } = false;

    // defining necessary variables for reading data over TCP
#if !UNITY_EDITOR
    public event TCP_Data_Read_EventHandler TCP_Data_Read;

    StreamSocket _dataStreamSocket = null;
    StreamSocketListener _listener = null;
#endif
    string connection_port;


    // initialize the TCP server to listen for incoming data from
    // the client on the computer
    public void Start()
    {
#if !UNITY_EDITOR
        TCP_Data_Read += TCPStreamCoordinateHandler.TCPDataReadHandler;

        // must match the port number defined in the TCPClient.py file
        connection_port = "8080";
        //UnityDebug.Log("Local TCP Sever :: connection port: " + connection_port);
        _listener = new StreamSocketListener();

        // subscribe the tcp_server_connection_established() method to the
        // StreamSocketListener:event:ConnectionReceived event
        _listener.ConnectionReceived += tcp_server_connection_established;

        _listener.Control.KeepAlive = false;
        start_tcp_server_listener();
#endif
    }


    public void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        _listener.ConnectionReceived -= tcp_server_connection_established;
        TCP_Data_Read -= TCPStreamCoordinateHandler.TCPDataReadHandler;
#endif
    }


    // connect the TCP server listener to the data port to
    // start receiving data over TCP
    private async void start_tcp_server_listener()
    {
        //UnityDebug.Log("Local TCP Server :: TCP listener bind to connection port...");
        try
        {
            // attach the listener to the data port
#if !UNITY_EDITOR
            await _listener.BindServiceNameAsync(connection_port);
#endif
            //UnityDebug.Log("Local TCP Server :: listener started.");
        }
        catch (Exception e) { UnityDebug.Log("Local TCP Server :: ERROR :: Error in _listener.BindServiceNameAsync \n" + e.Message); }
    }




    // connection is established with the TCP client on the computer. 
    // method is used to retrieve openpose data from the data stream
#if !UNITY_EDITOR
    string _checkByte_string, data_len_string, dataBuffer, BJC_data;
    byte[] _checkByte, data_len_bytes, data_bytes;
    uint data_len;
    private async void tcp_server_connection_established(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {   
        try
        {
            tcp_server_connection_status = true;
            
            //UnityDebug.Log("Local TCP Server :: Connection Established with Remote Client.");
            // create a data reader object to read data from the input stream

            while (true)
            {
                using (var _dataReader = new DataReader(args.Socket.InputStream))
                {
                    // read the input steam data in chunks rather than all at once
                    _dataReader.InputStreamOptions = InputStreamOptions.Partial;
                    _dataReader.ByteOrder = ByteOrder.BigEndian;

                    await _dataReader.LoadAsync(4);
                    _checkByte_string = _dataReader.ReadString(4);
                    //UnityDebug.Log("Local TCP Server :: Reading check byte string :: >> " + _checkByte_string);
                    _checkByte = Convert.FromBase64String(_checkByte_string);

                    if (_checkByte[0] != 0x01)
                    {
                        //UnityDebug.Log("Local TCP Server :: Incorrect check byte ( "+_checkByte[0].ToString()+" )");
                        continue;
                    }
                    //UnityDebug.Log("Local TCP Server :: Valid check byte ( "+_checkByte[0].ToString()+" )");

                    await _dataReader.LoadAsync(8);
                    data_len_string = _dataReader.ReadString(8);
                    //UnityDebug.Log("Local TCP Server :: Reading Length string :: >> " + data_len_string);
                    data_len_bytes = Convert.FromBase64String(data_len_string);
                    if (BitConverter.IsLittleEndian) Array.Reverse(data_len_bytes);
                    data_len = BitConverter.ToUInt32(data_len_bytes, 0);
                    //UnityDebug.Log("Local TCP Server :: Data Length :: Number of data bytes ( " +data_len+ " )");

                    await _dataReader.LoadAsync(data_len);
                    dataBuffer = _dataReader.ReadString(data_len);
                    data_bytes = Convert.FromBase64String(dataBuffer);
                    BJC_data = Encoding.UTF8.GetString(data_bytes);
                    //UnityDebug.Log("Local TCP Server :: Coordinate Data :: coordinates >> \n" + BJC_data);

                    _dataReader.Dispose();

                    // store the joint data to the body position manager
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

#endif
}