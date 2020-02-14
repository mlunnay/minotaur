using System.Collections.Generic;

namespace Minotaur.Graphics
{
    public class BitmapCharacter
    {
        public int ID = -1;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public int XOffset;
        public int YOffset;
        public int XAdvance;
        public Dictionary<int, int> Kerning = new Dictionary<int, int>();
    }
}
