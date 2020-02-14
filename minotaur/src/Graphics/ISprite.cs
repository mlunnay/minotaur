using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using OpenTK;

namespace Minotaur.Graphics
{
  public interface ISprite
  {
    Texture2D Texture { get; }

    int X { get; }
    int Y { get; }
    int Width { get; }
    int Height { get; }

    VertexPositionColorTexture[] GetVertices(int x, int y, int width, int height, float depth, Color4 color, out ushort[] indices);
  }
}
