using System;
using System.Threading;
using Microsoft.SPOT;
using System.Text;
using CTRE.Phoenix;
using CTRE.Phoenix.Controller;
using CTRE.Gadgeteer.Module;

namespace Custom_USB_Input_Test
{
    public class Program
    {
        static StringBuilder stringBuilder = new StringBuilder();

        static GameController _gamepad = null;
        GameControllerValues val = new GameControllerValues();

        static float[] axis = new float[9];

        public static void Main()
        {
            /* loop forever */
            while (true)
            {
                UsbHostDevice.GetInstance().SetSelectableXInputFilter(UsbHostDevice.SelectableXInputFilter.XInputDevices);

                if (null == _gamepad)
                {
                    _gamepad = new GameController(UsbHostDevice.GetInstance());
                    stringBuilder.AppendLine("Added gamepad!");
                }

                _gamepad.GetAllValues(val);

                stringBuilder.AppendLine(ValueType.)

                if (_gamepad.GetConnectionStatus() == UsbDeviceConnection.Connected)
                {
                    stringBuilder.AppendLine("Controller connected!");
                }
                else
                {
                    stringBuilder.AppendLine("Controller not connected!");
                }

                Debug.Print(stringBuilder.ToString());
                stringBuilder.Clear();
                /* feed watchdog to keep Talon's enabled */
                CTRE.Phoenix.Watchdog.Feed();
                /* run this task every 5s */
                Thread.Sleep(5000);
            }
        }
    }
}