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

            SerialEDM serial = null;
            bool manual_reading = false;
            ManualInstrumentReading reader = null;
            Laser laser = null;
            //SerialPort s_port;
            TcpListener server = null;
            double standard_deviation = 0.0;

            bool reading_2nd_laser = false;
            
           
            //Get user input on the type of EDM they want to use.
            Console.WriteLine("Select the type of instrument you are using:\r\n1   TC2002\r\n2   DI2002\r\n3   HP5519A\r\n4   Rocky\r\n5   Leica Disto D810 Touch\r\n");
            while(true){
                    string line = Console.ReadLine();

                if (line.Equals("1"))
                {
                    serial = new EDM_TC2002();
                    break;
                }

                else if (line.Equals("2"))
                {
                    serial = new EDM_DI2002();
                    break;
                }
                else if (line.Equals("3"))
                {
                    reading_2nd_laser = true;
                    laser = new Laser();
                    laser.Initialize_E1735A_DLL();

                    if (laser.readDeviceCnt() == 1)
                    {
                        laser.setDevice();
                        Console.WriteLine("Found 1 laser E5135 module, blinking LED ....\r\n");
                        laser.blink();
                        double b_strength = laser.ReadBeamStrength();
                        Console.WriteLine("The beam strength is: " + b_strength.ToString() + "\r\n");
                        laser.setParameter(LaserParameters.OP_WAVELENGTH, 632.991370);
                        laser.setParameter(LaserParameters.OP_MATCOMP, 1);
                        laser.setParameter(LaserParameters.OP_AIRCOMP, 1);
                        laser.Reset();
                        Console.WriteLine("The laser has been reset\n");
                        Console.WriteLine("The wavelength has been set to: " + laser.getParameter(LaserParameters.OP_WAVELENGTH) + "\n");
                        Console.WriteLine("The refractive index correction has been set to: " + laser.getParameter(LaserParameters.OP_AIRCOMP) + "\n");
                        Console.WriteLine("The material compensation has been set to: " + laser.getParameter(LaserParameters.OP_MATCOMP) + "\n");
                        String.Concat("The laser beam strength is ", b_strength.ToString(), "%\r\n");
                        Console.WriteLine("The Current laser position is: " + laser.ReadSample().ToString() + "\n");
                        break;
                    }
                }
                else if (line.Equals("4"))
                {
                    double result_ = 0.0;
                    serial = new EDM_ROCKY();

                    //while (true)
                    //{
                    //    System.Threading.Thread.Sleep(1000);
                    //    serial.ReadEDM(ref result_);
                    //    Console.WriteLine(result_.ToString());
                    //}
                    break;
                }
                else if (line.Equals("5"))
                {
                    double result_ = 0.0;
                    reader = new ManualInstrumentReading();
                    manual_reading = true;
                    break;
                }
                else
                {
                    Console.WriteLine(LaserErrorMessage.NoDevicesConnected);
                    Console.WriteLine("\r\n");
                }
                        
                    
                

            }

           

            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 16;
                string host = Environment.MachineName;
                IPAddress[] localAddr = Dns.GetHostAddresses(host);

                int index2 = 0;
                foreach (IPAddress addr in localAddr)
                {
                    if (addr.AddressFamily == AddressFamily.InterNetwork)
                    {
                        break;
                    }
                    index2++;
                }

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr[index2], port);

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
                    try
                    {
                        // Loop to receive all the data sent by the client. 
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
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
                                        if (!serial.SetUpEDM())
                                        {
                                            Console.WriteLine("EDM setup returned error code:" + serial.GetLastError);

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
                                        laser.Reset();

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
                                            if (manual_reading)
                                            {
                                                reader.Read(ref result);
                                            }
                                            else
                                            {
                                                while (!serial.ReadEDM(ref result))
                                                {
                                                    Console.WriteLine("EDM setup returned error code:" + serial.GetLastError);
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
                                            population_variance = population_variance / (averaging-1);
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
                                                result = laser.ReadSample();

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
