# Local TCP Functionality  

1. Class : TCPClient  
    1.1. Sending Data  
2. Class : TCP Server  
    2.1. Receiving Data  

# 1. Class : TCPClient  

## Method List  

```C#
public void OnapplicationFocus(bool);
public void OnApplicationQuit();
public async void start_tcp_client_connection();
public async void stop_tcp_client_connection();
public async void SendPVImageAsync(byte[]);
public async void SendSpatialImageAsync(string, byte[], long, long);
public async void StartCameraCalibration();

```  

## Class Description  

The `class : TCPClient` class is used as a means of establishing a connection between the HoloLens 2 device and the connected computer running the back-end of the application. Once a connection between the local TCP Client and remote TCP Server is made, we are able to send data messages to the computer to tell the back-end what we want it to do.  

## Remote Server Connection  

A connection to the remote TCP Server is made using the `method : start_tcp_client_connection()` method. In order to establish a connection with the computer, we must specify two important parameters:  

```C#
[SerializeField]
string remoteHost_IP_address, remote_connection_port;
```  

|Variable|Description|
|--------|-----------|
|**remoteHost_IP_address**|The IP address of the remote location. In this case, it refers to the IP of the PC and is associated to the connection port used.|  
|**remote_connection_port**| The TCP port of the remote location. We are also, in this case, refering to the port on the PC.|  

For the current setup of the project, the variable values for '*HATlaptop05*' are:  

* **remoteHost_IP**: 169.254.75.786  
* **remote_port**: 9090  

***Note**: These values may potentially change if running the TCPServer.py file on a different PC.*  

These variables are found and assigned from within the Unity editor. Using these values, a connection is established with the computer and we can begin sending data over the connection.  

## 1.1. Sending Data  

To send data over the TCP connection to the remote TCP Server we can use one of two methods, `method : SendPVImageAsync()` or `method : SendSpatialImageAsync()`. Both methods fundamentally perform the same action, the major difference is that one is used to send a single image captured from the photo/video camera ( *SendPVImageAsync()* ) and the other is used to send the combined image buffer captured from the front spatial cameras ( *SendSpatialImageAsync()* ).  

> *Note:* The `method : SendSpatialImageAsync()` variant is being used due to the method of image capture being from the front spatial cameras.  
>  
> In the unity application under the [Scripts > CameraStream > PVCameraScript] folder is a class called `class : CameraImageFrameStream`. This class was previously used in place of the `class : StereoCameraStream` class to capture image frames from the single photo/video camera on the HoloLens 2. It does not use the `plugin : HL2UnityPlugin` plugin, instead using Microsoft defined classes to create and access a video stream for capturing frames.  
>  
> The document `CameraStream-Framecapture.pdf` discusses how the process works, however, it is no longer used in the current state of the application. If photo/video camera access again becomes important, this document goes through how it works.  

### SendSpatialImageAsync()  

```C#
SendSpatialImageAsync(string header,byte[] LRFImage, long ts_left, long ts_right)
```

Before we can send our data we must first write a message to a data buffer. Our data buffer comes in the form of a `object : DataWriter` object. This data writer object is created after a connection is established to the remote TCP Server:  

```C#
public async void start_tcp_client_connection()
{
    // ...

    await _dataStreamSocket.ConnectAsync(_hostName, remote_connection_port);

    _dataWriter = new DataWriter(_dataStreamSocket.OutputStream);

    // ...
}
```  

To create our data message buffer we will use 4 variants of write methods:  

```C#
_dataWriter.WriteString();
_dataWriter.WriteInt32();
_dataWriter.WriteInt64();
_dataWriter.WriteBytes();
```  

|Method|Description|# Bytes|
|------|-----------|-------|
|WriteString()|The `method : WriteString()` method is used to write our header byte to the message buffer. This will specify what action we want to execute on the remote TCP Server end.|1 byte|
|WriteInt32()|The `method : WriteInt32()` method is used to write the 32 bit integer value that specifies the number image bytes that are being sent. This value is decoded and used to indicate how many bytes we should be reading to receive all the image bytes.|4 bytes|
|WriteInt64()|The `method : WriteInt64()` method is used to write the 64 bit long value of the timestamp captured for the respective image. This value is decoded and used for the filename of the decoded image.|8 bytes|
|WriteBytes()|The `WriteBytes()` method is used to write the image byte buffer to the message buffer. The number of bytes we write to the buffer is specified by the value written using the `method : WriteInt32()` method.|# bytes|  

Once we have finished creating our data message buffer, we can send the data to the remote TCP Server.  

```C#
// Send data to remote TCP Server
await _dataWriter.StoreAsync();
await _dataWriter.FlushAsync();
```  

Note that by writing unique header bytes, we can easily direct the back-end application. In the `method : stop_tcp_client_connection()` method, only a single header byte is stored to the data message buffer and sent to the remote TCP Server. The single header value `header = e` is sent to tell the back-end application that the HoloLens application has closed and the back-end can be stopped. The single header value `header = x` is sent to tell the back-end application to run the stereo camera calibration process.  

|Header|Description|
|------|-----------|
|**c**|Sending a set of stereo camera calibration images|
|**x**|Execute the stereo camera calibration process|
|**f**|Sending a set of front spatial images for Openpose processing|
|**v**|Sending a photo/video camera image for Openpose processing|
|**e**|Unity application has been closed, stop the back-end|  

# 2. Class : TCPServer  

## Method List  

```C#
public void Start();
public void OnApplicationQuit();
private async void start_tcp_server_listener();
private async void tcp_server_connection_established(SSL, SSLConnectionReceivedEventArgs);
//SSL = StreamSocketListener
```  

## Class Description  

The `class : TCPServer` class is used to receive the incoming data returned by the Openpose image processing occuring in the back-end application. Image data is sent to the remote TCP Server via our local TCP Client, Openpose then processes the images to find the joint coordinates, then the remote TCP Client encodes and sends the joint coordinate data to be received by the local TCP Server.  

Incoming messages are received and processed, decoding the data into a string form so that is can be futher converted into usable `object : Vector3` coordinate objects.  

## Initialization  

Upon starting the body tracking application, the `Unity : method : Start()` method is executed. To start the local TCP Server we first need to create a `object : StreamSocketListener` object. The listener object is then attached to our specified data port to listen for incoming messages sent by the remote TCP Client.  

> **Important:** The connection port we are attaching our listener object to, must be the same port we are trying to send data to. Therefore, the connection port we attach our listener to in our local TCP Server must match the connection port we specify in our back-end `file : TCPClient.py` script.  

Another action performed in the initialization of the local TCP Server is subscribing our event handler methods to their respective events:  

```C#
TCP_Data_Read += TCPStreamCoordinateHandler.TCPDataReadHandler;
_listener.ConnectionReceived += tcp_server_connection_established;
```  

These lines of code serve the basis for our event handling. both `event : TCP_Data_Read` and `StreamSocketListener : event : ConnectionReceived` are events that can occur during the runtime of the application. The above lines of code are used to attach or subscribe our event handler method to the event. This means that when the event occurs or is triggered, our event handler method will be executed. In the case of the `event : ConnectionReceived` event, when the stream sockt listener has received a connection, the event will me triggered and the `method : tcp_server_connection_established()` method will be executed.  

The `method : OnApplicationQuit()` method performs the inverse action of the above lines of code, disconnection or unsubscribing our event handler method from the event.  

## 2.1. Receiving Data  

The local TCP Server received data from within the `method : tcp_server_connection_established()` method. Within this method we create a loop that runs continually during the runtime of the application or until an error occurs (where a connection with the remote TCP Client is re-established).  

To read the data being sent from the remote TCP Client we must first create a data message buffer. This is done by creating a `object : DataWriter` object:  

```C#
using (var _dataReader = new DataReader(args.Socket.InputStream))
{
    // reading from the incoming data stream
}
```  

The data reader object allows us to access the incoming message buffer and read the byte data from it. When reading data from the data stream there are two important settings to establish for the data reader object:  

|Setting|Description|
|-------|-----------|
|InputStreamOptions.Partial|This allows the ability to read from the data stream in chunks.|
|ByteOrder.BigEndian|Sets the order of incoming bytes of data. A big-endian system stores the most significant byte of a word (2 bytes or 16 bits) at the smallest memory address and the least significant byte at the largest. This will affect how data is decoded.|  

Once we have established our data reader settings, we can start reading data from the data stream. This is done using `DataReader : method : LoadAsync()` where the input parameter is the number of bytes we want to read in from the data stream.  

**Note:** The data message bytes being sent by the remote TCP Client are encoded differently than when the local TCP Client encodes them, therefore, it is important to note that the number of bytes we need to load from the input stream is not equal to the number of bytes we load in the remote TCP Server.  

The remote TCP Client encodes the message bytes using a **base 64 encoding**. In our data message buffer is a header, the data length, and the data itself. The number of bytes for each section of the message are as follows:  

|Section|Description|# Bytes|
|-------|-----------|-------|
|Header|The unique message identifier.|4 bytes|
|Data Length|The number of bytes in the data message.|8 bytes|
|Data|The Openpose coordinate data. Length is specified by `Data Length`.|# bytes|  

To read data from the data stream we use two methods:  

```C#
_dataReader.LoadAsync(/*number-of-bytes*/)
_dataReader.ReadString(/*number-of-bytes*/)
```  

The `DataReader : method : Loadasync()` is used to get the data bytes from the input stream and store them to the data reader message buffer. From the message buffer, we can then read the data into a format we can better work with using the `DataReader : method : ReadString()` method. The read string method takes the bytes in the message buffer and stores them as a string of data. From this string of data, we can convert the data back into bytes using `Convert : method : FromBase64String()`. Once in byte form, we can easily convert our data to the form we need.  

For each incoming message, the check byte or header byte is read first by loading the first 4 bytes from the input stream. This header byte signifies a valid message and allows us to proceed. The next 8 bytes loaded from the input stream equate to the length integer specifying the number of bytes of data being sent. This integer is then used to receive load the remaining bytes from the input stream containing our coordinate data.  

Once all of the incoming data has been received, the joint coordinate data string is passed as a parameter to the `event : TCP_Data_Read` event and the event is triggered. By triggering this event, we are making a call to any subscribed event handler methods and executing it. This is done in the following line of code:  

```C#
bool data_read_status = TCP_Data_Read?.Invoke(BJC_data) ?? default;
```  

> |Component|Description|
> |---------|-----------|
> |TCP_Data_Read|The event we are triggering|  
> |TCP_Data_Read?|Check to see if any handler methods are subscribed to the event|
> |Invoke(param)|If a handler method is subscribed, execute it and pass it `param` as its parameter|
> |?? default|If no handler methods are subscribed, return the default value (false)|  

The event handler method to the `event : TCP_Data_Read` event is the method `TCPStreamCoordinateHandler : TCPDataReadHandler()`. It is from this method that the Openpose coordinate string is converted to an array of `object : Vector3` objects. These objects can then be used to update and display the users body position using game objects.