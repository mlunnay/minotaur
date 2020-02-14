using OpenTK;
using OpenTK.Graphics;

namespace Minotaur.Graphics.Primitives
{
  public class Arrow : IPrimitive
  {
    private Vector3 _start;
    private Vector3 _end;
    private Color4 _color;
    private float _width;
    private const float _ratio = 0.2f;

    public OpenTK.Graphics.OpenGL.BeginMode BeginMode
    {
      get { return OpenTK.Graphics.OpenGL.BeginMode.Lines; }
    }

    public Arrow(Vector3 start, Vector3 end, float headWidth, Color4 color)
    {
      _start = start;
      _end = end;
      _color = color;
      _width = headWidth;
    }

    public VertexPositionColor[] GetVertices(GraphicsDevice _graphicsDevice, Matrix4 world, Color4 color, out ushort[] indices)
    {
      VertexPositionColor[] vertices = new VertexPositionColor[4];
      Vector4 c = new Vector4(_color.R, _color.G, _color.B, _color.A);

      vertices[0] = new VertexPositionColor(Vector3.Transform(_start, world), c);
      vertices[1] = new VertexPositionColor(Vector3.Transform(_end, world), c);
      Vector3 mid = Vector3.Lerp(vertices[1].Position, vertices[0].Position, _ratio);
      Vector3 aa, ab, ba, bb;
      float width = _width != 0 ? _width : Vector3.Subtract(vertices[0].Position, vertices[1].Position).Length * _ratio * 0.8f;
      DebugBatch.CreateBillboard(_graphicsDevice.Camera, mid, vertices[1].Position, width, out aa, out ab, out ba, out bb);
      vertices[2] = new VertexPositionColor(ba, c);
      vertices[3] = new VertexPositionColor(bb, c);

      indices = new ushort[] { 0, 1, 1, 2, 1, 3 };
      return vertices;
    }
  }
}
