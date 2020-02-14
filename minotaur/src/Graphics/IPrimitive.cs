using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public interface IPrimitive
  {
    BeginMode BeginMode { get; }
    VertexPositionColor[] GetVertices(GraphicsDevice graphicsDevice, Matrix4 world, Color4 color, out ushort[] indices);
  }
}
