using System.Collections.Generic;

namespace Minotaur.Pipeline.Graphics
{
  public class SpriteFontContent : ContentItem
  {
    public class Character
    {
      public int ID;
      public int X;
      public int Y;
      public int Width;
      public int Height;
      public int XOffset;
      public int YOffset;
      public int XAdvance;
      public Dictionary<int, int> Kerning = new Dictionary<int, int>();

      public Character() { }

      public Character(int id, int x, int y, int width, int height, int xOffset, int yOffset, int xAdvance)
      {
        ID = id;
        X = x;
        Y = y;
        Width = width;
        Height = height;
        XOffset = xOffset;
        YOffset = yOffset;
        XAdvance = xAdvance;
      }
    }

    public class CharSet
    {
      public int LineHeight;
      public int Base;
      public int RenderSize;
      public int PaddingUp;
      public int PaddingRight;
      public int PaddingDowm;
      public int PaddingLeft;
      public List<Character> Characters = new List<Character>();
    }

    public CharSet CharacterSet;
    public Texture2DContent Texture;

    public SpriteFontContent()
    {
      CharacterSet = new CharSet();
    }
  }
}
