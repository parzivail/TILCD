using System.Collections.Generic;

namespace TiLcdTest
{
    internal class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public bool InPolygon(List<Point> polygon)
        {
            var inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
                if ((polygon[i].Y > Y != polygon[j].Y > Y) &&
                    (X <
                     (polygon[j].X - polygon[i].X)*(Y - polygon[i].Y)/(polygon[j].Y - polygon[i].Y) + polygon[i].X))
                    inside = !inside;
            return inside;
        }
    }
}