using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace TiLcdTest
{
    internal static class Utils
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

        public static void Swap<T>(ref T lhs, ref T rhs)
        {
            var temp = lhs;
            lhs = rhs;
            rhs = temp;
        }

        public static bool[][] ToBoolArray(this byte[] array)
        {
            var ret = new bool[array.Length][];

            var n = 0;
            foreach (var b in array)
            {
                ret[n] = new[]
                {
                    (b & 128) == 128,
                    (b & 64) == 64,
                    (b & 32) == 32,
                    (b & 16) == 16,
                    (b & 8) == 8,
                    (b & 4) == 4,
                    (b & 2) == 2,
                    (b & 1) == 1
                };

                n++;
            }

            return ret;
        }

        public static void Delay(this int delay)
        {
            Task.Delay(delay).Wait();
        }
    }
}