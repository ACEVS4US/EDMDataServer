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
    class Program
    {
        static void Main(string[] args)
        {
            bool reading_2nd_laser = false;
            SerialEDM serial = null;
            bool manual_reading = false;
            ManualInstrumentReading reader = null;
            Laser laser = null;
            EthernetServer server1 = new EthernetServer();
            WifiServer server2 = new WifiServer();

            //Get user input on the type of EDM they want to use.
            Console.WriteLine("Select the type of instrument you are using:\r\n1   TC2002\r\n2   DI2002\r\n3   HP5519A\r\n4   Rocky\r\n5   Leica Disto D810 Touch\r\n");
            while (true)
            {
                string line = Console.ReadLine();
                bool valid_line = false;
                bool initok = false;

                if (line.Equals("1"))
                {
                    string lin = "";

                    while (true)
                    {
                        Console.WriteLine("Enter the COM Port the EDM is connected to e.g COM11");
                        lin = Console.ReadLine();
                        if (lin.Contains("COM") && lin.Length > 3)
                        {
                            string sub = lin.Substring(3, lin.Length - 3);
                            int num = 0;
                            if (int.TryParse(sub, out num))
                            {
                                //The user has correctly entered the COM string, now let's test if it is a valid connection
                                serial = new EDM_TC2002(lin, ref initok);
                                if (initok)
                                {
                                    Console.WriteLine("Successfully connected to TC2002");
                                    break;
                                }
                                else Console.WriteLine("Connection Unsuccessful");
                            }
                        }
                    }
                    break;
                }

                else if (line.Equals("2"))
                {
                    string lin = "";
                    while (true)
                    {
                        Console.WriteLine("Enter the COM Port the EDM is connected to e.g COM11");

                        lin = Console.ReadLine();
                        if (lin.Contains("COM") && lin.Length > 3)
                        {
                            string sub = lin.Substring(3, lin.Length - 3);
                            int num = 0;
                            if (int.TryParse(sub, out num))
                            {
                                //The user has correctly entered the COM string, now let's test if it is a valid connection
                                serial = new EDM_DI2002(lin, ref initok);
                                if (initok)
                                {
                                    Console.WriteLine("Successfully connected to DI2002");
                                    break;
                                }
                                else Console.WriteLine("Connection Unsuccessful");
                            }
                        }
                    }
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
                   
                    serial = new EDM_ROCKY("", ref initok);
                    break;
                }
                else if (line.Equals("5"))
                {
                    
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

            server1.IP = IPAddress.Parse("192.168.1.1");
            server1.Port = 16;
            server1.Serial_EDM = serial;
            server1.ReadingSecondLaser = reading_2nd_laser;
            server1.ManualInstrumentReading = reader;
            server1.ManualReading = manual_reading;
            server1.LSR = laser;

            //listen over the ethernet adapter for a client connection;
            Thread ethernet_listener = new Thread(new ThreadStart(server1.Listen));
            ethernet_listener.Start();

            server2.IP = IPAddress.Parse("192.168.1.1");
            server2.Port = 16;
            server2.Serial_EDM = serial;
            server2.ReadingSecondLaser = reading_2nd_laser;
            server2.ManualInstrumentReading = reader;
            server2.ManualReading = manual_reading;
            server2.LSR = laser;

            //listen over the ethernet adapter for a client connection;
            Thread wifi_listener = new Thread(new ThreadStart(server2.Listen));
            wifi_listener.Start();

            while (true)
            {
                Thread.Sleep(1000);
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
