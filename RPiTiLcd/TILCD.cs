using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

namespace RPiTiLcd
{
    internal class TiLcd
    {
        private readonly GpioPin _ce;
        private readonly GpioPin _di;
        private readonly GpioPin _wr;
        private readonly GpioPin _rst;
        private readonly GpioPin _d0;
        private readonly GpioPin _d1;
        private readonly GpioPin _d2;
        private readonly GpioPin _d3;
        private readonly GpioPin _d4;
        private readonly GpioPin _d5;
        private readonly GpioPin _d6;
        private readonly GpioPin _d7;
        private byte _contrast;

        private bool[,] _graphicsBuffer = new bool[96, 64];

        public TiLcd(byte ce, byte di, byte wr, byte rst, byte d0, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6,
            byte d7)
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
                throw new ArgumentNullException("No GPIO found on device!");

            _ce = gpio.OpenPin(ce);
            _ce.SetDriveMode(GpioPinDriveMode.Output);
            _di = gpio.OpenPin(di);
            _di.SetDriveMode(GpioPinDriveMode.Output);
            _wr = gpio.OpenPin(wr);
            _wr.SetDriveMode(GpioPinDriveMode.Output);
            _rst = gpio.OpenPin(rst);
            _rst.SetDriveMode(GpioPinDriveMode.Output);

            _d0 = gpio.OpenPin(d0);
            _d0.SetDriveMode(GpioPinDriveMode.Output);
            _d1 = gpio.OpenPin(d1);
            _d1.SetDriveMode(GpioPinDriveMode.Output);
            _d2 = gpio.OpenPin(d2);
            _d2.SetDriveMode(GpioPinDriveMode.Output);
            _d3 = gpio.OpenPin(d3);
            _d3.SetDriveMode(GpioPinDriveMode.Output);
            _d4 = gpio.OpenPin(d4);
            _d4.SetDriveMode(GpioPinDriveMode.Output);
            _d5 = gpio.OpenPin(d5);
            _d5.SetDriveMode(GpioPinDriveMode.Output);
            _d6 = gpio.OpenPin(d6);
            _d6.SetDriveMode(GpioPinDriveMode.Output);
            _d7 = gpio.OpenPin(d7);
            _d7.SetDriveMode(GpioPinDriveMode.Output);

            _contrast = 48;

            _wr.Write(false);
        }

        public void Init(byte contrast)
        {
            _contrast = contrast;
            SetWordLength(true);
            SetCounterMode(CounterMode.XUp);
            SetDisplayState(DisplayState.On);
            SetContrast(contrast);
            SetPos(0, 0);
        }

        public void Reset()
        {
            _rst.Write(false);
            Task.Delay(100).Wait();
            _rst.Write(true);
            Init(_contrast);
        }

        public void SetDisplayState(byte state)
        {
            WriteBinaryValue(0, (byte) (TiCommand.DisplayStateSet | state));
        }

        public void SetCounterMode(byte mode)
        {
            WriteBinaryValue(0, (byte)(TiCommand.CounterModeSet | mode));
        }

        public void SetContrast(byte contrast)
        {
            WriteBinaryValue(0, (byte) (TiCommand.ContrastSet | (contrast & CommandMask.ContrastMask)));
        }

        public void SetPos(byte x, byte y)
        {
            SetX(x);
            SetY(y);
        }

        public void SetX(byte x)
        {
            // Display X is really the Y/Page buffer
            WriteBinaryValue(0, (byte) (TiCommand.YAddressSet | (x & CommandMask.YMask)));
        }

        public void SetZ(byte z)
        {
            WriteBinaryValue(0, (byte) (TiCommand.ZAddressSet | (z & CommandMask.ZMask)));
        }

        public void SetY(byte y)
        {
            // Display Y is really the X buffer
            WriteBinaryValue(0, (byte) (TiCommand.XAddressSet | (y & CommandMask.XMask)));
        }

        public void SetScreenBytes(bool[,] pixels)
        {
            for (byte x = 0; x < 12; x++)
            {
                SetX(x);
                for (byte y = 0; y < 64; y++)
                {
                    //WriteBinaryValue(1, (byte) (pixels[x, y] ? 1 : 0));
                }
            }
        }

        public void SetScreenBytes(byte[,] pixels)
        {
            for (byte x = 0; x < 96; x += 8)
            {
                SetX((byte) (x / 8));
                for (byte y = 0; y < 64; y++)
                {
                    byte n = 0;
                    var npxl = new List<byte>();
                    for (var nx = 0; nx < 8; nx++) npxl.Add(pixels[y, x + nx]);
                    foreach (var pixel in npxl)
                    {
                        n <<= 1;
                        n += pixel;
                    }
                    WriteBinaryValue(1, n);
                }
            }
        }

        private void WriteBinaryValue(byte di, byte value)
        {
            _ce.Write(false);
            _di.Write(di);
            _d0.Write(value & 1);
            _d1.Write(value & 2);
            _d2.Write(value & 4);
            _d3.Write(value & 8);
            _d4.Write(value & 16);
            _d5.Write(value & 32);
            _d6.Write(value & 64);
            _d7.Write(value & 128);
            _ce.Write(true);
        }

        private void SetWordLength(bool eightBits)
        {
            WriteBinaryValue(0, (byte)(TiCommand.WordLengthSet | (eightBits ? WordLength.EightBits : WordLength.SixBits)));
        }
    }
}