using System;
using System.IO.Ports;
using System.Threading;
using System.Text;
using System.Globalization;

namespace PrinterCalibration
{
    class Program
    {
        static void Main(string[] args)
        {
            bool runningState = true;
            int programState = 0;
            string selectedPortInputString = string.Empty;

            double currentESteps = 0;
            double newEsteps = 0;
            double lenToExtrude = 0;
            double lenToMark = 0;
            double lenToMarkAfterExtruding = 0;

            PrinterSerialInterface printerInterface = new PrinterSerialInterface();
            Console.WriteLine("Extruder calibration app v0.1. by Martin P.\r\n");

            while (runningState)
            {
                switch (programState)
                {
                    case 0: // User select port
                        // Read possible serial ports
                        string[] ports = SerialPort.GetPortNames();

                        if (ports.Length > 0)
                        {

                            Console.WriteLine("The following serial ports were found:\r\n");

                            // Display each port name to the console.
                            int index = 0;
                            Console.WriteLine(" # | PORT NUM.");
                            Console.WriteLine("---------------");
                            foreach (string port in ports)
                            {
                                Console.WriteLine(" {0} : {1}", index, port);
                                index = index + 1;
                            }

                            Console.WriteLine("\r\n");
                            Console.Write("Select 3d printer port (# index from above table):");
                            string selectedPortInput = Console.ReadLine();

                            int indexPort;
                            int.TryParse(selectedPortInput, out indexPort);
                            Console.WriteLine("Selected port: {0}\n", ports[indexPort]);
                            selectedPortInputString = ports[indexPort];

                            programState = 1;
                        }
                        else
                        {
                            Console.WriteLine("No device connected to serial port!");
                            Console.WriteLine("Press enter to finish execution.");
                            Console.ReadLine();
                            programState = 100;
                        }
                        break;

                    case 1: // Open selected port
                        Console.WriteLine("Connecting to 3d printer");
                        printerInterface.OpenComm(selectedPortInputString);
                        programState = 2;
                        break;

                    case 2: // Read currrent parameters and preheat nozzle
                        currentESteps = printerInterface.GetEsteps();
                        
                        Console.WriteLine("Current E steps: E{0}\n", Convert.ToString(currentESteps, CultureInfo.InvariantCulture));
                        Console.Write("Select nozzle temp:");
                        printerInterface.HeatNozzle(Console.ReadLine());
                        Console.WriteLine("Press enter when finished heating the nozzle.");
                        Console.ReadLine();

                        programState = 3;
                        break;

                    case 3: // Ekstrude fillament
                        Console.Write("Enter reference mark distance:");
                        string lenToMarkString = Console.ReadLine();
                        double.TryParse(lenToMarkString, out lenToMark);

                        Console.Write("Enter extrude distance distance:");
                        string lenToExtrudeString = Console.ReadLine();
                        double.TryParse(lenToExtrudeString, out lenToExtrude);

                        Console.WriteLine("Extruding!");
                        printerInterface.Extrude(lenToExtrude);

                        Console.WriteLine("Press enter when finished extruding");
                        Console.ReadLine();
                        programState = 4;
                        break;

                    case 4: // Enter new distance to the mark and calculate new E steps
                        Console.Write("Enter new distnace to the mark:");
                        string lenToMarkNewString = Console.ReadLine();
                        double.TryParse(lenToMarkNewString, NumberStyles.Number, CultureInfo.InvariantCulture,out lenToMarkAfterExtruding);

                        double deltaLen = lenToMark - lenToMarkAfterExtruding;
                        double corrK = lenToExtrude / deltaLen;
                        newEsteps = currentESteps * corrK;
                        currentESteps = newEsteps;

                        Console.WriteLine("New value of ESteps: {0}", newEsteps);
                        Console.WriteLine("Warning: you need to set new ESteps manually!");
                        Console.WriteLine("Do you want to repeat the calibration? [y/n]", newEsteps);

                        if ((Console.ReadLine().ToLower()).Equals("y"))
                        {
                            programState = 3;
                        }
                        else
                        {
                            programState = 99;
                        }
                        break;

                    case 99: // End the program
                        printerInterface.CloseComm();
                        programState = 100;
                        break;

                    case 100:
                        runningState = false;
                        Console.WriteLine("Ending the application");
                        Console.WriteLine("Bye!\n");
                        Console.WriteLine("Press enter to close the app");
                        Console.ReadLine();
                        break;
                }
            }
 
        }
    }
}

