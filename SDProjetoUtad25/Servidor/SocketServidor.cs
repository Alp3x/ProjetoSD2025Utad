using System;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


public class SocketListener
{
    private static Mutex mutex = new Mutex();
    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

    public static void StartServer()
    {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 1000);
        Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        int numThreads = 0;
        listener.Bind(localEndPoint);
        try
        {
            while (true)
            {
                try
                {

                    listener.Listen(10);

                    Console.WriteLine("Awaiting connection");

                    while (true)
                    {
                        Socket handler = listener.Accept();
                        Console.WriteLine("Client conected.");
                        Thread agregadorThread = new Thread(() => HandleClient(handler));
                        agregadorThread.Name = System.String.Format("Agregador{0}", numThreads + 1);
                        agregadorThread.Start();
                        numThreads++;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
        catch (Exception e) { Console.WriteLine(e.ToString()); }
    }
    public static void HandleClient(Socket handler)
    {
        string data = "";
        byte[] bytes = new byte[1024];
        
            try
            {
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (data.Contains("agregToServer")) {
                    Console.WriteLine("Text recieved: {0}", data);
                    mutex.WaitOne();
                    try
                    {
                        File.AppendAllText("dados_recebidos.txt", Thread.CurrentThread.Name + "\n" + data + Environment.NewLine);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    mutex.ReleaseMutex();

                    Console.WriteLine("Data received sucessfullyn\n");
                    byte[] msg = Encoding.ASCII.GetBytes("Data received sucessfully\n");
                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    Console.WriteLine("Socket Closed");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        
    }
}

