namespace TiLcd
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
