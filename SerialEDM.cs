using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;

namespace EDM_Data_Server
{
    public abstract class SerialEDM
    {

        public static SerialPort s_port;
        protected string last_error = "ERROR code not specified";
        public SerialEDM(string port_,ref bool init_ok)
        {
            //get the names of all the com ports
            

            if (port_.Contains("COM"))
            {
                //The com port has been specified by the user
                if (COMInit(port_))
                {
                    init_ok = true;
                }
              
            }
            else
            {
                string[] ports = SerialPort.GetPortNames();
                //In this case the user had not specified a valid com port
                foreach (string portname in ports)
                {
                    if (COMInit(portname))
                    {
                        init_ok = true;
                        break;   //this sets the correct serial port and the port is now open for comunication
                    }
                }
            }
        }


        public string GetLastError
        {
            get
            {
                return last_error;
            }
            set
            {
                last_error = value;
            }
        }
        public bool PortOpen()
        {
            if (s_port.IsOpen) return true;
            else return false;
        }

        public abstract bool SetUpEDM();

        public abstract bool COMInit(string portname);

        public abstract bool ReadEDM(ref double result);
    }
}
