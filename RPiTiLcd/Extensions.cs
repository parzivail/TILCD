using Windows.Devices.Gpio;

namespace RPiTiLcd
{
    internal static class Extensions
    {
        public static void Write(this GpioPin pin, bool value)
        {
            pin.Write(value ? GpioPinValue.High : GpioPinValue.Low);
        }
        public static void Write(this GpioPin pin, int value)
        {
            pin.Write(value == 0 ? GpioPinValue.Low : GpioPinValue.High);
        }
    }
}