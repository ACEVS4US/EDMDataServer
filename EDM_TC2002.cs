using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;

namespace EDM_Data_Server
{
    //see chapter 16.2 of the GEOCOM reference manual

    

    public struct Return_Codes
    {
        public const ushort RC_OK = 0;
        public const ushort RC_UNDEFINED = 1;
        public const ushort RC_IVPARAM = 2;
        public const ushort RC_IVRESULT = 3;
        public const ushort RC_FATAL = 4;
        public const ushort RC_NOT_IMPL = 5;
        public const ushort RC_TIME_OUT = 6;
        public const ushort RC_SET_IMCOMPL = 7;
        public const ushort RC_ABORT = 8;
        public const ushort RC_NOMEMORY = 9;
        public const ushort RC_NOTINIT = 10;
        public const ushort RC_SHUT_DOWN = 12;
        public const ushort RC_SYSBUSY = 13;
        public const ushort RC_HWFAILURE = 14;
        public const ushort RC_ABORT_APPL = 15;
        public const ushort RC_LOW_POWER = 16;
        public const ushort NO_RETURN_VALUE = 255;
    }
        

    public class EDM_TC2002:SerialEDM
    {
        
       
       
        public EDM_TC2002()
        {
            
            
        }

        public override bool COMInit(string portname)
        {
            return COM_NullProc(portname);
        }

        public override bool SetUpEDM()
        {
            //set EDM mode
            if (s_port.IsOpen)
            {
                s_port.WriteLine("%R1Q,2020:1\r\n");
            }
            else return false;

            string line;
            try
            {
                line = s_port.ReadLine();
            }
            catch (TimeoutException)
            {
                return false;
            }
            int val = parseForReturnCode(line);
            last_error = processReturnCode((ushort)val);
            Console.WriteLine(last_error);

            if (val == 0) return true;

            else return false;  //edm setup failed
            
        }

        public override bool ReadEDM(ref double result)
        {
            //call do measure
            try
            {
                s_port.WriteLine("%R1Q,2008:1,1\r\n");
                string line = s_port.ReadLine();
                int val = parseForReturnCode(line);
                last_error = processReturnCode((ushort)val);
                Console.WriteLine(last_error);

                if (val == 0)
                {
                    s_port.WriteLine("%R1Q,2108:3000,0\r\n");
                    line = s_port.ReadLine();
                    val = parseForReturnCode(line);
                    last_error = processReturnCode((ushort)val);
                    Console.WriteLine(last_error);
                }
                else return false;  //do measure error

                if (val == 0)
                {
                    result = ParseForResult(line);
                    return true;
                }
                else return false;  //getsimplemeaserror
            }
            catch (TimeoutException)
            {
                return false;
            }
        }

        public bool COM_NullProc(string portname)
        {

            try
            {
                //create a new serial port
                s_port = new SerialPort(portname,19200,Parity.None,8);
                s_port.Open();
                s_port.Write("\n%R1Q,1010:ON\r\n");
                s_port.Write("\n%R1Q,0:\r\n");

                //We expect to get back "%R1P, 0, 0:RC"
                s_port.ReadTimeout = 1000;
                string line = s_port.ReadLine();
                int val = parseForReturnCode(line);
                last_error = processReturnCode((ushort)val);

                if (val == 0)
                {
                    return true;
                }
                else return false;

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
        }

        public void Write(string command)
        {
            s_port.Write(command);
        }

        public string processReturnCode(ushort return_code)
        {
            switch (return_code)
            {

                case Return_Codes.RC_OK:
                    return "Function Successfully Completed";
                case Return_Codes.RC_UNDEFINED:
                    return "Unknown error, result unspecified";
                case Return_Codes.RC_IVPARAM:
                    return "Invalid parameter detected. Result unspecified";
                case Return_Codes.RC_IVRESULT:
                    return "Invalid result";
                case Return_Codes.RC_FATAL:
                    return "Fatal error";
                case Return_Codes.RC_NOT_IMPL:
                    return "Not implemented yet";
                case Return_Codes.RC_TIME_OUT:
                    return "Function execution timed out.  Result unspecified.";
                case Return_Codes.RC_SET_IMCOMPL:
                    return "Parameter setup for subsystem is incomplete";
                case Return_Codes.RC_ABORT:
                    return "Function execution has been aborted";
                case Return_Codes.RC_NOMEMORY:
                    return "Fatal error, not enough memory";
                case Return_Codes.RC_NOTINIT:
                    return "Fatal error, subsystem non initialised";
                case Return_Codes.RC_SHUT_DOWN:
                    return "Subsystem is down";
                case Return_Codes.RC_SYSBUSY:
                    return "System busy/already in use of another preocess, cannot execute function";
                case Return_Codes.RC_HWFAILURE:
                    return "Fatal error - hardware failure";
                case Return_Codes.RC_ABORT_APPL:
                    return "Execution of application has been aborted (Shift-ESC)";
                case Return_Codes.RC_LOW_POWER:
                    return "Operation aborted - insufficient power supply level";
                default: return "ERROR code not specified";
                    
            }
        }
        public int parseForReturnCode(string parseme)
        {

            int index_of_colon = parseme.IndexOf(':');
            string return_substring = parseme.Substring(index_of_colon + 1);

            if (index_of_colon == -1)
            {
                return 255;
            }

            int return_value=0;
            int base10 = 1;
            bool has_return_value = false;
            foreach (char c in return_substring)
            {
                ushort c_ = (ushort) c;

                //check each character to make sure they are numeric
                if ((c_ >= 32 && c_ <= 57))
                {
                    has_return_value = true;
                    return_value = (return_value*base10) + c_;
                    base10 = base10*10;
                }
                else if (!(c_ >= 32 && c_ <= 57) && has_return_value)
                {
                    break;  //break if we've stopped getting numeric characters
                }
            }

            if (has_return_value)
            {
                return return_value;
            }
            else
            {
                return 255;
            }
        }

        public double ParseForResult(string line)
        {
            
            char[] delimeter = new char[1];
            delimeter[0] = ',';
            int comma_count=0;
            
            foreach(char c in line){
                if(c==','){
                    comma_count++;
                }
            }

            if (comma_count == 0)
            {
                return 0.0;
            }

            //find the first comma if it exists
            string[] split_up_by_commas = line.Split(delimeter);
            
            double result=0.0;

            try
            {
                result = Convert.ToDouble(split_up_by_commas[comma_count]);
            }
            catch (FormatException)
            {
                return 0.0;
            }

            return result;
        }
    }
}
