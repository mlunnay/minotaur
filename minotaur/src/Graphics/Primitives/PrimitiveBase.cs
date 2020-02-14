using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics.Primitives
{
  public abstract class PrimitiveBase : IPrimitive
  {
    private BeginMode _beginMode;
    private Matrix4 _world;
    private Color4 _color;

    protected abstract IEnumerable<Vector3> Positions { get; }
    protected abstract IEnumerable<ushort> Indices { get; }
    protected virtual IEnumerable<Color4> Colors { get { return null; } }

    public BeginMode BeginMode
    {
      get { return _beginMode; }
    }

    protected PrimitiveBase(BeginMode beginMode)
    {
      _beginMode = beginMode;
    }

    public VertexPositionColor[] GetVertices(GraphicsDevice _graphicsDevice,Matrix4 world, Color4 color, out ushort[] indices)
    {
      indices = Indices.ToArray();
      if (Colors == null)
        return Positions.Select(i => new VertexPositionColor(Vector3.Transform(i, world), new Vector4(color.R, color.G, color.B, color.A))).ToArray();
      else  // custom colors so ignore the color passed in.
        return Positions.Zip(Colors, (p, c) => new VertexPositionColor(Vector3.Transform(p, world), new Vector4(c.R, c.G, c.B, c.A))).ToArray();
    }
  }
}
