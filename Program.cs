using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
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
        private const string ip = "127.0.0.1";
        private const Int32 port = 11000;
        private const string INCREMENT_MSG = "increment";
        private const string EVALUATE_MSG = "evaluate";
        private const string CLOSE_MSG = "close";

        static void Main(string[] args)
        {
            // Start socket server and listening for the client
            Task.Factory.StartNew(() => StartScoketServer());

            Console.ReadLine();
        }

        /// <summary>
        /// To start server
        /// </summary>
        public static void StartScoketServer()
        {
            // Establish the local endpoint for the socket.  
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Create a TCP/IP socket.  
            Socket server = new Socket(localEndPoint.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                byte[] bytes = new Byte[1024];
                byte[] msg = new Byte[1024];

                //Bind sever with end point.
                server.Bind(localEndPoint);

                server.Listen(10);

                Console.WriteLine("Server: Started.");

                //Start client
                StartClient();

                // Start listening for connections.  
                while (true)
                {
                    Socket handler = server.Accept();

                    while (true)
                    {
                        string receiveData = string.Empty;
                        int bytesRec = handler.Receive(bytes);
                        receiveData = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                        if (receiveData.Equals(CLOSE_MSG, StringComparison.OrdinalIgnoreCase))
                        {
                            //Stop listening
                            break;
                        }

                        if (!string.IsNullOrEmpty(receiveData))
                        {
                            switch (receiveData.ToLower())
                            {
                                case INCREMENT_MSG:
                                    string resultMsg = Increment();
                                    msg = Encoding.ASCII.GetBytes(resultMsg);
                                    handler.Send(msg);
                                    break;
                                case EVALUATE_MSG:
                                    int maxvalue = Evaluate();
                                    receiveData = "Evaluate :" + maxvalue.ToString();
                                    msg = Encoding.ASCII.GetBytes(receiveData);
                                    handler.Send(msg);
                                    break;
                            }
                        }
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nServer: Press ENTER to continue...");
            Console.Read();

        }

        /// <summary>
        /// To start 500 clients
        /// </summary>
        public static void StartClient()
        {
            byte[] bytesToRead = new byte[1024];
            byte[] msgByte = new byte[1024];
            int bytesRead;
            string Message = string.Empty;

            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 500; i++)
                {
                    Console.WriteLine("Client# " + i.ToString());
                    using (var client = new TcpClient(ip, port))
                    {
                        Thread.Sleep(5000);
                        var stream = client.GetStream();
                        {
                            //Send "increment" message
                            Message = INCREMENT_MSG;
                            msgByte = Encoding.ASCII.GetBytes(Message);
                            stream.Write(msgByte, 0, msgByte.Length);

                            //Receive "success" message from server
                            bytesToRead = new byte[client.ReceiveBufferSize];
                            bytesRead = stream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                            Console.WriteLine("Client:" + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));

                            //Send "evaluate" message to server.
                            Message = EVALUATE_MSG;
                            msgByte = Encoding.ASCII.GetBytes(Message);
                            stream.Write(msgByte, 0, msgByte.Length);

                            //Receive "evaluate" value from server
                            bytesToRead = new byte[client.ReceiveBufferSize];
                            bytesRead = stream.Read(bytesToRead, 0, client.ReceiveBufferSize);
                            Console.WriteLine("Client: " + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead));

                            //Send "close" message to server to listen for next client.
                            Message = CLOSE_MSG;
                            byte[] msgByte1 = Encoding.ASCII.GetBytes(Message);
                            stream.Write(msgByte1, 0, msgByte1.Length);
                        }

                        client.Close();
                    }
                    Console.WriteLine("--------------------------------------\n");
                }
            });
        }

        /// <summary>
        /// Increment value by 1
        /// </summary>
        private static string Increment()
        {
            string resultMessage = string.Empty;
            try
            {
                int maxvalue = 0;
                string strqry = "SELECT MAX(Value) FROM Count";
                DataTable dtMax = DBConnectionManager.Instance.GetData(strqry);
                if (dtMax != null && dtMax.Rows.Count > 0 && dtMax.Rows[0][0] != DBNull.Value)
                {
                    maxvalue = Convert.ToInt32(dtMax.Rows[0][0]) + 1;
                }

                strqry = string.Format("INSERT INTO Count(Value) VALUES ({0})", maxvalue);
                DBConnectionManager.Instance.ExecuteQuery(strqry);
                resultMessage = "Success";
            }
            catch (Exception)
            {
                resultMessage = "Failed";
            }
            return resultMessage;
        }

        /// <summary>
        /// Get latest value
        /// </summary>
        /// <returns></returns>
        private static int Evaluate()
        {
            int maxvalue = 0;
            string strqry = "SELECT MAX(Value) FROM Count";
            DataTable dtMax = DBConnectionManager.Instance.GetData(strqry);
            if (dtMax != null && dtMax.Rows.Count > 0)
            {
                maxvalue = Convert.ToInt32(dtMax.Rows[0][0]);
            }
            return maxvalue;
        }
    }
}
