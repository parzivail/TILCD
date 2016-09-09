using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPiTiLcd
{
    class TiCharacter
    {
        public char Character;
        public bool[,] Pixels;

        public TiCharacter(char c, bool[,] pixels)
        {
            Character = c;
            Pixels = pixels;
        }
    }
}
