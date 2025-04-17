using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

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

    public static byte[] ReadData()
    {
        byte[] dados = File.ReadAllBytes("dadoswavy.txt");
        return dados;
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

                sender.Connect(remoteEP);

                Console.WriteLine("->Socket connected to {0}",sender.RemoteEndPoint.ToString());

                byte[] msg = Encoding.ASCII.GetBytes("scheduleRequest");

                int bytesSent = sender.Send(msg);

                while (true)
                {
                    int bytesRec = sender.Receive(bytes);
                    if (Encoding.ASCII.GetString(bytes, 0, bytesRec).Contains("setSchedule"))
                    {
                        var Data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        var DataSplit = Data.Split("-");
                        DateTime dataSendSchedule = Convert.ToDateTime(DataSplit[1]);
                        Console.WriteLine("->Schedule set to {0}", DataSplit[1]);
                        while (true) // Checks if it has to deliver data. After it deliver its data e it will wait to receive a new schedule
                        {
                            if (Math.Abs((DateTime.Now - dataSendSchedule).TotalSeconds) <= 4)
                            {
                                byte[] msgData =Encoding.ASCII.GetBytes("dataUpload-"+Encoding.ASCII.GetString(ReadData()));

                                int bytesSentDATA = sender.Send(msgData);
                                Console.WriteLine("-> dataUploadComplete\n");
                                break;
                            }
                            //if (dataSendSchedule < DateTime.Now)
                            //{
                            //    byte[] msgData = Encoding.ASCII.GetBytes("scheduleRequest");
                            //    int coco = sender.Send(msgData);
                            //    break;
                            //}
                        }
                    }
                }
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