using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

// Socket Listener acts as a server and listens to the incoming
// messages on the specified port and protocol.
public class SocketListener
{
    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

    public static void StartServer()
    {
        // Get Host IP Address that is used to establish a connection
        // In this case, we get one IP address of localhost that is IP : 127.0.0.1
        // If a host has multiple addresses, you will get a list of addresses
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        try
        {

            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);
            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 10 requests at a time
            listener.Listen(10);

            Console.WriteLine("Waiting for a connection...");
            Socket handler = listener.Accept();

            // Incoming data from the client.
            string data = null;
            byte[] bytes = null;

            while (true)
            {
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                data.Split(" ");

                if (data.IndexOf("/n") > -1)
                {
                    break;
                }
            }

            Console.WriteLine("Text received : {0}", data);

            byte[] msg = Encoding.ASCII.GetBytes(data);
            handler.Send(msg);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\n Press any key to continue...");
        Console.ReadKey();
    }
}








// Client app is the one sending messages to a Server/listener.
// Both listener and client can send messages back and forth once a
// communication is established.
public class SocketClient
{
    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }

    public static void StartClient()
    {
        byte[] bytes = new byte[1024];

        try
        {
            // Connect to a Remote server
            // Get Host IP Address that is used to establish a connection
            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
            // If a host has multiple addresses, you will get a list of addresses
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP  socket.
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                // Connect to Remote EndPoint
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString());

                // Encode the data string into a byte array.
                byte[] msg = Encoding.ASCII.GetBytes("This is a test /n");

                // Send the data through the socket.
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.
                int bytesRec = sender.Receive(bytes);
                Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}
