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
        // Create a blank gamepad object.
        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        // Define motor ports on PDP.
        static int motorPort1 = 0;
        static int motorPort2 = 1;
        static int motorPort3 = 2;
        static int motorPort4 = 3;

        // Assign motors to talons.
        static TalonSRX motor1 = new TalonSRX(motorPort1);
        static TalonSRX motor2 = new TalonSRX(motorPort2);
        static TalonSRX motor3 = new TalonSRX(motorPort3);
        static TalonSRX motor4 = new TalonSRX(motorPort4);

        /*
        // Assign analog inputs to potentiometers used to control motors. Numbers correspond to motors.
        static AnalogInput motorControl1 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin3);
        static AnalogInput motorControl2 = new AnalogInput(CTRE.HERO.IO.Port1.Analog_Pin4);
        static AnalogInput motorControl3 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin3);
        static AnalogInput motorControl4 = new AnalogInput(CTRE.HERO.IO.Port8.Analog_Pin4);
        */
        
        static int analogInput1 = 0;
        static int analogInput2 = 0;
        static int analogInput3 = 0;
        static int analogInput4 = 0;
        

        // Assign gamepad buttons to each function. - Future Update


        // Assign digital input as a master run switch.
        static InputPort mainRun = new InputPort(CTRE.HERO.IO.Port3.Pin3, false, Port.ResistorMode.Disabled);

        // Assign digital inputs to motor lock switches.
        // These will be used to run motors as pairs if needed.
        static InputPort motorLock1 = new InputPort(CTRE.HERO.IO.Port3.Pin8, false, Port.ResistorMode.Disabled);
        static InputPort motorLock2 = new InputPort(CTRE.HERO.IO.Port3.Pin9, false, Port.ResistorMode.Disabled);

        // Assign digital inputs to motor lock switches.
        // These will be used to run motors as pairs if needed.
        static InputPort motorDirection1 = new InputPort(CTRE.HERO.IO.Port3.Pin4, false, Port.ResistorMode.Disabled);
        static InputPort motorDirection2 = new InputPort(CTRE.HERO.IO.Port3.Pin5, false, Port.ResistorMode.Disabled);
        static InputPort motorDirection3 = new InputPort(CTRE.HERO.IO.Port3.Pin6, false, Port.ResistorMode.Disabled);
        static InputPort motorDirection4 = new InputPort(CTRE.HERO.IO.Port3.Pin7, false, Port.ResistorMode.Disabled);

        // Assign PCM to its CAN bus port.
        static PneumaticControlModule PCM = new PneumaticControlModule(0);

        // Assign PDP to its CAN bus port.
        static PowerDistributionPanel PDP = new PowerDistributionPanel(0);

        // Assign digital inputs to solenoid toggles.  These will fire pistons as needed.
        static InputPort pistonFire1 = new InputPort(CTRE.HERO.IO.Port5.Pin3, false, Port.ResistorMode.Disabled);
        static InputPort pistonFire2 = new InputPort(CTRE.HERO.IO.Port5.Pin4, false, Port.ResistorMode.Disabled);
        static InputPort pistonFire3 = new InputPort(CTRE.HERO.IO.Port5.Pin5, false, Port.ResistorMode.Disabled);
        static InputPort pistonFire4 = new InputPort(CTRE.HERO.IO.Port5.Pin6, false, Port.ResistorMode.Disabled);
        
        public static void Main()
        {
            while (true)  // Run only while master switch is on.
            {
                bool runSwitch = mainRun.Read();  // Check that the run switch is still active.

                // Check if gamepad is attached.  If so, create it.
                if (null == _gamepad)
                {
                    _gamepad = new GameController(UsbHostDevice.GetInstance());
                }

                DriveMotors();  // Drive motors according to potentiometer inputs and lock switches.
                Pneumatics();  // Fire solenoids with buttons

                if (runSwitch && _gamepad != null)
                {
                    CTRE.Phoenix.Watchdog.Feed();
                }

                Thread.Sleep(20);  // Pause for 20ms.
            }
        }

        /**
         * If value is within 5% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref double value)
        {
            if (value < -0.05)
            {
                /* outside of deadband */
            }
            else if (value > +0.05)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 5% so zero it */
                value = 0;
            }
        }

        static void DriveMotors()
        {
            // Set motor directions from their switches. Switches read true if placed in reverse.
            motor1.SetInverted(motorDirection1.Read());
            motor2.SetInverted(motorDirection2.Read());
            motor3.SetInverted(motorDirection3.Read());
            motor4.SetInverted(motorDirection4.Read());

            /*
            // Read motor speeds as percentages from potentiometers.
            double motorInput1 = motorControl1.Read();
            double motorInput2 = motorControl2.Read();
            double motorInput3 = motorControl3.Read();
            double motorInput4 = motorControl4.Read();
            */

            // Read potentiometers connected to talons as integers from 0 to 1023.
            motor1.GetSensorCollection().GetAnalogIn(out analogInput1);
            motor2.GetSensorCollection().GetAnalogIn(out analogInput2);
            motor3.GetSensorCollection().GetAnalogIn(out analogInput3);
            motor4.GetSensorCollection().GetAnalogIn(out analogInput4);

            // Convert each potentiometer input to a percentage between 0 and 1.
            double motorInput1 = analogInput1 / 1023;
            double motorInput2 = analogInput2 / 1023;
            double motorInput3 = analogInput3 / 1023;
            double motorInput4 = analogInput4 / 1023;
            

            // Run a 5% deadband to eliminate low-end drift.
            Deadband(ref motorInput1);
            Deadband(ref motorInput2);
            Deadband(ref motorInput3);
            Deadband(ref motorInput4);

            // Run motor 2 the same as 1 if its lock switch is on, otherwise run according to its pot.
            if (motorLock1.Read())
            {
                motorInput2 = motorInput1;
            }
            motor1.Set(ControlMode.PercentOutput, motorInput1);  // Run Motor 1 according to its pot.
            motor2.Set(ControlMode.PercentOutput, motorInput2);

            // Run motor 4 the same as 3 if its lock switch is on, otherwise run according to its pot.
            if (motorLock2.Read())
            {
                motorInput4 = motorInput3;
            }
            motor3.Set(ControlMode.PercentOutput, motorInput3);  // Run Motor 3 according to its pot.
            motor4.Set(ControlMode.PercentOutput, motorInput4);
        }

        static void Pneumatics()
        {
            // Set solenoid 0 to match button, solenoid 1 opposite
            PCM.SetSolenoidOutput(0, pistonFire1.Read());
            PCM.SetSolenoidOutput(1, !pistonFire1.Read());
            // Set solenoid 2 to match button, solenoid 3 opposite
            PCM.SetSolenoidOutput(2, pistonFire2.Read());
            PCM.SetSolenoidOutput(3, !pistonFire2.Read());
            // Set solenoid 4 to match button, solenoid 5 opposite
            PCM.SetSolenoidOutput(4, pistonFire3.Read());
            PCM.SetSolenoidOutput(5, !pistonFire3.Read());
            // Set solenoid 6 to match button, solenoid 7 opposite
            PCM.SetSolenoidOutput(6, pistonFire4.Read());
            PCM.SetSolenoidOutput(7, !pistonFire4.Read());

            /*
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
            }
            // Invert piston 2 state if its toggle button is pressed and is not locked.
            // Pistons are fired by solenoids that run as pairs, so 2 and 3 must be inverted.
            if (pistonToggle2.Read())  // Run if Piston 2 button is pressed.
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
            }
            // Invert piston 4 state if its toggle button is pressed and is not locked.
            // Pistons are fired by solenoids that run as pairs, so 6 and 7 must be inverted.
            if (pistonToggle4.Read())  // Run if Piston 4 button is pressed.
            {
                PCM.SetSolenoidOutput(6, !solenoidState6);  // Invert the state of solenoid 6
                PCM.SetSolenoidOutput(7, solenoidState6);  // Invert the state of solenoid 7
            }
            */
        }

        static void Display()
        {
            // Measure current current draw on each motor channel.
            float motorCurrent1 = PDP.GetChannelCurrent(motorPort1);
            float motorCurrent2 = PDP.GetChannelCurrent(motorPort2);
            float motorCurrent3 = PDP.GetChannelCurrent(motorPort3);
            float motorCurrent4 = PDP.GetChannelCurrent(motorPort4);

            // Measure power percentage for each motor.
            float motorPower1 = motor1.GetMotorOutputPercent();
            float motorPower2 = motor2.GetMotorOutputPercent();
            float motorPower3 = motor3.GetMotorOutputPercent();
            float motorPower4 = motor4.GetMotorOutputPercent();
        }
    }
}
