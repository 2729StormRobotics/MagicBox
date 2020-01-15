using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;

using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;
using CTRE.Phoenix.MotorControl.CAN;

namespace MagicBoxGamepad
{
    public class Program
    {
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

        static CTRE.Phoenix.Controller.GameController _gamepad = null;

        // Assign PCM to its CAN bus port.
        static PneumaticControlModule PCM = new PneumaticControlModule(0);

        public static void Main()
        {
            PCM.ClearAllPCMStickyFaults();

            PCM.StartCompressor();

            PCM.SetSolenoidOutput(0, true);
            PCM.SetSolenoidOutput(1, false);
            PCM.SetSolenoidOutput(2, true);
            PCM.SetSolenoidOutput(3, false);
            PCM.SetSolenoidOutput(4, true);
            PCM.SetSolenoidOutput(5, false);
            PCM.SetSolenoidOutput(6, true);
            PCM.SetSolenoidOutput(7, false);

            /* loop forever */
            while (true)
            {
                /* drive robot using gamepad */
                Drive();
                
                /* control pneumatics with gamepad */
                GamepadPneumatics();
                
                /* feed watchdog to keep Talon's enabled */
                CTRE.Phoenix.Watchdog.Feed();

                /* run this task every 20ms */
                Thread.Sleep(20);
            }
        }

        /**
         * If value is within 5% of center, clear it.
         * @param value [out] floating point value to deadband.
         */
        static void Deadband(ref float value)
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

        static void Drive()
        {
            if (null == _gamepad)
                _gamepad = new GameController(UsbHostDevice.GetInstance());

            bool motorInverted1 = motor1.GetInverted();
            bool motorInverted2 = motor2.GetInverted();
            bool motorInverted3 = motor3.GetInverted();
            bool motorInverted4 = motor4.GetInverted();

            // Read button press

            int idx = GetFirstButton(_gamepad);

            // Check if button pressed is within motor-inverting range. Otherwise, ignore it.
            
            if (idx > 4)
            {
                switch (idx)
                {
                    case 5:
                        motor1.SetInverted(!motorInverted1);
                        break;
                    case 7:
                        motor2.SetInverted(!motorInverted2);
                        break;
                    case 6:
                        motor3.SetInverted(!motorInverted3);
                        break;
                    case 8:
                        motor4.SetInverted(!motorInverted4);
                        break;
                }
            }
            
            // Read motor speeds from gamepad analog stick y-axes.
            float motorInputLeft = -1*_gamepad.GetAxis(1);
            float motorInputRight = -1*_gamepad.GetAxis(5);

            // Run a 5% deadband to eliminate low-end drift.
            Deadband(ref motorInputLeft);
            Deadband(ref motorInputRight);

            // Drive motors as pairs according to the left or right analog sticks
            motor1.Set(ControlMode.PercentOutput, motorInputLeft);
            motor2.Set(ControlMode.PercentOutput, motorInputLeft);
            motor3.Set(ControlMode.PercentOutput, motorInputRight);
            motor4.Set(ControlMode.PercentOutput, motorInputRight);
        }

        static int GetFirstButton(GameController gamepad)
        {
            Thread.Sleep(50);
            for (uint i = 1; i < 16; ++i)
            {
                if (gamepad.GetButton(i))
                    return (int)i;
            }
            return -1;
        }

        static void GamepadPneumatics()
        {
            PCM.ClearAllPCMStickyFaults();

            // Check states of all solenoids
            bool solenoidState0 = PCM.GetSolenoidOutput(0);
            bool solenoidState1 = PCM.GetSolenoidOutput(1);
            bool solenoidState2 = PCM.GetSolenoidOutput(2);
            bool solenoidState3 = PCM.GetSolenoidOutput(3);
            bool solenoidState4 = PCM.GetSolenoidOutput(4);
            bool solenoidState5 = PCM.GetSolenoidOutput(5);
            bool solenoidState6 = PCM.GetSolenoidOutput(6);
            bool solenoidState7 = PCM.GetSolenoidOutput(7);

            // Read button press
            int idx = GetFirstButton(_gamepad);

            // Check if button pressed is within piston-firing range. Otherwise, ignore it.
            
            switch (idx)
			{
				case 1:
					PCM.SetSolenoidOutput(0, !solenoidState0);  // Invert the state of solenoid 0
					PCM.SetSolenoidOutput(1, solenoidState0);  // Invert the state of solenoid 1
					break;
				case 2:
					PCM.SetSolenoidOutput(2, !solenoidState2);  // Invert the state of solenoid 2
					PCM.SetSolenoidOutput(3, solenoidState2);  // Invert the state of solenoid 3
					break;
				case 3:
					PCM.SetSolenoidOutput(4, !solenoidState4);  // Invert the state of solenoid 4
					PCM.SetSolenoidOutput(5, solenoidState4);  // Invert the state of solenoid 5
					break;
				case 4:
					PCM.SetSolenoidOutput(6, !solenoidState6);  // Invert the state of solenoid 6
					PCM.SetSolenoidOutput(7, solenoidState6);  // Invert the state of solenoid 7
					break;
                default:
                    break;
			}
        }
    }
}