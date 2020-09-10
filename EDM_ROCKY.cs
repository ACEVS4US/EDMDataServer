using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;

namespace EDM_Data_Server
{
    public class EDM_ROCKY:SerialEDM
    {

        public override bool COMInit(string portname)
        {
            return COM_NullProc(portname);
        }

        public override bool SetUpEDM()
        {
            return true;
        }

        public override bool ReadEDM(ref double result)
        {
            //call do measure
            try
            {
                s_port.ReadTimeout = 10000;
                s_port.WriteTimeout = 10000;
                
                
                do
                {
                    s_port.DiscardInBuffer();
                    s_port.Write("$g*XX\r\n");   // start measurement
                    s_port.ReadLine(); // read g confirmation
                    String line = s_port.ReadLine(); // read measurement
                    result = ParseForResult(line);
                    s_port.Write("$s*XX\r\n");   // stop measuremnt
                } while (Double.IsNaN(result));
                
                
                return true;
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
                s_port = new SerialPort("COM10",115200,Parity.None,8);
                if(!s_port.IsOpen) s_port.Open();

                return true;

            }
            catch (IOException)
            {
                s_port.Close();
                return false;
            }
            catch (UnauthorizedAccessException)
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


        public double ParseForResult(string line)
        {
            if (line.Contains("$L")){
                string[] words = line.Split(',');
                if (words.Length > 3){
                    return Double.Parse(words[2]);
                }
              
            }
              return Double.NaN;
        }
    }
}
