using UnityEngine;

/*
Summary:
Class is used for accessing a reference to the TCP Client after the
body tracking scene has started. This allows us to call the
start_tcp_client_connection() method using the 'Connect Server' 
voice command if the remote TCP Server connection is lost during use.
    ie. do not need to restart application
*/
public class TCPReconnectHandler : MonoBehaviour
{
    TCPClient tcp_client;
    GameObject ImageFrameStream_Object;

    /*
    Summary:
    Get a reference to the TCPClient class.
    */
    void Start()
    {
        ImageFrameStream_Object = GameObject.Find("CameraImageFrameStream_Object");
        
        if (ImageFrameStream_Object != null)
        {
            tcp_client = ImageFrameStream_Object.GetComponent<TCPClient>();
        }   
    }

    /*
    Summary: 
    The handler method for the voice command 'Connect Server' to reconnect to the
    remote TCP Server if it closes or disconnects.
    */
    public void ConnectToRemoteServer() { tcp_client.start_tcp_client_connection(); }
}
