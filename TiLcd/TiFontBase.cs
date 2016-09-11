using System;

namespace TiLcdTest
{
    internal abstract class TiFontBase
    {
        /// <summary>
        /// The actual font itself. Fonts should be 5x8 pixels in size and should be stored vertically.
        /// Fonts should be stored in the pattern: [width * one byte] * number of characters
        /// </summary>
        protected static byte[] Font = new byte[0];

        public int CharWidth { get; protected set; }
        public int CharHeight { get; protected set; }

        public int GetStringWidth(string s)
        {
            return s.Length * (CharWidth + 1);
        }

        public byte[] GetCharBytes(char c)
        {
            if (c < 0 || c >= 255)
                c = '?';

            var charBytes = new byte[CharWidth];
            Array.Copy(Font, c*CharWidth, charBytes, 0, charBytes.Length);

            return charBytes;
        }

        public void RenderCharToBuffer(int x, int y, char c, ref bool[,] buffer)
        {
            var charBytes = GetCharBytes(c).ToBoolArray();

            for (var nx = x; nx < x + CharWidth; nx++)
                for (var ny = y; ny < y + CharHeight; ny++)
                    buffer[y + (y + CharHeight - ny), nx] = charBytes[nx - x][ny - y];
        }

        public void RenderStringToBuffer(int x, int y, string s, ref bool[,] buffer)
        {
            var nx = 0;
            foreach (var c in s.ToCharArray())
            {
                RenderCharToBuffer(x + nx, y, c, ref buffer);
                nx += CharWidth + 1;
                if (nx >= 96)
                    break;
            }
        }
    }
}