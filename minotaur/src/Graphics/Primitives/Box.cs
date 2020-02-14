using System.Collections.Generic;
using OpenTK;

namespace Minotaur.Graphics.Primitives
{
  public class Box : PrimitiveBase
  {
    private Vector3[] _positions;
    private ushort[] _indices = new ushort[] { 0, 1, 1, 2, 2, 3, 3, 0 };

    protected override IEnumerable<Vector3> Positions
    {
      get { return _positions; }
    }

    protected override IEnumerable<ushort> Indices
    {
      get { return _indices; }
    }

    public Box(Vector3 topLeft, Vector3 bottomRight, bool solid = false)
      : base(OpenTK.Graphics.OpenGL.BeginMode.Lines)
    {
      _positions = new Vector3[] { topLeft,
        new Vector3(topLeft.X, bottomRight.Y, topLeft.Z),
        bottomRight,
        new Vector3(bottomRight.X, topLeft.Y, bottomRight.Z)};
    }
  }
}
