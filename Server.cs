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
    public abstract class Server
    {
        protected IPAddress address;
        protected int port = 0;
        protected bool reading_2nd_laser = false;
        protected SerialEDM serial_instance = null;
        protected TcpListener server = null;
        protected Laser lsr;
        protected ManualInstrumentReading man_istr_r;
        protected bool man_reading;
        protected double standard_deviation = 0.0;
        protected bool enabled = true;
        public IPAddress IP
        {
            set { address = value; }
            get { return address; }
        }
        public int Port
        {
            set { port = value; }
            get { return port; }
        }
        public bool Enabled
        {
            set { enabled = value; }
            get { return enabled; }
        }
        public bool ManualReading
        {
            set { man_reading = value; }
            get { return man_reading; }
        }
        public ManualInstrumentReading ManualInstrumentReading
        {
            set { man_istr_r = value; }
            get { return man_istr_r; }
        }
        public Laser LSR
        {
            set { lsr = value; }
            get { return lsr; }
        }
        public SerialEDM Serial_EDM
        {
            set { serial_instance = value; }
            get { return serial_instance; }
        }
        public bool ReadingSecondLaser
        {
            set { reading_2nd_laser = value; }
            get { return reading_2nd_laser; }
        }

        public abstract void Listen();
    }
}
