using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Gpio;

namespace RPiTiLcd
{
    internal class TILCD
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

        public TILCD(byte ce, byte di, byte wr, byte rst, byte d0, byte d1, byte d2, byte d3, byte d4, byte d5, byte d6,
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

            _wr.Write(GpioPinValue.Low);
        }

        public void init(byte contrast)
        {
            _contrast = contrast;
            setWordLength(true);
            setCounterMode(false, true);
            setDisplayOn(true);
            setContrast(contrast);
            setPos(0, 0);
        }

        public void reset()
        {
            _rst.Write(GpioPinValue.Low);
            Task.Delay(100).Wait();
            _rst.Write(GpioPinValue.High);
            init(_contrast);
        }

        public void setDisplayOn(bool on)
        {
            writeBinaryValue(0, (byte) ("00000010".BinaryLiteralToByte() | (on ? 1 : 0)));
        }

        public void setCounterMode(bool y, bool up)
        {
            writeBinaryValue(0, (byte)("00000100".BinaryLiteralToByte() | (y ? B10 : 0) | (up ? 1 : 0));
        }

        public void setContrast(byte contrast)
        {
            writeBinaryValue(0, B11000000 | (contrast & B00111111));
        }

        public void setPos(byte x, byte y)
        {
            setX(x);
            setY(y);
        }

        public void setX(byte x)
        {
            writeBinaryValue(0, B00100000 | (x & B00011111));
        }

        public void setZ(byte z)
        {
            writeBinaryValue(0, B01000000 | (z & B00111111));
        }

        public void setY(byte y)
        {
            writeBinaryValue(0, B10000000 | (y & B00111111));
        }

        public void setScreenBytes(byte bytes [])
        {
            for (int x = 0; x < 12; x++)
            {
                setX(x);
                for (int y = 0; y < 64; y++)
                {
                    writeBinaryValue(1, bytes[x*64 + y]);
                }
            }
        }

        public void writeBinaryValue(byte di, byte value)
        {
            digitalWrite(_ce, false);
            digitalWrite(_di, di);
            digitalWrite(_d0, HIGH && (value & B00000001));
            digitalWrite(_d1, HIGH && (value & B00000010));
            digitalWrite(_d2, HIGH && (value & B00000100));
            digitalWrite(_d3, HIGH && (value & B00001000));
            digitalWrite(_d4, HIGH && (value & B00010000));
            digitalWrite(_d5, HIGH && (value & B00100000));
            digitalWrite(_d6, HIGH && (value & B01000000));
            digitalWrite(_d7, HIGH && (value & B10000000));
            digitalWrite(_ce, true);
        }

        public void setWordLength(bool eightBits)
        {
            writeBinaryValue(0, eightBits ? 1 : 0);
        }
    }
}