using System;
using System.Collections.Generic;

namespace TiLcdTest
{
    class TiGraphics
    {
        private readonly TiLcd _lcd;
        private BeginMode _currentMode;
        private List<Point> _currentPoints;

        public TiGraphics(ref TiLcd lcd)
        {
            _lcd = lcd;
            _currentPoints = new List<Point>();
        }

        /// <summary>
        /// Plot the line from (x0, y0) to (x1, y1)
        /// </summary>
        /// <author>Jason Morley</author>
        /// <param name="x0">The start x</param>
        /// <param name="y0">The start y</param>
        /// <param name="x1">The end x</param>
        /// <param name="y1">The end y</param>
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            var steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep) { Utils.Swap(ref x0, ref y0); Utils.Swap(ref x1, ref y1); }
            if (x0 > x1) { Utils.Swap(ref x0, ref x1); Utils.Swap(ref y0, ref y1); }
            int dX = x1 - x0, dY = Math.Abs(y1 - y0), err = dX / 2, ystep = y0 < y1 ? 1 : -1, y = y0;

            for (var x = x0; x <= x1; ++x)
            {
                if (!(steep ? _lcd.SetPixel(y, x, true, false) : _lcd.SetPixel(x, y, true, false))) return;
                err = err - dY;
                if (err >= 0) continue;
                y += ystep; err += dX;
            }
        }

        public void BeginDraw(BeginMode mode)
        {
            _currentMode = mode;
        }

        public void EndDraw()
        {
            RenderDrawnPoints(_currentMode, _currentPoints);
            _currentMode = BeginMode.None;
        }

        private void RenderDrawnPoints(BeginMode currentMode, List<Point> currentPoints)
        {
            throw new NotImplementedException();
        }

        public void AddPoint(int x, int y)
        {
            if (_currentMode != BeginMode.None)
                _currentPoints.Add(new Point(x, y));
        }
    }
}
