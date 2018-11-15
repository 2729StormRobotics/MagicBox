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

        // Assign analog inputs to potentiometers used to control motors. Numbers correspond to motors.
        static AnalogInput motorControl1 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin3);
        static AnalogInput motorControl2 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin4);
        static AnalogInput motorControl3 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);
        static AnalogInput motorControl4 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin4);

        // Assign digital input as a master run switch.
        static InputPort mainRun = new InputPort(CTRE.HERO.IO.Port5.Pin3, false, Port.ResistorMode.Disabled);

        // Assign digital inputs to motor lock switches.
        // These will be used to run motors as pairs if needed.
        static InputPort motorLock1 = new InputPort(CTRE.HERO.IO.Port1.Pin6, false, Port.ResistorMode.Disabled);
        static InputPort motorLock2 = new InputPort(CTRE.HERO.IO.Port8.Pin6, false, Port.ResistorMode.Disabled);

        // Assign PCM to its CAN bus port.
        static PneumaticControlModule PCM = new PneumaticControlModule(0);

        // Assign digital inputs to solenoid toggles.  These will fire pistons as needed.
        static InputPort pistonToggle1 = new InputPort(CTRE.HERO.IO.Port5.Pin4, false, Port.ResistorMode.Disabled);
        static InputPort pistonToggle2 = new InputPort(CTRE.HERO.IO.Port5.Pin5, false, Port.ResistorMode.Disabled);
        static InputPort pistonToggle3 = new InputPort(CTRE.HERO.IO.Port5.Pin6, false, Port.ResistorMode.Disabled);
        static InputPort pistonToggle4 = new InputPort(CTRE.HERO.IO.Port5.Pin7, false, Port.ResistorMode.Disabled);

        // Assign digital inputs to piston lock toggles.
        // These will be used to fire pistons as pairs if needed.
        static InputPort pistonLock1 = new InputPort(CTRE.HERO.IO.Port5.Pin8, false, Port.ResistorMode.Disabled);
        static InputPort pistonLock2 = new InputPort(CTRE.HERO.IO.Port5.Pin9, false, Port.ResistorMode.Disabled);

        public static void Main()
        {
            while (true)  // Run only while master switch is on.
            {
                bool runSwitch = mainRun.Read();  // Check that the run switch is still active.

                DriveMotors();  // Drive motors according to potentiometer inputs and lock switches.
                Pneumatics();  // Fire solenoids with buttons

                if (runSwitch)
                {
                    CTRE.Phoenix.Watchdog.Feed();
                }

                Thread.Sleep(20);  // Pause for 20ms.
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
            double motorInput1 = motorControl1.Read();
            double motorInput2 = motorControl2.Read();
            double motorInput3 = motorControl3.Read();
            double motorInput4 = motorControl4.Read();

            // Run a 10% deadband to eliminate low-end drift.
            Deadband(ref motorInput1);
            Deadband(ref motorInput2);
            Deadband(ref motorInput3);
            Deadband(ref motorInput4);

            motor1.Set(ControlMode.PercentOutput, motorInput1);  // Run Motor 1 according to its pot.

            // Run motor 2 the same as 1 if its lock switch is on, otherwise run according to its pot.
            if (motorLock1.Read())
            {
                motor2.Set(ControlMode.PercentOutput, motorInput1);
            }
            else
            {
                motor2.Set(ControlMode.PercentOutput, motorInput2);
            }
            

            motor3.Set(ControlMode.PercentOutput, motorInput3);  // Run Motor 3 according to its pot.

            // Run motor 4 the same as 3 if its lock switch is on, otherwise run according to its pot.
            if (motorLock2.Read())
            {
                motor4.Set(ControlMode.PercentOutput, motorInput3);
            }
            else
            {
                motor4.Set(ControlMode.PercentOutput, motorInput4);
            }
        }

        static void Pneumatics()
        {
            // Check states of all solenoids
            bool solenoidState0 = PCM.GetSolenoidOutput(0);
            bool solenoidState1 = PCM.GetSolenoidOutput(1);
            bool solenoidState2 = PCM.GetSolenoidOutput(2);
            bool solenoidState3 = PCM.GetSolenoidOutput(3);
            bool solenoidState4 = PCM.GetSolenoidOutput(4);
            bool solenoidState5 = PCM.GetSolenoidOutput(5);
            bool solenoidState6 = PCM.GetSolenoidOutput(6);
            bool solenoidState7 = PCM.GetSolenoidOutput(7);

            // Invert piston 1 state if its toggle button is pressed.
            // Pistons are fired by solenoids that run as pairs, so 0 and 1 must be inverted.
            if (pistonToggle1.Read())  // Run if Piston 1 button is pressed.
            {
                PCM.SetSolenoidOutput(0, !solenoidState0);  // Invert the state of solenoid 0
                PCM.SetSolenoidOutput(1, solenoidState0);  // Invert the state of solenoid 1
                if (pistonLock1.Read())  // Run if Piston 1-2 lock is on
                {
                    PCM.SetSolenoidOutput(2, !solenoidState2);  // Invert the state of solenoid 2
                    PCM.SetSolenoidOutput(3, solenoidState2);  // Invert the state of solenoid 3
                }
            }
            // Invert piston 2 state if its toggle button is pressed and is not locked.
            // Pistons are fired by solenoids that run as pairs, so 2 and 3 must be inverted.
            if (pistonToggle2.Read() && !pistonLock1.Read())  // Run if Piston 2 button is pressed.
            {
                PCM.SetSolenoidOutput(2, !solenoidState2);  // Invert the state of solenoid 2
                PCM.SetSolenoidOutput(3, solenoidState2);  // Invert the state of solenoid 3
            }
            // Invert piston 3 state if its toggle button is pressed.
            // Pistons are fired by solenoids that run as pairs, so 4 and 5 must be inverted.
            if (pistonToggle3.Read())  // Run if Piston 3 button is pressed.
            {
                PCM.SetSolenoidOutput(4, !solenoidState4);  // Invert the state of solenoid 4
                PCM.SetSolenoidOutput(5, solenoidState4);  // Invert the state of solenoid 5
                if (pistonLock2.Read())  // Run if Piston 3-4 lock is on
                {
                    PCM.SetSolenoidOutput(6, !solenoidState2);  // Invert the state of solenoid 6
                    PCM.SetSolenoidOutput(7, solenoidState2);  // Invert the state of solenoid 7
                }
            }
            // Invert piston 4 state if its toggle button is pressed and is not locked.
            // Pistons are fired by solenoids that run as pairs, so 6 and 7 must be inverted.
            if (pistonToggle2.Read() && !pistonLock2.Read())  // Run if Piston 4 button is pressed.
            {
                PCM.SetSolenoidOutput(6, !solenoidState6);  // Invert the state of solenoid 6
                PCM.SetSolenoidOutput(7, solenoidState6);  // Invert the state of solenoid 7
            }
        }
    }
}
