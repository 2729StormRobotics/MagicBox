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
        // Assign motors to talons. They will be numbered and labeled according to PDP port.
        static TalonSRX motor1 = new TalonSRX(1);
        static TalonSRX motor2 = new TalonSRX(2);
        static TalonSRX motor3 = new TalonSRX(3);
        static TalonSRX motor4 = new TalonSRX(4);

        // Assign analog inputs to potentiometers used to control motors. Numbers correspond.
        static AnalogInput analogInput1 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin3);
        static AnalogInput analogInput2 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin4);
        static AnalogInput analogInput3 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);
        static AnalogInput analogInput4 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin4);

        // Assign digital input as a master run switch.
        static InputPort digitalInput0 = new InputPort(CTRE.HERO.IO.Port6.Pin3, false, Port.ResistorMode.Disabled);

        // Assign digital inputs to motor lock switches.
        // These will be used to run motors as pairs if needed.
        static InputPort digitalInput1 = new InputPort(CTRE.HERO.IO.Port6.Pin4, false, Port.ResistorMode.Disabled);
        static InputPort digitalInput2 = new InputPort(CTRE.HERO.IO.Port6.Pin5, false, Port.ResistorMode.Disabled);


        public static void Main()
        {
            bool runSwitch = digitalInput0.Read();  // Check if master run switch is on.

            while (runSwitch)  // Run only while master switch is on.
            {
                DriveMotors();  // Drive motors according to potentiometer inputs and lock switches.

                CTRE.Phoenix.Watchdog.Feed();

                Thread.Sleep(20);  // Pause for 20ms.

                runSwitch = digitalInput0.Read();  // Double check that the run switch is still active.
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
            // Read motor speeds as percentages from potentiometers.
            double motorInput1 = analogInput1.Read();
            double motorInput2 = analogInput2.Read();
            double motorInput3 = analogInput3.Read();
            double motorInput4 = analogInput4.Read();

            // Run a 10% deadband to eliminate low-end drift.
            Deadband(ref motorInput1);
            Deadband(ref motorInput2);
            Deadband(ref motorInput3);
            Deadband(ref motorInput4);

            motor1.Set(ControlMode.PercentOutput, motorInput1);  // Run Motor 1 according to its pot.

            // Run motor 2 the same as 1 if its lock switch is on, otherwise run according to its pot.
            if (digitalInput1.Read())
            {
                motor2.Set(ControlMode.PercentOutput, motorInput1);
            }
            else
            {
                motor2.Set(ControlMode.PercentOutput, motorInput2);
            }
            

            motor3.Set(ControlMode.PercentOutput, motorInput3);  // Run Motor 3 according to its pot.

            // Run motor 4 the same as 3 if its lock switch is on, otherwise run according to its pot.
            if (digitalInput1.Read())
            {
                motor4.Set(ControlMode.PercentOutput, motorInput3);
            }
            else
            {
                motor4.Set(ControlMode.PercentOutput, motorInput4);
            }
        }
    }
}
