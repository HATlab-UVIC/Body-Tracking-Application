# Remote TCP Functionality  

1. Module : TCPServer  
    1.1. Receiving Data  
2. Module : TCPClient  
    2.1. Sending Data

# 1. Module : TCPServer  

## Function List  

```Python
def tcp_server():
def spatial_image_conversion(len, image_byte_buffer, save_loc_L, save_loc_R, ts):
    return file_name_left, file_name_right
def pv_image_conversion(image_byte_buffer, save_loc):
    return file_name
def receive_tcp_message(sock):
    return header, data_length, timestamp_bytes, image_bytes
def receive_all(sock, data_len):
    return image_bytes
def directory_check(dir):

if __name__ == "__main__": # executable script
```  

## Module Description  

The `module : TCPServer` module (remote TCP Server) is the main component of the back-end application. The remote TCP Server is used to receive data from the unity app TCP Client and process the data. The main functionality of the remote TCP Server is to receive, decode, and process images taken by the HoloLens 2 device. The images received are processed by the Openpose library to extract body joint coordinates that are used to create an artificial mirrored body pose in the parallel unity application.  

## Definitions  

Preceeding the function definitions in the `module : TCPServer` module are a collection of string definitions defining the file paths to the locations of:  

|Path|Description|
|----|-----------|
|tracking_img_pth|The location where the body tracking images are stored while the tracking scene is running.|
|calib_folder|The parent folder of where the calibration images are stored to.|
|left_cam_img_pth|The location where the front left camera images are stored when calibration images are captured.|
|right_cam_img_pth|The location where the front right camera images are stored when calibration images are captured.|
|cl.log_folder|The parent folder of where the coordinate log files are stored.|
|cl.coord_log_dir|The location where the coordinate log file is created.|  

These path variables are defined for easy reference to store and retreive data from the above locations during the runtime of the application. A validation step is also carried out when the remote TCP Server is first started to check that each path is defined and valid and that we can write and retreive data to these locations.  

## Unity App Client Connection  

When the `module : TCPServer` module is run, the `function : tcp_server()` function is executed. The first step that occurs is the creation of a `object : stream socket` object:  

```Python
sSock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
```  

The `sSock` variable is what we will use to receive the data being sent from the unity app TCP Client. before we can receive any data we must attach the stream socket to a port on our computer and start listening for connection requests:  

```Python
serverHost = '' # localhost
serverPort = 9090

try:
    # attach the socket to the specified port
    sSock.bind((serverHost, serverPort))
except socket.error as msg:
    # error
    return

sSock.listen(10)
```  

> *Note:* The `variable : serverHost` variable is to be left blank. The `variable :serverPort` variable must match the `Unity : TCPClient : variable : remote_connection_port` value specified within the unity editor.  

We do not want to proceed until we have established a connection with the unity app TCP Client, thus we will loop until a connection has been made. Once a connection has been made, break from the loop and continue to the main functionality of the back-end.  

```Python
while True:
    try:
        # connect to the remote TCP Client
        conn, addr = sSock.accept()
        break
    except KeyboardInterrupt:
        sys.exit(0)
    except Exception:
        continue
```  

## 1.1. Receiving Data  

The main operation of the back-end application occurs in a continuous while loop. Unless an error is encountered, continue reading and processing data sent by the unity app TCP Client.  

Receiving data works in 2 stages:  

### Receiving Message Bytes  

Message bytes are received by the `function : receive_tcp_message()` function:  

```Python
def receive_tcp_message(sock):
    return header, length, timestamps, image_data
```  

The `function : receive_tcp_message()` function takes in the stream socket object created after a connection to the unity app TCP Client has been established. From the stream socket object we can read in data from the incoming data stream using the `stream socket : function : recv()` function where the parameter specifies the number of bytes we want to read.  

The first step in reading the TCP message is to read the header byte which signifies what kind of data is being sent or what process should occur. this is done by reading 1 byte from the data stream:  

|Header|Description|
|------|-----------|
|**c**|Sending a set of stereo camera calibration images|
|**x**|Execute the stereo camera calibration process|
|**f**|Sending a set of front spatial images for Openpose processing|
|**v**|Sending a photo/video camera image for Openpose processing|
|**e**|Unity application has been closed, stop the back-end|  

Once the header byte has been read, if the header indicates an image or set of images is also being trasmitted, the next step is reading the 32 bit integer value representing the number of bytes of data being sent. 32 bits is equivalent to 4 bytes, therefore, we will read the next 4 bytes of data from the data stream.  

If a header value `header = 'f' or 'c'` is received, this indicates that a set of two spatial images is being transmitted. Along with the images, two timestamps recorded at the time of capture are also sent along with the image data. These are two 64 bit long values, thus, to receive the timestamps we must read the next 16 bytes from the data stream.  

Lastly we will read the image bytes. The image bytes are read and returned using the `function : receive_all()` function.

```Python
def receive_all(sock, data_len):
     image_bytes = b''
     # read all the image bytes (data_len # of bytes) from the data stream
     while len(image_bytes) < data_len:
          _data_buffer = sock.recv(data_len - len(image_bytes))
          if not _data_buffer:
               raise EOFError
          
          image_bytes += _data_buffer

     return image_bytes
```  

The above function works to receive all the image bytes by looping until the number of bytes read from the input stream match that of the length value that was sent along with the data message. Once all if the image bytes have been read from the input stream they are returned to be processed.  

For each respective step of reading data from the input stream, the number of bytes to specify to the `stream socket : function : recv()` are as follows:  

|Section|Description|Unity Data Type|# Bytes|
|-------|-----------|---------------|-------|
|Header|The unique message identifier.|Byte|1 bytes|
|Data Length|The number of bytes in the data message.|Int32|4 bytes|
|Timestamp|The timestamps of the spatial images at capture.|Int64|2 x 8 bytes|
|Data|The Openpose coordinate data. Length is specified by `Data Length`.|Bytes|# bytes|  

Once all the data has been read from the data stream we can move on to converting the image byte to images and processing the images using Openpose.  

### Bytes To Images  

Using the byte data received from the input stream, it can now be converted to an image file. Two functions are employed for performing this task: `function : spatial_image_conversion()` and `function : pv_image_conversion()`. Both functions work in a similar way. The main difference is that the **PV** version is used for converting and storing images captured from the photo/video camera and the **spatial** version operates on the stereo images from the front spatial cameras.  

Each function takes in the image bytes and converts them from a byte buffer to an image file, storing the image in the specified folder location. The last step of these functions is to return the file names of the images. The file name is what will allow us to pass the images to Openpose to be processed.  

## Processing Images  

Once the images have finished being processed, they can then be passed to Openpose for calculating the body joint coordinates of the user in the image frame.  

In order to process the images we must call the `body_from_image : function : find_points()` function. It is this function that is used to calculate and return a string containing the body joint coordinates.  

> ***Note**: Openpose is some machine learning magic. I dont understand how the code in the `module : body_from_image` module works, however, passing the image path and filename will return a string of joint coordinates.*  

### Stereo Depth Calculation  

**Note:** *The stereo depth calculation functionality is an area where further research and development is required.*  

Using the stereo images and the left and right sets of Openpose coordinates we can now perform the calculation to acquire our depth coordinate for each joint. This depth calculation is done using the `module : HLCameraCalibration` module. Barring that a stereo camera calibration has been performed, the `HLCameraCalibration : function : Calculate3DCoordinates()` function can be used to calculate the missing depth parameter.  

Once the images have been processed by both Openpose and the `HLCameraCalibration : function : Calculate3DCoordinates()` function, a string containing the joint coordinate data is returned in the form:  

* `[[[X Y Z][X Y Z][X Y Z]...[X Y Z]]]`  

This coordinate string is then passed to the `module : TCPClient` and sent back to the unity application.  

# 2. Module : TCPClient  

## Function List  

```Python
async def tcp_echo_client(data_msg, loop):
def sendPoints(openpose_coordinates):
```  

## Module Description  

The `module : TCPClient` module (remote TCP Client) is the component used to send the calculated joint coordinate data back to the unity application. The functions within the module encode the data message, establish a connection with the unity app TCP Server, and send the message data.  

## Unity App TCP Server Connection  

To be able to send data the the unity app TCP Server we must establish a connection with it. The connection to the unity app TCP Server is established in the `function : tcp_echo_client()` function.  

Within the function, a call is made to `asyncio : function : open_connection()`. It is from this function that a connection is established. It is important to note that the **IP** and **Port** parameters are the IP and Port of the connected HoloLens 2 device.  
 
> To find the IP address to be used for this step:
> 1) Open the `settings` window on the HoloLens  
> 2) Click on the `Update & Security` tab on the left side of the hologram  
> 3) Click on the `For developers` tab on the left side of the hologram
> 4) Scroll to the bottom of the hologram and look for `Connect using:`  
>       * We want to use the IP under the `Ethernet` title (Ethernet reffering to connection using a usb cable)  
>  
> The port should not change between HoloLens'  
>  
> > **HoloLens Info:**  
> > *HoloLens 1 (JAFFMT):*  
> > * Ethernet IP: **169.254.81.229**  
> > * Wi-Fi IP: **206.87.182.176**  
> >  
> > *HoloLens 2 (PKFUNF):*  
> > * Ethernet IP: **169.254.86.76**  
> > * Wi-Fi IP: **206.87.68.27**

## 2.1. Sending Data  

Within the remote TCP Server module, the `function : sendPoints()` function is used to encode and send the data back to the unity application.  

As described in the **[Unity Classes > TCP_Client_Server.pdf]** document, data encoding occurs in a different manner in the python back-end than in the C# TCP Client. Once a coordinate string is passed to the `function : sendPoints()` function, the byte data is encoded using a `base64 : function : b64encode()` encoding function. It is in this conversion that a different number of bytes are returned than what what passed into the converter.  

It is **important** to note that, because more bytes are returned from the encoding process, the data bytes must be encoded first, then the encoded data bytes length can be recorded. This process must be followed to ensure that the correct number of bytes are accounted for on the receiving end. If the un-encoded byte length is recorded and sent in the data message, all of the data bytes may not be received resulting in an incomplete/corrupt message.  

The number of bytes required to be read by the unity app TCP Server to appropriately receive each piece of data from the message is as follows:  

|Section|Description|# Bytes|
|-------|-----------|-------|
|Encoded Header|The unique message identifier.|4 bytes|
|Encoded Data Length|The number of bytes in the data message.|8 bytes|
|Encoded Data|The joint coordinate coordinate data. Length is specified by `Encoded Data Length`.|# bytes|  

Once the message has been encoded and combined into a single buffer, the data message is sent to the unity app TCP Server.