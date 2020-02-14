using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
    public class BitmapCharacterSet
    {
        public int LineHeight;
        public int Base;
        public int RenderedSize;
        public int Width;
        public int Height;
        public int PaddingUp = 0;
        public int PaddingRight = 0;
        public int PaddingDown = 0;
        public int PaddingLeft = 0;
        public List<BitmapCharacter> Characters = new List<BitmapCharacter>();

        public BitmapCharacter this[int id]
        {
            get
            {
                return Characters.FirstOrDefault(c => c.ID == id);
            }
        }

        public BitmapCharacter GetCharacterByID(int id)
        {
            BitmapCharacter character = Characters.FirstOrDefault(c => c.ID == id);
            return character;
        }
    }
}
