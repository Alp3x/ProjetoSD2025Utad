﻿using System;
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

// Socket Listener acts as a server and listens to the incoming
// messages on the specified port and protocol.
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
        while (true)
        {

            try
            {
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEndPoint);
                listener.Listen(10);

                Console.WriteLine("Awaiting connection");

                while (true)
                {
                    Socket handler = listener.Accept();
                    Console.WriteLine("Client conected.");
                    Thread clientThread = new Thread(() => HandleClient(handler));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
    public static void HandleClient(Socket handler)
    {
        string data = "";
        byte[] bytes = new byte[1024];

        try
        {
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            Console.WriteLine("Text recieved: {0}", data);
            mutex.WaitOne();
            try
            {
                File.AppendAllText("dados_recebidos.txt", data + Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            mutex.ReleaseMutex();

            Console.WriteLine("Data received sucessfullyn\n");
            byte[] msg = Encoding.ASCII.GetBytes("Data received sucessfully\n");
            handler.Send(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine( e.Message);
        }
    }
}

