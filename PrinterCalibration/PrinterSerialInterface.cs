using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Globalization;

namespace PrinterCalibration
{
    public class PrinterSerialInterface
    {
        SerialPort port = null;
        private string comPort;

        public void OpenComm(string argcomPort)
        {
            comPort = argcomPort.ToUpper();
            port = new SerialPort(argcomPort,
            115200, Parity.None, 8, StopBits.One);

            try
            {
                Console.WriteLine("Openening port {0}\n", comPort);
                port.Open();
                Thread.Sleep(2000);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void CloseComm()
        {
            try
            {             
                port.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Extrude(double argExtrudeLen)
        {
            string extrudeLenString = Convert.ToString(argExtrudeLen);

            try
            {
                port.Write("M83 \r\n");
                string tmp = string.Format("G1 E{0} F50 \r\n", extrudeLenString);
                port.Write(tmp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public void HeatNozzle(string argNozzleTemp)
        {
            string tmp = string.Format("M104 S{0} \r\n", argNozzleTemp);
            port.Write(tmp);
        }

        public double GetEsteps()
        {
            double eSteps = 0;
            string eStepsString;

            port.Write("M503 \r\n");
            Thread.Sleep(1000);

            byte[] serial_data = new byte[port.BytesToRead];
            if (port.BytesToRead > 0)
            {
                port.Read(serial_data, 0, port.BytesToRead - 1);
                string data = Encoding.ASCII.GetString(serial_data);
                eStepsString = data.Substring(data.IndexOf("M92") + 27, 5);
                double.TryParse(eStepsString, NumberStyles.Number, CultureInfo.InvariantCulture, out eSteps);

            }
            return eSteps;
        }
    }
}
