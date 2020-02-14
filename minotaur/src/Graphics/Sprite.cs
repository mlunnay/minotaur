using System.Collections.Generic;
using OpenTK.Graphics;

namespace Minotaur.Graphics
{
  public class Sprite : ISprite
  {
    public Texture2D Texture { get; private set; }

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }


    public Sprite(Texture2D texture,
      int x, int y, int width, int height)
    {
      Texture = texture;
      X = x;
      Y = y;
      Width = width == 0 ? texture.Width : width;
      Height = height == 0 ? texture.Height : height;
    }

    public Sprite(Sprite s)
    {
      Texture = s.Texture;
      X = s.X;
      Y = s.Y;
      Width = s.Width;
      Height = s.Height;
    }

    public virtual VertexPositionColorTexture[] GetVertices(int x, int y, int width, int height, float depth, Color4 color, out ushort[] indices)
    {
      indices = new ushort[] { 0, 2, 1, 1, 2, 3 };
      VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[4];
      vertices[0] = new VertexPositionColorTexture(x, y, depth,
        color.R, color.G, color.B, color.A,
        X / (float)Texture.Width,
        Y / (float)Texture.Height);
      vertices[1] = new VertexPositionColorTexture(x + width, y, depth,
        color.R, color.G, color.B, color.A,
        (X + Width) / (float)Texture.Width,
        Y / (float)Texture.Height);
      vertices[2] = new VertexPositionColorTexture(x, y + height, depth,
        color.R, color.G, color.B, color.A,
        X / (float)Texture.Width,
        (Y + Height) / (float)Texture.Height);
      vertices[3] = new VertexPositionColorTexture(x + width, y + height, depth,
        color.R, color.G, color.B, color.A,
        (X + Width) / (float)Texture.Width,
        (Y + Height) / (float)Texture.Height);
      return vertices;
    }
  }
}
