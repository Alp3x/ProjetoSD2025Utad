using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class SocketListener
{
    private static Mutex mut = new Mutex();
    private const int maxNumThreads = 5;
    
    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

    public static void AgregFunc(Socket handler)
    {
        Console.WriteLine("{0} oppened a connection and is running code\n",Thread.CurrentThread.Name);
        Thread.Sleep(1000);
        //mut.WaitOne();
        
        string data = null;
        byte[] bytes = null;
        byte[] msg = null;

        while (true)
        {
            bytes = new byte[1024];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            data.Split(" ");

            Console.WriteLine("Text received : {0}", data);
            msg = Encoding.ASCII.GetBytes(data);

            if (data.IndexOf("/n") > -1)
            {
                break;
            }
        }
        string textBack = "\nYour Data as been saved";
        msg = Encoding.ASCII.GetBytes(textBack);
        handler.Send(msg);
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

        Thread.Sleep(1000);

        //mut.ReleaseMutex();
        Console.WriteLine("{0} has stopped running code and closed connection\n",Thread.CurrentThread.Name);
    }

    public static void StartServer()
    {
        // Get Host IP Address that is used to establish a connection
        // In this case, we get one IP address of localhost that is IP : 127.0.0.1
        // If a host has multiple addresses, you will get a list of addresses
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
        int numThreads = 0;

        try
        {
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);

            while (true)
            {
               
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();
                Thread newThread = new Thread(() => AgregFunc(handler));
                newThread.Name = String.Format("Wavy{0}", numThreads + 1);
                newThread.Start();
            }
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
//public class SocketClient
//{
//    public static int Main(String[] args)
//    {
//        StartClient();
//        return 0;
//    }

//    public static void StartClient()
//    {
//        byte[] bytes = new byte[1024];

//        try
//        {
//            // Connect to a Remote server
//            // Get Host IP Address that is used to establish a connection
//            // In this case, we get one IP address of localhost that is IP : 127.0.0.1
//            // If a host has multiple addresses, you will get a list of addresses
//            IPHostEntry host = Dns.GetHostEntry("localhost");
//            IPAddress ipAddress = host.AddressList[0];
//            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

//            // Create a TCP/IP  socket.
//            Socket sender = new Socket(ipAddress.AddressFamily,
//                SocketType.Stream, ProtocolType.Tcp);

//            // Connect the socket to the remote endpoint. Catch any errors.
//            try
//            {
//                // Connect to Remote EndPoint
//                sender.Connect(remoteEP);

//                Console.WriteLine("Socket connected to {0}",
//                    sender.RemoteEndPoint.ToString());

//                // Encode the data string into a byte array.
//                byte[] msg = Encoding.ASCII.GetBytes("This is a test /n");

//                // Send the data through the socket.
//                int bytesSent = sender.Send(msg);

//                // Receive the response from the remote device.
//                int bytesRec = sender.Receive(bytes);
//                Console.WriteLine("Echoed test = {0}",
//                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

//                // Release the socket.
//                sender.Shutdown(SocketShutdown.Both);
//                sender.Close();

//            }
//            catch (ArgumentNullException ane)
//            {
//                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
//            }
//            catch (SocketException se)
//            {
//                Console.WriteLine("SocketException : {0}", se.ToString());
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine("Unexpected exception : {0}", e.ToString());
//            }

//        }
//        catch (Exception e)
//        {
//            Console.WriteLine(e.ToString());
//        }
//    }
//}
