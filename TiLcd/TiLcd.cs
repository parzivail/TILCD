using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Gpio;

namespace TiLcdTest
{
    internal class TiLcd
    {
        private readonly GpioPin _ce;
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
        private byte _contrast;

        internal readonly bool[,] GraphicsBuffer = new bool[64, 96];

        private BeginMode _currentMode;
        private readonly List<Point> _currentPoints;
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
            var wr1 = gpio.OpenPin(wr); // Local variable because I don't think I'll ever implement status read, only write.
            wr1.SetDriveMode(GpioPinDriveMode.Output);
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

            wr1.Write(false);

            _currentMode = BeginMode.None;
            _currentPoints = new List<Point>();

            ClearScreenBuffer();
            ClearTempBuffer();
        }

        #region LCD Control

        /// <summary>
        ///     Initializes the display. Sets the word length to 8, counter mode to <see cref="CounterMode.XUp" />, display state
        ///     to <see cref="DisplayState.On" />, and contrast to [contrast = 48].
        /// </summary>
        /// <param name="contrast">The contrast of the display. Optional.</param>
        public void Init(byte contrast = 48)
        {
            _contrast = contrast;
            SetWordLength(true);
            SetCounterMode(CounterMode.XUp);
            SetDisplayState(DisplayState.On);
            SetContrast(contrast);
            SetPos(0, 0);
        }

        /// <summary>
        ///     Resets the display.
        /// </summary>
        public void Reset()
        {
            _rst.Write(false);
            100.Delay();
            _rst.Write(true);
            Init(_contrast);
        }

        /// <summary>
        ///     Sets the <see cref="DisplayState" /> of the display.
        /// </summary>
        /// <param name="state">The state, 0 or 1.</param>
        public void SetDisplayState(byte state)
        {
            WriteBinaryValue(0, (byte) (TiCommand.DisplayStateSet | state));
        }

        /// <summary>
        ///     This command selects the counter and the up/down mode. For instance, when X-counter/up mode is selected,
        ///     the X-address is incremented in response to every data read and write.However, when X-counter/up mode is
        ///     selected, the address in the Y-(page) counter will not change.Hence the Y-address must be set (with the SYE
        ///     command) before it can be changed.
        /// </summary>
        /// <param name="mode">The mode (of type <see cref="CounterMode" />)</param>
        public void SetCounterMode(byte mode)
        {
            WriteBinaryValue(0, (byte) (TiCommand.CounterModeSet | mode));
        }

        /// <summary>
        ///     This command sets the contrast for the LCD. The LCD contrast can be set in 64 steps. The command C0H
        ///     selects the brightest level; the command FFH selects the darkest.
        /// </summary>
        /// <param name="contrast"></param>
        public void SetContrast(byte contrast)
        {
            WriteBinaryValue(0, (byte) (TiCommand.ContrastSet | (contrast & CommandMask.ContrastMask)));
        }

        /// <summary>
        ///     Sets the physical position of the register pointer for 8 bit writing.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPos(byte x, byte y)
        {
            SetX(x);
            SetY(y);
        }

        /// <summary>
        ///     Sets the physical X value of the LCD driver's buffer. Note this is technically the Y/Page buffer.
        /// </summary>
        /// <param name="x"></param>
        public void SetX(byte x)
        {
            // Display X is really the Y/Page buffer
            WriteBinaryValue(0, (byte) (TiCommand.YAddressSet | (x & CommandMask.YMask)));
        }

        /// <summary>
        ///     This command sets the top row of the LCD screen, irrespective of the current X-address. For instance, when
        ///     the Z-address is 32, the top row of the LCD screen is address 32 of the display RAM, and the bottom row of the
        ///     LCD screen is address 31 of the display RAM.
        /// </summary>
        /// <param name="z">The y address</param>
        public void SetZ(byte z)
        {
            WriteBinaryValue(0, (byte) (TiCommand.ZAddressSet | (z & CommandMask.ZMask)));
        }

        /// <summary>
        ///     Sets the physical Y value of the LCD driver's buffer. Note this is technically the X buffer.
        /// </summary>
        /// <param name="y">The y position between 0 and 63</param>
        public void SetY(byte y)
        {
            // Display Y is really the X buffer
            WriteBinaryValue(0, (byte) (TiCommand.XAddressSet | (y & CommandMask.XMask)));
        }

        /// <summary>
        ///     Writes a binary value to the command interface.
        /// </summary>
        /// <param name="di">1 if the command is graphical information, 0 otherwise</param>
        /// <param name="value">The value to write</param>
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

        /// <summary>
        ///     Sets the word length of the LCD command interface.
        /// </summary>
        /// <param name="eightBits">True is the interface should be 8 bits wide</param>
        private void SetWordLength(bool eightBits)
        {
            WriteBinaryValue(0,
                (byte) (TiCommand.WordLengthSet | (eightBits ? WordLength.EightBits : WordLength.SixBits)));
        }

        #endregion

        #region Graphics Buffer

        /// <summary>
        ///     Clears the graphics buffer.
        /// </summary>
        private void ClearScreenBuffer()
        {
            for (byte x = 0; x < 96; x++)
                for (byte y = 0; y < 64; y++)
                    GraphicsBuffer[y, x] = false;
        }

        /// <summary>
        ///     Public bridge for <see cref="ClearScreenBuffer" />.
        /// </summary>
        public void Clear()
        {
            ClearScreenBuffer();
        }

        /// <summary>
        ///     refreshes the screen from the internal graphics buffer, overwriting any previous screen content.
        /// </summary>
        public void RefreshFromBuffer()
        {
            SetScreenBools(GraphicsBuffer);
        }

        /// <summary>
        ///     Sets the value of each pixel to the values in the given 64x96 array.
        /// </summary>
        /// <param name="pixels">The pixels to set. Must be 64x96.</param>
        public void SetScreenBools(bool[,] pixels)
        {
            if (pixels.Length != 64*96)
                return;

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

        /// <summary>
        ///     Sets a pixel in the given buffer to the given value
        /// </summary>
        /// <param name="x">The x position of the pixel</param>
        /// <param name="y">The y position of the pixel</param>
        /// <param name="value">The pixel's value</param>
        /// <returns>True if the value was sucessfully written</returns>
        public bool SetPixel(int x, int y, bool value)
        {
            if ((x < 0) || (x >= 96) || (y < 0) || (y >= 64))
                return false;

            GraphicsBuffer[y, x] = value;

            return true;
        }

        /// <summary>
        ///     Sets a pixel in the given buffer to the given value
        /// </summary>
        /// <param name="x">The x position of the pixel</param>
        /// <param name="y">The y position of the pixel</param>
        /// <param name="value">The pixel's value</param>
        /// <param name="buffer">The buffer to set in</param>
        /// <returns>True if the value was sucessfully written</returns>
        public bool SetPixelInBuffer(int x, int y, bool value, ref bool[,] buffer)
        {
            if ((x < 0) || (x >= 96) || (y < 0) || (y >= 64))
                return false;

            buffer[y, x] = value;

            return true;
        }

        /// <summary>
        ///     Sets the value of each pixel to the values in the given 64x96 array.
        /// </summary>
        /// <param name="pixels">The pixels to set. Must be 64x96.</param>
        public void SetScreenBytes(byte[,] pixels)
        {
            if (pixels.Length != 64*96)
                return;

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

        /// <summary>
        ///     Begins a drawing routine with the specified mode.
        /// </summary>
        /// <param name="mode">The drawing mode to use when rendering</param>
        public void BeginDraw(BeginMode mode)
        {
            _currentMode = mode;
        }

        /// <summary>
        ///     Ends the drawing routine and renders the points to the graphics buffer.
        /// </summary>
        public void EndDraw()
        {
            RenderDrawnPoints();
            _currentPoints.Clear();
            _currentMode = BeginMode.None;
        }

        /// <summary>
        ///     Renders the points from the point list, and merges the temporary buffer.
        /// </summary>
        private void RenderDrawnPoints()
        {
            if (_currentPoints.Count < 2)
                return;

            Point last = null;

            foreach (var currentPoint in _currentPoints)
            {
                if (last != null)
                    DrawLineToBuffer(ref _tempBuffer, last.X, last.Y, currentPoint.X, currentPoint.Y);
                last = currentPoint;
            }

            switch (_currentMode)
            {
                case BeginMode.LineLoop:
                    DrawLineToBuffer(ref _tempBuffer, last.X, last.Y, _currentPoints[0].X, _currentPoints[0].Y);
                    break;
                case BeginMode.Fill:
                    DrawLineToBuffer(ref _tempBuffer, last.X, last.Y, _currentPoints[0].X, _currentPoints[0].Y);

                    var minX = _currentPoints.Min(point => point.X);
                    var maxX = _currentPoints.Max(point => point.X);
                    var minY = _currentPoints.Min(point => point.Y);
                    var maxY = _currentPoints.Max(point => point.Y);

                    for (var y = minY - 1; y <= maxY; y++)
                        for (var x = minX - 1; x <= maxX; x++)
                            _tempBuffer[y, x] = new Point(x, y).PointInPolygon(_currentPoints);

                    break;
            }

            MergeTempBuffer();
        }

        /// <summary>
        ///     Merges the temporary buffer with the graphics buffer using OR.
        /// </summary>
        private void MergeTempBuffer()
        {
            for (byte x = 0; x < 96; x++)
                for (byte y = 0; y < 64; y++)
                    GraphicsBuffer[y, x] = GraphicsBuffer[y, x] || _tempBuffer[y, x];
            ClearTempBuffer();
        }

        /// <summary>
        ///     Clears the temporary buffer.
        /// </summary>
        private void ClearTempBuffer()
        {
            for (byte x = 0; x < 96; x++)
                for (byte y = 0; y < 64; y++)
                    _tempBuffer[y, x] = false;
        }

        /// <summary>
        ///     Adds a point to the temporary buffer. When <see cref="EndDraw" /> is called, the buffer is drawn.
        /// </summary>
        /// <param name="x">The x position of the point</param>
        /// <param name="y">The y position of the point</param>
        public void AddPoint(int x, int y)
        {
            if (_currentMode != BeginMode.None)
                _currentPoints.Add(new Point(x, y));
        }

        #endregion

        #region Drawing Methods

        /// <summary>
        ///     Plot the line from (x0, y0) to (x1, y1)
        /// </summary>
        /// <author>Jason Morley</author>
        /// <param name="buffer">The buffer to draw the line to</param>
        /// <param name="x0">The start x</param>
        /// <param name="y0">The start y</param>
        /// <param name="x1">The end x</param>
        /// <param name="y1">The end y</param>
        private void DrawLineToBuffer(ref bool[,] buffer, int x0, int y0, int x1, int y1)
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
                if (!(steep ? SetPixelInBuffer(y, x, true, ref buffer) : SetPixelInBuffer(x, y, true, ref buffer)))
                    return;
                err = err - dY;
                if (err >= 0) continue;
                y += ystep;
                err += dX;
            }
        }

        /// <summary>
        ///     Draws the specified text to screen. Edge-to-edge the screen can fit 16x10 lines of text.
        /// </summary>
        /// <param name="x">The x position of the string</param>
        /// <param name="y">The y position of the string</param>
        /// <param name="s">The string to draw</param>
        public void DrawText(int x, int y, string s)
        {
            if (_currentMode != BeginMode.None)
                return;

            TiDefaultFont.RenderStringToBuffer(x, y, s, ref _tempBuffer);
            MergeTempBuffer();
        }

        /// <summary>
        ///     Draws a rectangle to the graphics buffer.
        /// </summary>
        /// <param name="x">The x of the rectangle</param>
        /// <param name="y">The y of the rectangle</param>
        /// <param name="w">The width of the rectangle</param>
        /// <param name="h">The height of the rectangle</param>
        /// <param name="filled"></param>
        public void DrawRectangle(int x, int y, int w, int h, bool filled)
        {
            if (_currentMode != BeginMode.None)
                return;

            BeginDraw(filled ? BeginMode.Fill : BeginMode.LineLoop);

            AddPoint(x, y);
            AddPoint(x, y + h);
            AddPoint(x + w, y + h);
            AddPoint(x + w, y);

            EndDraw();
        }

        /// <summary>
        ///     Draws a circle to the graphics buffer.
        /// </summary>
        /// <param name="x">The x of the circle</param>
        /// <param name="y">The y of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <param name="filled">True if the circle should be filled</param>
        public void DrawCircle(int x, int y, int r, bool filled)
        {
            DrawPartialCircle(x, y, r, 1, filled);
        }

        /// <summary>
        ///     Draws a partial circle to the graphics buffer.
        /// </summary>
        /// <param name="x">The x of the circle</param>
        /// <param name="y">The y of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <param name="percent">The percent "full" of the circle</param>
        /// <param name="filled">True if the circle should be filled</param>
        public void DrawPartialCircle(int x, int y, int r, float percent, bool filled)
        {
            if (_currentMode != BeginMode.None)
                return;

            BeginDraw(filled ? BeginMode.Fill : BeginMode.LineLoop);

            AddPoint(x, y);

            for (var i = 0; i <= 360*percent; i += 2)
            {
                var nx = (int) Math.Round(Math.Sin(i*3.141526f/180)*r);
                var ny = (int) Math.Round(Math.Cos(i*3.141526f/180)*r);
                AddPoint(x + nx, y - ny);
            }

            EndDraw();
        }

        /// <summary>
        ///     Draws a donut to the graphics buffer.
        /// </summary>
        /// <param name="x">The x of the donut</param>
        /// <param name="y">The y of the donut</param>
        /// <param name="r">The radius of the donut</param>
        /// <param name="stripSize">The size of the filled strip</param>
        /// <param name="filled">True if the circle should be filled</param>
        public void DrawDonut(int x, int y, int r, int stripSize, bool filled)
        {
            DrawPartialDonut(x, y, r, stripSize, 1, filled);
        }

        /// <summary>
        ///     Draws a partial donut to the graphics buffer.
        /// </summary>
        /// <param name="x">The x of the donut</param>
        /// <param name="y">The y of the donut</param>
        /// <param name="r">The radius of the donut</param>
        /// <param name="stripSize">The size of the filled strip</param>
        /// <param name="percent">The percent "full" of the circle</param>
        /// <param name="filled">True if the circle should be filled</param>
        public void DrawPartialDonut(int x, int y, int r, int stripSize, float percent, bool filled)
        {
            if (_currentMode != BeginMode.None)
                return;

            BeginDraw(filled ? BeginMode.Fill : BeginMode.LineLoop);

            AddPoint(x, y - (r - stripSize));

            for (var i = 0; i <= 360*percent; i++)
            {
                var nx = (int) Math.Round(Math.Sin(i*3.141526f/180)*r);
                var ny = (int) Math.Round(Math.Cos(i*3.141526f/180)*r);
                AddPoint(nx + x, y - ny);
            }

            for (var i = 360*percent; i >= 0; i--)
            {
                var nx = (int) Math.Round(Math.Sin(i*3.141526f/180)*(r - stripSize));
                var ny = (int) Math.Round(Math.Cos(i*3.141526f/180)*(r - stripSize));
                AddPoint(nx + x, y - ny);
            }

            EndDraw();
        }

        #endregion
    }
}