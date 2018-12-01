using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;


using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace Analog_Input_Test
{
    public class Program
    {
        /* create a talon */
        static TalonSRX talon = new TalonSRX(8);

        static int analogInput = 0;
        static double analogOutput = 0;

        public static void Main()
        {
            /* loop forever */
            while (true)
            {
                talon.GetSensorCollection().GetAnalogIn(out analogInput);
                analogOutput = analogInput / 1023;
                Debug.Print(analogInput.ToString());
                Debug.Print("\n");

                /* feed watchdog to keep Talon's enabled */
                Watchdog.Feed();
                /* run this task every 20ms */
                Thread.Sleep(20);
            }
        }
    }
}