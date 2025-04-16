using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;

// Socket Listener acts as a server and listens to the incoming
// messages on the specified port and protocol.
public class SocketListener
{
   private static readonly Mutex mutex = new Mutex();

    public static int Main(String[] args)
    {
        StartServer();
        return 0;
    }

    public static void StartServer()
    {
        IPHostEntry host = Dns.GetHostEntry("localhost");
        IPAddress ipAddress = host.AddressList[0];
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

        try
        {
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(10);

            Console.WriteLine("Servidor a aguardar ligações...");

            while (true)
            {
                Socket handler = listener.Accept();
                Console.WriteLine("Cliente conectado.");
                Thread clientThread = new Thread(() => HandleClient(handler));
                clientThread.Start();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPressione qualquer tecla para continuar...");
        Console.ReadKey();
    }

    public static void HandleClient(Socket handler)
    {
        string data = "";
        byte[] bytes = new byte[1024];

        try
        {
            while (true)
            {
                int bytesRec = handler.Receive(bytes);
                data += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                if (data.Contains("\n")) break;
            }

            Console.WriteLine("Texto recebido: {0}", data);

            mutex.WaitOne();
            try
            {
                File.AppendAllText("dados_recebidos.txt", data + Environment.NewLine);
            }
            finally
            {
                mutex.ReleaseMutex();
            }

            byte[] msg = Encoding.ASCII.GetBytes("Dados recebidos com sucesso.\n");
            handler.Send(msg);
        }
        catch (Exception e)
        {
            Console.WriteLine("Erro ao lidar com cliente: " + e.Message);
        }
        finally
        {
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
