using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace LcdDriver
{
    public class TiLcd
    {
        public enum BeginMode
        {
            None,
            Line,
            LineLoop,
            Fill
        }

        private readonly GpioPin _ce;
        private readonly List<Point> _currentPoints;
        private readonly GpioPin _d0;
        private readonly GpioPin _d1;
        private readonly GpioPin _d2;
        private readonly GpioPin _d3;
        private readonly GpioPin _d4;
        private readonly GpioPin _d5;
        private readonly GpioPin _d6;
        private readonly GpioPin _d7;
        private readonly GpioPin _di;
        private readonly GpioPin _rst;
        private readonly GpioPin _wr;

        public readonly bool[,] GraphicsBuffer = new bool[64, 96];
        private byte _contrast;

        private BeginMode _currentMode;

        private bool[,] _tempBuffer = new bool[64, 96];

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

            _currentMode = BeginMode.None;
            _currentPoints = new List<Point>();

            ClearScreenBuffer();
            ClearTempBuffer();
        }

        private void ClearScreenBuffer()
        {
            for (byte x = 0; x < 96; x++)
                for (byte y = 0; y < 64; y++)
                    GraphicsBuffer[y, x] = false;
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
            Utils.Write(_rst, false);
            Task.Delay(100).Wait();
            Utils.Write(_rst, true);
            Init(_contrast);
        }

        public void SetDisplayState(byte state)
        {
            WriteBinaryValue(0, (byte) (TiCommand.DisplayStateSet | state));
        }

        public void SetCounterMode(byte mode)
        {
            WriteBinaryValue(0, (byte) (TiCommand.CounterModeSet | mode));
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

        public void RefreshFromBuffer()
        {
            SetScreenBytes(GraphicsBuffer);
        }

        public void SetScreenBytes(bool[,] pixels)
        {
            for (byte x = 0; x < 96; x += 8)
            {
                SetX((byte) (x/8));
                for (byte y = 0; y < 64; y++)
                {
                    byte n = 0;
                    var npxl = new List<byte>();
                    for (var nx = 0; nx < 8; nx++) npxl.Add((byte) (pixels[y, x + nx] ? 1 : 0));
                    foreach (var pixel in npxl)
                    {
                        n <<= 1;
                        n += pixel;
                    }
                    WriteBinaryValue(1, n);
                }
            }
        }

        public bool SetPixel(int x, int y, bool value, bool refreshFromBuffer)
        {
            if ((x < 0) || (x >= 96) || (y < 0) || (y >= 64))
                return false;

            GraphicsBuffer[y, x] = value;

            if (refreshFromBuffer)
                RefreshFromBuffer();

            return true;
        }

        public bool SetPixel(int x, int y, bool value, ref bool[,] buffer)
        {
            if ((x < 0) || (x >= 96) || (y < 0) || (y >= 64))
                return false;

            buffer[y, x] = value;

            return true;
        }

        public void SetScreenBytes(byte[,] pixels)
        {
            for (byte x = 0; x < 96; x += 8)
            {
                SetX((byte) (x/8));
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
            Utils.Write(_ce, false);
            Utils.Write(_di, di);
            Utils.Write(_d0, value & 1);
            Utils.Write(_d1, value & 2);
            Utils.Write(_d2, value & 4);
            Utils.Write(_d3, value & 8);
            Utils.Write(_d4, value & 16);
            Utils.Write(_d5, value & 32);
            Utils.Write(_d6, value & 64);
            Utils.Write(_d7, value & 128);
            Utils.Write(_ce, true);
        }

        private void SetWordLength(bool eightBits)
        {
            WriteBinaryValue(0,
                (byte) (TiCommand.WordLengthSet | (eightBits ? WordLength.EightBits : WordLength.SixBits)));
        }

        /// <summary>
        ///     Plot the line from (x0, y0) to (x1, y1)
        /// </summary>
        /// <author>Jason Morley</author>
        /// <param name="x0">The start x</param>
        /// <param name="y0">The start y</param>
        /// <param name="x1">The end x</param>
        /// <param name="y1">The end y</param>
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Utils.Swap(ref x0, ref y0);
                Utils.Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Utils.Swap(ref x0, ref x1);
                Utils.Swap(ref y0, ref y1);
            }
            int dX = x1 - x0, dY = Math.Abs(y1 - y0), err = dX/2, ystep = y0 < y1 ? 1 : -1, y = y0;

            for (var x = x0; x <= x1; ++x)
            {
                if (!(steep ? SetPixel(y, x, true, false) : SetPixel(x, y, true, false))) return;
                err = err - dY;
                if (err >= 0) continue;
                y += ystep;
                err += dX;
            }
        }

        public void DrawLineToBuffer(ref bool[,] buffer, int x0, int y0, int x1, int y1)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                Utils.Swap(ref x0, ref y0);
                Utils.Swap(ref x1, ref y1);
            }
            if (x0 > x1)
            {
                Utils.Swap(ref x0, ref x1);
                Utils.Swap(ref y0, ref y1);
            }
            int dX = x1 - x0, dY = Math.Abs(y1 - y0), err = dX/2, ystep = y0 < y1 ? 1 : -1, y = y0;

            for (var x = x0; x <= x1; ++x)
            {
                if (!(steep ? SetPixel(y, x, true, ref buffer) : SetPixel(x, y, true, ref buffer))) return;
                err = err - dY;
                if (err >= 0) continue;
                y += ystep;
                err += dX;
            }
        }

        public void BeginDraw(BeginMode mode)
        {
            _currentMode = mode;
        }

        public void EndDraw()
        {
            RenderDrawnPoints(_currentMode, _currentPoints);
            _currentPoints.Clear();
            _currentMode = BeginMode.None;
        }

        private void RenderDrawnPoints(BeginMode currentMode, IReadOnlyList<Point> currentPoints)
        {
            if (currentPoints.Count < 2)
                return;

            Point last = null;

            foreach (var currentPoint in currentPoints)
            {
                if (last != null)
                    DrawLineToBuffer(ref _tempBuffer, last.X, last.Y, currentPoint.X, currentPoint.Y);
                last = currentPoint;
            }

            switch (currentMode)
            {
                case BeginMode.LineLoop:
                    DrawLineToBuffer(ref _tempBuffer, last.X, last.Y, currentPoints[0].X, currentPoints[0].Y);
                    break;
                case BeginMode.Fill:
                    DrawLineToBuffer(ref _tempBuffer, last.X, last.Y, currentPoints[0].X, currentPoints[0].Y);

                    var minY = currentPoints.Min(point => point.Y);
                    var maxY = currentPoints.Max(point => point.Y);

                    for (var y = 0; y < 64; y++)
                    {
                        if (y == minY || y == maxY)
                            continue;

                        var isFill = false;
                        for (var x = 0; x < 96; x++)
                        {
                            if (_tempBuffer[y, x] && x + 1 < 96 && !_tempBuffer[y, x + 1])
                                isFill = !isFill;
                            if (isFill)
                                _tempBuffer[y, x] = true;
                        }
                    }
                    break;
            }

            MergeTempBuffer();
        }

        private void MergeTempBuffer()
        {
            for (byte x = 0; x < 96; x++)
                for (byte y = 0; y < 64; y++)
                    GraphicsBuffer[y, x] = GraphicsBuffer[y, x] || _tempBuffer[y, x];
            ClearTempBuffer();
        }

        private void ClearTempBuffer()
        {
            for (byte x = 0; x < 96; x++)
                for (byte y = 0; y < 64; y++)
                    _tempBuffer[y, x] = false;
        }

        public void AddPoint(int x, int y)
        {
            if (_currentMode != BeginMode.None)
                _currentPoints.Add(new Point(x, y));
        }

        public class Point
        {
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}