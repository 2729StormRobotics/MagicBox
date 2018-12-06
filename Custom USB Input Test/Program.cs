using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;
using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Phoenix.MotorControl;

namespace Custom_USB_Input_Test
{
    public class Program
    {
        /* the goal is to plug in a Xinput Logitech Gamepad or Xbox360 style controller */
        GameController _gamepad = new GameController(UsbHostDevice.GetInstance());
        GameControllerValues gv = new GameControllerValues();
        

        bool[] _buttons = new bool[16];

        float[] _sticks = new float[8];

        public void RunForever()
        {
            /* enable XInput, if gamepad is in DInput it will disable robot.  This way you can
             * use X mode for drive, and D mode for disable (instead of vice versa as the 
             * stock HERO implementation traditionally does). */
            UsbHostDevice.GetInstance(0).SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.BothDInputAndXInput);
            _gamepad.GetAllValues(ref gv);
            /* loop forever */
            while (true)
            {
                /* get buttons */
                for (uint i = 1; i < _buttons.Length; ++i)
                    _buttons[i] = _gamepad.GetButton(i);

                /* get sticks */
                for (uint i = 0; i < _sticks.Length; ++i)
                    _sticks[i] = _gamepad.GetAxis(i);

                /* yield for a bit, and track timeouts */
                System.Threading.Thread.Sleep(10);

                /* build line to print */
                StringBuilder sb = new StringBuilder();
                if (_gamepad.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    Watchdog.Feed();
                    sb.Append("Connected");
                }
                /*
                foreach (float stick in _sticks)
                {
                    sb.Append(Format(stick));
                    sb.Append(",");
                }

                sb.Append("-");
                for (uint i = 1; i < _buttons.Length; ++i)
                {
                    if (_buttons[i])
                    {
                        sb.Append("b" + i + ",");
                    }
                }
                */
                sb.Append("Axes: ");
                sb.Append(gv.axes);
                sb.Append(", Buttons: ");
                sb.Append(gv.btns);
                sb.Append(", VID: ");
                sb.Append(gv.vid);
                sb.Append(", PID: ");
                sb.Append(gv.pid);
                sb.Append(", vendor Spec F: ");
                sb.Append(gv.vendorSpecF);
                sb.Append(", vendor spec I: ");
                sb.Append(gv.vendorSpecI);

                /* print useful info */
                sb.AppendLine();
                Debug.Print(sb.ToString());
            }
        }
        /**
        * @param x arbitrary float
        * @return string version of x truncated to "X.XX" 
        */
        String Format(float x)
        {
            x *= 100;
            x = (int)x;
            x *= 0.01f;
            return "" + x;
        }

        public static void Main()
        {
            new Program().RunForever();
        }
    }
}