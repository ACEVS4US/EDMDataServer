using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.IO.Ports;

namespace EDM_Data_Server
{
    class Program
    {
        static void Main(string[] args)
        {

            SerialEDM EDMSerial;
           
            //Get user input on the type of EDM they want to use.
            Console.WriteLine("Select the type of EDM you are using:\r\n1   TC2002\r\n2   DI2002\r\n");
                while(true){
                    string line = Console.ReadLine();
                    if(line.Equals("1")){
                        EDMSerial = new EDM_TC2002();
                        break;
                    }
                    else if(line.Equals("2")){
                        EDMSerial = new EDM_DI2002();
                        break;
                    }
                }

            //SerialPort s_port;
            TcpListener server = null;

           

            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 16;
                IPAddress[] localAddr = Dns.GetHostAddresses("IRLSS27034");
                //IPAddress localAddr = IPAddress.Parse("IRLSS27034");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr[1], port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                String response = null;
                int averaging = 1;
              
                

                // Enter the listening loop. 
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");


                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client. 
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        Console.WriteLine("Received: {0}", data);

                        switch (data)
                        {
                            case "Averaging":
                                break;
                            case "Reset":
                                if (!EDMSerial.SetUpEDM())
                                {
                                    Console.WriteLine("EDM setup returned error code:" + EDMSerial.GetLastError);

                                    response = "false";
                                }
                                else response = "true";
                                break;
                            case "Measure":
                                double result = 0.0;
                                double summed_result = 0.0;
                                for (int j = 0; j < averaging; j++)
                                {
                                    while (!EDMSerial.ReadEDM(ref result))
                                    {
                                        Console.WriteLine("EDM setup returned error code:" + EDMSerial.GetLastError);
                                    }
                                    summed_result = (summed_result + result);  
                                }
                                result = summed_result / averaging;

                                Console.WriteLine(result +" metres\n");

                                response = "Measure: "+ result.ToString();
                                break;
                            case "Turnoff":
                                break;
                        }


                        

                        // Process the data sent by the client.
                        //data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
