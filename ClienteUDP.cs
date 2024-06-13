using eDIPEmotiBit;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ClienteUDP /* : MonoBehaviour */
{
    // Flag to control the listening state
    bool isListening = false;
    // UDP client for receiving data
    UdpClient udpClient;

    // Action to be invoked when data is received
    public System.Action<string> onDataReceived = null;

    // Start is called before the first frame update
    void Start()
    {
        // Start listening for UDP data if not already listening
        if (!isListening)
        {
            UDPListener();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // No need for update logic here as data processing is done in Task
    }

    // Initializes and starts the UDP listener in a separate Task
    
    private void UDPListener()
    {
        // Endpoint for reading any incoming data
        var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedResults;
        isListening = true; // Set listening flag to true at the start
        Task.Run(() =>
        {
            using (udpClient = new UdpClient(12346)) // Listen on port 12346
            {
                while (isListening) // Continue listening until isListening is set to false
                {
                    try
                    {
                        // Blocking call to receive data
                        receivedResults = udpClient.Receive(ref remoteEndPoint);
                        // Invoke the onDataReceived action with the received data as a string                        
                        if (onDataReceived != null) onDataReceived(Encoding.ASCII.GetString(receivedResults));
                    }
                    catch (SocketException ex)
                    {
                        //Debug.Log($"UDP Receive error: {ex.Message}");                        
                    }
                }

                // Log when the listener task ends
                Debug.Log("UDPListener Task End!");                
            }
        });
    }

    // Clean up resources when the application quits or the component is disabled
    private void Cleanup()
    {
        if (udpClient != null)
        {
            isListening = false; // Ensure the listening loop ends            
            udpClient.Close(); // Close the UDP client
            udpClient = null; // Help with garbage collection
            Debug.Log("Close UDP socket");
        }
    }

    private void OnApplicationQuit()
    {
        Cleanup(); // Call cleanup method
    }

    private void OnDisable()
    {
        Cleanup(); // Ensure resources are cleaned up even if GameObject is disabled before quitting
    }
}
