using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Text;

using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace MagicBox
{
    public class Program
    {
        static TalonSRX motor4 = new TalonSRX(4);
        static TalonSRX motor3 = new TalonSRX(3);
        static TalonSRX motor2 = new TalonSRX(2);
        static TalonSRX motor1 = new TalonSRX(1);

        static AnalogInput analogInput1 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin3);
        static AnalogInput analogInput2 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin4);
        static AnalogInput analogInput3 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);
        static AnalogInput analogInput4 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin4);


        public static void Main()
        {
            while (true)
            {
                DriveMotors();

                CTRE.Phoenix.Watchdog.Feed();

                Thread.Sleep(20);
            }
        }

        /**
         * If value is within 10% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref double value)
        {
            if (value < -0.1)
            {
                /* outside of deadband */
            }
            else if (value > +0.1)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }

        static void DriveMotors()
        {
            double input1 = analogInput1.Read();
            double input2 = analogInput2.Read();
            double input3 = analogInput3.Read();
            double input4 = analogInput4.Read();

            Deadband(ref input1);
            Deadband(ref input2);
            Deadband(ref input3);
            Deadband(ref input4);

            motor1.Set(ControlMode.PercentOutput, input1);
            motor2.Set(ControlMode.PercentOutput, input2);
            motor3.Set(ControlMode.PercentOutput, input3);
            motor4.Set(ControlMode.PercentOutput, input4);
        }
    }
}
