using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SocketProgramming
{
    class Program
    {
        static string ip = "127.0.0.1";
        static Int32 port = 11000;
        private const string incrementMsg = "increment";
        private const string evaluateMsg = "evaluate";
        static int counter = 0;
        static void Main(string[] args)
        {
            // Start socket server and listening for the client
            //Task.Run(() => StartScoketServer());

            //Start client
            Task.Run(() => StartClient1());

            Console.ReadLine();
        }

        private static void StartScoketServer()
        {
            // Establish the local endpoint for the socket.  
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket server = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // listen for incoming connections.  
            try
            {
                server.Bind(localEndPoint);
                server.Listen(10);

                // Start listening for connections.  
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");

                    Socket client = server.Accept();
                    string receivedData = null;
                   
                    while (true)
                    {
                        byte[] bytes = new Byte[1024];
                        int bytesRec = client.Receive(bytes);
                        receivedData = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        switch (receivedData.ToLower())
                        {
                            case incrementMsg:
                                //TODO: increment value
                                counter++;
                                break;
                            case evaluateMsg:
                                //TODO: send back current value.
                                byte[] msg = Encoding.ASCII.GetBytes(counter.ToString());
                                client.Send(msg);
                                break;
                                //case "close":
                                //    client.Shutdown(SocketShutdown.Both);
                                //    client.Close();
                                //    Console.WriteLine("XXXXXXXXXXXX Closed XXXXXXXXXXXXX");
                                //    break;
                        }
                       
                        if (client.Available == 0) break;
                    }
                  
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void StartClient()
        {
            for (int i = 0; i < 1; i++)
            {
                byte[] bytes = new byte[1024];

                // Connect to a server device.  
                try
                {
                    // Establish the remote endpoint for the socket.  

                    IPAddress ipAddress = IPAddress.Parse(ip);
                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

                    // Create a TCP/IP  socket.  
                    Socket client = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect the socket to the server endpoint. 
                    try
                    {
                        client.Connect(remoteEP);

                        //Sleep for 5 seconds
                        Thread.Sleep(5000);

                        // Sending "increment" msg to server.
                        byte[] msg = Encoding.ASCII.GetBytes(incrementMsg);

                        // Send the data through the socket.  
                        Console.WriteLine("Client {0}: Sending increment msg", i.ToString());
                        int bytesSent = client.Send(msg);

                        int bytesRec = client.Receive(bytes);
                        Console.WriteLine("Client {0}: Received increment msg:-{1}", i.ToString(), Encoding.ASCII.GetString(bytes, 0, bytesRec));


                        //Sending "evaluate" to server.
                        msg = Encoding.ASCII.GetBytes(evaluateMsg);

                        // Send the data through the socket.  
                        Console.WriteLine("Client {0}: Sending evaluate msg", i.ToString());
                        bytesSent = client.Send(msg);


                        // Receive response from the server.  
                        bytesRec = client.Receive(bytes);
                        Console.WriteLine("Client {0}: Received evaluate msg:-{1}", i.ToString(), Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        bytesSent = client.Send(Encoding.ASCII.GetBytes("close"));
                        // Release the client.  
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();

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
                Console.WriteLine("--------------------------------------------------------------\n");

            }
        }


        public static void StartClient1()
        {
            Console.WriteLine("Started");
            string message;

            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("Client "+i.ToString());
                using (var client = new TcpClient(ip, port))
                {
                    if (!client.Connected) continue;
                    var stream = client.GetStream();
                    //var reader = new StreamReader(stream, Encoding.ASCII);
                    //var writer = new StreamWriter(stream, Encoding.ASCII);

                    bool stopRequested;

                    //do
                    {
                        message = incrementMsg; //counter.ToString();
                        byte[] msgByte = Encoding.ASCII.GetBytes(message);
                        stream.Write(msgByte, 0, msgByte.Length);
                        counter++;

                        byte[] bytesToRead = new byte[client.ReceiveBufferSize];
                        int bytesRead = stream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                        Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));


                        message = evaluateMsg;//counter.ToString();
                        msgByte = Encoding.ASCII.GetBytes(message);
                        stream.Write(msgByte, 0, msgByte.Length);
                        counter++;

                        byte[] bytesToRead1 = new byte[client.ReceiveBufferSize];
                        int bytesRead1 = stream.Read(bytesToRead1, 0, client.ReceiveBufferSize);
                        Console.WriteLine("Received : " + Encoding.ASCII.GetString(bytesToRead1, 0, bytesRead1));

                        message = "<EOF>"; //counter.ToString();
                        byte[] msgByte1 = Encoding.ASCII.GetBytes(message);
                        stream.Write(msgByte1, 0, msgByte1.Length);

                        stopRequested = (counter > 10);
                    } //while (!stopRequested);
                   
                    counter = 0;

                    client.Close();
                }
                Console.WriteLine("--------------------------------------\n");
            }
            return;

            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
              
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip),port);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(remoteEP.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    //-----------------------------------------------

                    sender.Connect(remoteEP);

                     bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                     bytesRec = sender.Receive(bytes);
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
}
