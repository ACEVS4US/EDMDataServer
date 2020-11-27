using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Threading;

namespace EDM_Data_Server
{
    public struct DI2002_Return_Codes
    {
        public const short E00 = 0;
        public const short E03 = 3;  
        public const short E12 = 12;  
        public const short E21 = 21; 
        public const short E52 = 52;
        public const short E53 = 53;  
        public const short E55 = 55;  
        public const short E56 = 56;  
        public const short E57 = 57;
        public const short E62 = 62; 
        public const short E70 = 70;
        public const short E71 = 71;
        public const short E72 = 72;
        public const short E73 = 73;
        public const short E74 = 74;
        public const short E75 = 75;
        public const short E76 = 76;
        public const short E77 = 77;
        public const short E78 = 78;
        public const short E79 = 79;
        public const short E80 = 80;
        public const short E81 = 81;
        public const short E82 = 82;
        public const short E83 = 83;
        public const short E84 = 84;
        public const short E85 = 85;
        public const short E86 = 86;
        public const short E87 = 87;
        public const short E88 = 88;
        public const short E89 = 89;
        public const short E90 = 90;
        public const short E91 = 91;
        public const short E92 = 92;
        public const short E93 = 93;
        public const short E94 = 94;
        public const short E95 = 95;
        public const short E96 = 96;
        public const short E97 = 97;
        public const short E98 = 98;
        public const short E99 = 99;
        public const short E100 = 100;
        
    }

    public class EDM_DI2002:SerialEDM
    {
        
        
       
        public EDM_DI2002(string port_, ref bool init_ok):base(port_,ref init_ok)
        {
        }

        public override bool SetUpEDM()
        {
            //set EDM mode
            if (s_port.IsOpen)
            {
                try
                {
                    
                    s_port.Write("c\r\n");
                    if (!getStandardReturn(500)) throw new IOException();
                    //acknowledge
                    Thread.Sleep(200);
                    s_port.Write("?\r\n");

                    return true;
                }
                catch (IOException)
                {
                    s_port.Close();
                    last_error = "ERROR: Expected a ? return character and didn't get it";
                    return false; //incorrect character returned
                }
            }

            else
            {
                last_error = "Error: Serial port closed on EDM setup";
                return false;  //edm setup failed
            }
            
        }

        public override bool ReadEDM(ref double result)
        {
            

            //not if the port is closed
            if (!s_port.IsOpen)
            {
                last_error = "Error: Serial port closed on ReadEDM";
                return false;
            }

            else
            {
                try
                {
                    
                    //call do measure
                    s_port.Write("l\r\n");

                    s_port.ReadTimeout = 60000;
                    string line = s_port.ReadLine();
                    s_port.ReadTimeout = 1000;

                    short val = parseForReturnCode(line);
                    last_error = processReturnCode(val);
                    if (val == 0)
                    {

                        //if (!getStandardReturn(500)) throw new IOException();
                        //acknowledge
                        s_port.Write("?\r\n");
                        

                        result = ParseForResult(line);
                        return true;
                    }
                    else return false;  //failed measure
                }
                catch (TimeoutException)
                {
                    result = -1;
                    last_error = "EDM Read timeout check EDM alignment";
                    return false;
                }
            }
        }


        //turn the edm on
        public override bool COMInit(string portname)
        {

            try
            {
                //create a new serial port
                s_port = new SerialPort();

                s_port.PortName = portname;

                s_port.BaudRate = 2400;
                s_port.Parity = Parity.Even;
                s_port.DataBits = 7;
                s_port.StopBits = StopBits.One;
                s_port.Handshake = Handshake.XOnXOff;
                s_port.RtsEnable = false;
                s_port.DtrEnable = false;
                //s_port.WriteBufferSize = 1000;
                s_port.ReadTimeout = 1000;
                s_port.WriteTimeout = 1000;
                s_port.Open();
                s_port.DiscardInBuffer();

                //clear any errors
                s_port.Write("c\r\n");
                if (!getStandardReturn(500)) throw new IOException();

                //acknowledge
                s_port.Write("?\r\n");
                Thread.Sleep(200);
                s_port.Write("a\r\n");
                if (!getStandardReturn(500)) throw new IOException();

                Thread.Sleep(200);
                //acknowledge
                s_port.Write("?\r\n");



                return true;



            }
            catch (IOException)
            {
                s_port.Close();
                return false;
            }
            catch (TimeoutException)
            {
                s_port.Close();
                return false;
            }
            catch (AccessViolationException)
            {
                Console.WriteLine("Serial Port already open");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("Serial Port already open");
                return false;
            }
        }

        public void Write(string command)
        {
            s_port.Write(command);
        }

        public string processReturnCode(short return_code)
        {
           
                switch (return_code)
                {
                    case DI2002_Return_Codes.E00:
                        return "No Error";
                    case DI2002_Return_Codes.E03:
                        return "Invalid Input";
                    case DI2002_Return_Codes.E12:
                        return "Battery Voltage Too Low";
                    case DI2002_Return_Codes.E21:
                        return "Parity Error";
                    case DI2002_Return_Codes.E52:
                        return "Temperature Inside Instrument Too High or Too Low";
                    case DI2002_Return_Codes.E53:
                        return "Temperature Inside Instrument Too High or Too Low";
                    case DI2002_Return_Codes.E55:
                        return "Interference during measurement i.e. no return signal, interuption of beam, excessive air turbulence";
                    case DI2002_Return_Codes.E56:
                        return "DIL Mode; difference to last measurement too great.";
                    case DI2002_Return_Codes.E57:
                        return "Measurement below ambiguiity";
                    case DI2002_Return_Codes.E62:
                        return "Invalid WI";
                    case DI2002_Return_Codes.E100:
                        return "Unknown Error";
              
                    default: return "System Error";

                }
            
            
        }
        public short parseForReturnCode(string parseme)
        {

            if (parseme.Contains("@E"))
            {
                if (!ClearError())
                {
                    return 100;
                }

                //the error code should be at the index of @ +3
                string return_code = parseme.Substring(parseme.IndexOf('@') + 3, 2);
                try
                {
                    short result = Convert.ToInt16(return_code);
                    return result;
                }
                catch (FormatException)
                {
                    return 100;
                }
            }
            else return 0;
        }
        public bool ClearError()
        {
            //clear the error
            s_port.Write("c\r\n");

            return getStandardReturn(500);
        }

        public bool getStandardReturn(int timeout)
        {
            int t_zero = Environment.TickCount;

            while (Environment.TickCount < t_zero + timeout)
            {
                string line;
                line = s_port.ReadLine();
                if (line.Contains('?')) return true;
            }
            
            return false;
        }

        public double ParseForResult(string line)
        {
            if (line.Contains("31"))
            {
                string substring = line.Substring(line.IndexOf("31") + 6, 9);
                substring = substring.Insert(5, ".");
                double result = Convert.ToDouble(substring);
                
                return result;
            }


            else return -1.1;
        }


    }
}
