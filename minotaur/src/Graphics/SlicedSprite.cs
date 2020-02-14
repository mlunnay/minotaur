using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;

namespace Minotaur.Graphics
{
  public class SlicedSprite : Sprite
  {
    private float[] _textureCoords = new float[8];  // [x0, x1, x2, x3, y0, y1, y2, y3]
    private int[] _borders;

    public int[] Borders  // left, top, right, bottom
    {
      get { return _borders; }
      set
      {
        _borders = value;
        CalcTextureCoords();
      }
    }

    public SlicedSprite(Texture2D texture,
      int x, int y, int width, int height,
      int[] borders)
      : base(texture, x, y, width, height)
    {
      Borders = borders;
    }

    public SlicedSprite(Texture2D texture,
      int x, int y, int width, int height,
      int leftBorder, int topBorder, int rightBorder, int bottomBorder)
      : this(texture, x, y, width, height, new int[] { leftBorder, topBorder, rightBorder, bottomBorder }) { }

    public SlicedSprite(Texture2D texture,
      int x, int y, int width, int height,
      int border)
      : this(texture, x, y, width, height, new int[] { border, border, border, border }) { }

    public SlicedSprite(SlicedSprite s)
      :base(s.Texture, s.X, s.Y, s.Width, s.Height)
    {
      Borders = new int[] { s.Borders[0], s.Borders[1], s.Borders[2], s.Borders[3] };
    }

    public override VertexPositionColorTexture[] GetVertices(int x, int y, int width, int height, float depth, Color4 color, out ushort[] indices)
    {
      //indices = _indices;
      indices = new ushort[] {
      0, 5, 1,
      0, 4, 5,
      1, 5, 6,
      1, 6, 2,
      2, 6, 3,
      3, 6, 7,
      4, 8, 5,
      5, 8, 9,
      5, 9, 6,
      6, 9, 10,
      6, 10, 7,
      7, 10, 11,
      8, 12, 9,
      9, 12, 13,
      9, 13, 14,
      9, 14, 10,
      10, 14, 15,
      10, 15, 11
    };
      VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[16];
      vertices[0] = new VertexPositionColorTexture(x, y, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[0],
        _textureCoords[4]);
      vertices[1] = new VertexPositionColorTexture(x + Borders[0], y, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[1],
        _textureCoords[4]);
      vertices[2] = new VertexPositionColorTexture(x + width - Borders[2], y, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[2],
        _textureCoords[4]);
      vertices[3] = new VertexPositionColorTexture(x + width, y, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[3],
        _textureCoords[4]);
      vertices[4] = new VertexPositionColorTexture(x, y + Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[0],
        _textureCoords[5]);
      vertices[5] = new VertexPositionColorTexture(x + Borders[0], y + Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[1],
        _textureCoords[5]);
      vertices[6] = new VertexPositionColorTexture(x + width - Borders[2], y + Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[2],
        _textureCoords[5]);
      vertices[7] = new VertexPositionColorTexture(x + width, y + Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[3],
        _textureCoords[5]);
      vertices[8] = new VertexPositionColorTexture(x, y + height - Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[0],
        _textureCoords[6]);
      vertices[9] = new VertexPositionColorTexture(x + Borders[0], y + height - Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[1],
        _textureCoords[6]);
      vertices[10] = new VertexPositionColorTexture(x + width - Borders[2], y + height - Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[2],
        _textureCoords[6]);
      vertices[11] = new VertexPositionColorTexture(x + width, y + height - Borders[1], depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[3],
        _textureCoords[6]);
      vertices[12] = new VertexPositionColorTexture(x, y + height, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[0],
        _textureCoords[7]);
      vertices[13] = new VertexPositionColorTexture(x + Borders[0], y + height, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[1],
        _textureCoords[7]);
      vertices[14] = new VertexPositionColorTexture(x + width - Borders[2], y + height, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[2],
        _textureCoords[7]);
      vertices[15] = new VertexPositionColorTexture(x + width, y + height, depth,
        color.R, color.G, color.B, color.A,
        _textureCoords[3],
        _textureCoords[7]);

      return vertices;
    }

    private void CalcTextureCoords()
    {
      _textureCoords[0] = X / (float)Texture.Width;
      _textureCoords[1] = (X + Borders[0]) / (float)Texture.Width;
      _textureCoords[2] = (X + Width - Borders[2]) / (float)Texture.Width;
      _textureCoords[3] = (X + Width) / (float)Texture.Width;
      _textureCoords[4] = Y / (float)Texture.Height;
      _textureCoords[5] = (Y + Borders[1]) / (float)Texture.Height;
      _textureCoords[6] = (Y + Height - Borders[3]) / (float)Texture.Height;
      _textureCoords[7] = (Y + Height) / (float)Texture.Height;
    }
  }
}
