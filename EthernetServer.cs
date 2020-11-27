using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Net;
using System.IO.Ports;

namespace EDM_Data_Server
{
    public class EthernetServer:Server
    {
        //Listen for data packets over the ethernet adaptor
        public override void Listen()
        {
            // Set the TcpListener on port 16.
            port = 16;
            address = IPAddress.Parse("192.168.1.1");

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {

                    if (ni.Name.Equals("Local Area Connection"))
                    {
                        Console.WriteLine(ni.Name);
                        foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                Console.WriteLine(ip.Address.ToString());
                                address = ip.Address;

                            }
                        }
                    }

                }
            }
            try
            {
                
                server = new TcpListener(address, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                String response = null;
                int averaging = 1;



                // Enter the listening loop. 
                while (Enabled)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests. 
                    // You could also use server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");


                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;
                    try
                    {
                        // Loop to receive all the data sent by the client. 
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            //i = stream.Read(bytes, 0, bytes.Length);
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            byte[] msg = null;
                            Console.WriteLine("Received: {0}", data);
                            string content = "";
                            if (data.Contains("Averaging:"))
                            {
                                int index = data.IndexOf(':');
                                content = data.Substring(index + 1);
                                data = data.Remove(index);
                            }

                            switch (data)
                            {
                                case "Averaging":
                                    try
                                    {
                                        averaging = Convert.ToInt32(content);
                                        response = "true";
                                        msg = System.Text.Encoding.ASCII.GetBytes(response);
                                        // Send back a response.
                                        stream.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent: {0}\n", response);
                                    }
                                    catch (FormatException)
                                    {
                                        response = "false";
                                        msg = System.Text.Encoding.ASCII.GetBytes(response);
                                        // Send back a response.
                                        stream.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent: {0}\n", response);

                                    }
                                    break;
                                case "Reset":

                                    //reset the Total Station or EDM depending on what was selected
                                    if (!reading_2nd_laser)
                                    {
                                        if (!serial_instance.SetUpEDM())
                                        {
                                            Console.WriteLine("EDM setup returned error code:" + serial_instance.GetLastError);

                                            response = "false";
                                        }
                                        else response = "true";

                                        msg = System.Text.Encoding.ASCII.GetBytes(response);
                                        // Send back a response.
                                        stream.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent: {0}\n", response);
                                    }
                                    //otherwise we are reading the laser (special case)
                                    else
                                    {
                                        lsr.Reset();

                                        response = "true";
                                        msg = System.Text.Encoding.ASCII.GetBytes(response);
                                        // Send back a response.
                                        stream.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent: {0}\n", response);
                                    }
                                    break;
                                case "Measure":
                                    double result = 0.0;
                                    double summed_result = 0.0;

                                    if (!reading_2nd_laser)
                                    {
                                        double[] values = new double[averaging];
                                        for (int j = 0; j < averaging; j++)
                                        {
                                            if (ManualReading)
                                            {
                                                ManualInstrumentReading.Read(ref result);
                                            }
                                            else
                                            {
                                                while (!serial_instance.ReadEDM(ref result))
                                                {

                                                    Console.WriteLine("EDM read returned error code:" + serial_instance.GetLastError);
                                                    serial_instance.SetUpEDM(); //try calling setup again
                                                }
                                            }
                                            values[j] = result;
                                            Console.WriteLine("Measurement {0}: {1}", j, result.ToString());
                                            summed_result = (summed_result + result);
                                        }

                                        //compute the average
                                        result = summed_result / averaging;


                                        double population_variance = 0;
                                        //compute the variances using the averaged result
                                        foreach (double length in values)
                                        {
                                            double deviation = length - result;
                                            population_variance += Math.Pow(deviation, 2);
                                        }

                                        //the population variance
                                        if (averaging == 1)
                                        {
                                            population_variance = 0;

                                        }
                                        else
                                        {
                                            population_variance = population_variance / (averaging - 1);
                                        }
                                        //The standard deviation
                                        standard_deviation = Math.Sqrt(population_variance);
                                        response = result.ToString();

                                        msg = System.Text.Encoding.ASCII.GetBytes(response);

                                        // Send back a response.
                                        stream.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent: {0}", response);
                                    }
                                    else
                                    {
                                        double[] values = new double[averaging];
                                        for (int j = 0; j < averaging; j++)
                                        {
                                            try
                                            {
                                                result = LSR.ReadSample();

                                            }
                                            catch (ArgumentException e)
                                            {
                                                Console.WriteLine(e);
                                                break;
                                            }

                                            values[j] = result;
                                            Console.WriteLine("Measurement {0}: {1}", j, result.ToString());
                                            summed_result = (summed_result + result);



                                        }
                                        result = (summed_result / averaging);
                                        response = result.ToString();

                                        double population_variance = 0;
                                        //compute the variances using the averaged result
                                        foreach (double length in values)
                                        {
                                            double deviation = length - result;
                                            population_variance += Math.Pow(deviation, 2);
                                        }

                                        //the population variance
                                        if (averaging == 1)
                                        {
                                            population_variance = 0;

                                        }
                                        else
                                        {
                                            population_variance = population_variance / (averaging - 1);
                                        }

                                        //The standard deviation
                                        standard_deviation = Math.Sqrt(population_variance);

                                        //format the response
                                        msg = System.Text.Encoding.ASCII.GetBytes(response);

                                        // Send back a response.
                                        stream.Write(msg, 0, msg.Length);
                                        Console.WriteLine("Sent: {0}", response);
                                    }
                                    break;

                                case "Stdev":
                                    string stdev = standard_deviation.ToString();
                                    msg = System.Text.Encoding.ASCII.GetBytes(stdev);
                                    stream.Write(msg, 0, msg.Length);
                                    Console.WriteLine("Standard Deviation Sent: {0} ", stdev);
                                    break;
                                case "Turnoff":
                                    break;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        continue;
                    }

                    // Shutdown and end connection
                    Console.WriteLine("Connection closed");
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
        
        }
    }
}
