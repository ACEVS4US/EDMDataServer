using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDM_Data_Server
{
    class ManualInstrumentReading
    {
        private double result = 0.0;

        public ManualInstrumentReading()
        {

        }

        public void Read(ref double _result)
        {
            //get the user to enter a reading
            Console.WriteLine("Enter the reading as a numeric value!");
            while (true)
            {
                string line = Console.ReadLine();
                try
                {
                    result = Convert.ToDouble(line);
                    _result = result;
                    break;
                }
                catch(FormatException){
                    continue;
                }
            }
        }
    }
}
