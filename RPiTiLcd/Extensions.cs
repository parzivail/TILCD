using System;
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

        public static byte[] Range(this byte[] array, int start, int end)
        {
            var r = new byte[end - start];
            for (var i = start; i < end; i++)
                r[i - start] = array[i];
            return r;
        }

        public static byte[] GetPixelRow(this byte[,] array, int row)
        {
            const int d2 = 64;
            const int doubleSize = 8;
            
            var target = new byte[d2];
            
            Buffer.BlockCopy(array, doubleSize * d2 * row, target, 0, doubleSize * d2);

            return target;
        }
    }
}