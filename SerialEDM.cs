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
        public SerialEDM()
        {
            //get the names of all the com ports
            string[] ports = SerialPort.GetPortNames();

            foreach (string portname in ports)
            {
                if (COMInit(portname))
                {
                    break;   //this sets the correct serial port and the port is now open for comunication
                }
            }
            if (!s_port.IsOpen)
            {
                Console.WriteLine("Could not find a valid serial COM port");
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

        public abstract bool SetUpEDM();

        public abstract bool COMInit(string portname);

        public abstract bool ReadEDM(ref double result);
    }
}
