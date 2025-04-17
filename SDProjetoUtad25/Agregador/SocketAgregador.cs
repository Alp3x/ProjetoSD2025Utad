using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class SocketListener
{
    private static Mutex mut = new Mutex();
    private const int maxNumThreads = 5;
    // Static to coordinate between threads (put this at class level)
    private static object syncLock = new object(); // Already defined in your code
    private static bool hasSentData = false; // Also make this static to share across threads
    private static StringBuilder fullData = new StringBuilder(); // Shared across threads

    public static int Main(System.String[] args)
    {
        StartServer();
        return 0;
    }

    //func que atribui um horario para o wavy
    public static void sendSchedule(Socket handler) {
        byte[] msg = null;
        string tex = "setSchedule-" + DateTime.Now.AddSeconds(10).ToString();
        msg = Encoding.ASCII.GetBytes(tex);
        handler.Send(msg);
        Console.WriteLine("-> Schedule sent to {0}\n", Thread.CurrentThread.Name);
    }

    //Func responsavel por receber os dados do wavy
    public static void sendData(Socket handler,string data, StringBuilder fullData)
    {
        byte[] msg = null;
        var DataSplit = data.ToString().Split("-");
        if (DataSplit.Length < 2)
        {
            Console.WriteLine("-> Error: Data not received");
            return;
        }

        lock (syncLock) // Ensure thread-safe access
        {
            fullData.Append(DataSplit[1]); // Append data safely
        }

        Console.WriteLine("-> Data Received from {0}\n {1}\n", Thread.CurrentThread.Name, DataSplit[1]);
        string textBack = "Your Data has been saved";
        msg = Encoding.ASCII.GetBytes(textBack);
        handler.Send(msg);
    }

    public static void AgregFunc(Socket handler,DateTime sendDataScheduleServer,StringBuilder fullData, ManualResetEventSlim communicationAllowed, Socket server)
    {
        Console.WriteLine("{0} oppened a connection and is running code\n",Thread.CurrentThread.Name);
        fullData.Append(Thread.CurrentThread.Name + " data :\n");

        string data = null;
        byte[] bytes = null;
        byte[] msg = null;

        bytes = new byte[1024];
        while (true)
        {
            try
            {
                int bytesRec = handler.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                if (Math.Abs((DateTime.Now - sendDataScheduleServer).TotalSeconds) <= 4)
                {
                    if (!hasSentData)
                    {
                        EnviarParaServidor(fullData.ToString(),server);
                        sendDataScheduleServer.AddMinutes(2);
                        Console.WriteLine("-> Schedule set to {0}", sendDataScheduleServer.ToString());
                        hasSentData = false;
                    }
                }
                if (data == "scheduleRequest")
                {
                    sendSchedule(handler);
                }
                if (data.Contains("dataUpload-"))
                {
                    sendData(handler,data,fullData);
                    sendSchedule(handler);
                }
            }
            catch (Exception e) { }
        }
        Console.WriteLine("{0} has stopped running code and closed connection\n",Thread.CurrentThread.Name);
    }

    public static void StartServer()
    {
        ManualResetEventSlim communicationAllowed = new ManualResetEventSlim(true);
        //IP : 127.0.0.1
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
        int numThreads = 0;
        DateTime dataSendSchedule = DateTime.Now.AddSeconds(30);
        Console.WriteLine("->Schedule set to {0}", dataSendSchedule.ToString());
        Console.WriteLine(dataSendSchedule);
        IPHostEntry hostServer = Dns.GetHostEntry("localhost");
        IPAddress ipAddressServer = hostServer.AddressList[0];
        IPEndPoint remoteEPServer = new IPEndPoint(ipAddressServer, 1000);
        Socket server = new Socket(ipAddressServer.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            server.Connect(remoteEPServer);

            Console.WriteLine("connected to: " + remoteEPServer.ToString());

            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            while (true)
            {
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection...");
                Socket handler = listener.Accept();
                Thread newThread = new Thread(() => AgregFunc(handler,dataSendSchedule,fullData, communicationAllowed, server));
                newThread.Name = System.String.Format("Wavy{0}", numThreads + 1);
                newThread.Start();
                numThreads++;

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\n Press any key to continue...");
        Console.ReadKey();
    }
    
    public static void EnviarParaServidor(string dados,Socket server)
    {
        try
        {
            
            byte[] msg = Encoding.ASCII.GetBytes(dados);
            server.Send(msg);
 
            byte[] buffer = new byte[1024];
            int bytesRec = server.Receive(buffer);
            string resposta = Encoding.ASCII.GetString(buffer, 0, bytesRec);
            Console.WriteLine("Server Response " + resposta);
            
            hasSentData = true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

}
