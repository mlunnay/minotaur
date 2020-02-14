using System.Collections.Generic;
using OpenTK;

namespace Minotaur.Graphics.Primitives
{
  public class AABox : PrimitiveBase
  {
    private Vector3[] _vertices;
    private ushort[] _indices;

    protected override IEnumerable<Vector3> Positions
    {
      get { return _vertices; }
    }

    protected override IEnumerable<ushort> Indices
    {
      get { return _indices; }
    }

    public AABox(Vector3 topleft, Vector3 bottomRight, bool solid = false)
      :base(solid ? OpenTK.Graphics.OpenGL.BeginMode.Triangles : OpenTK.Graphics.OpenGL.BeginMode.Lines)
    {
      _vertices = new Vector3[] {
        topleft,
        new Vector3(topleft.X, topleft.Y, bottomRight.Z),
        new Vector3(bottomRight.X, topleft.Y, bottomRight.Z),
        new Vector3(bottomRight.X, topleft.Y, topleft.Z),
        new Vector3(topleft.X, bottomRight.Y, topleft.Z),
        new Vector3(topleft.X, bottomRight.Y, bottomRight.Z),
        bottomRight,
        new Vector3(bottomRight.X, bottomRight.Y, topleft.Z),
      };
      if (solid)
      {
        _indices = new ushort[] {
          0, 1, 3, 3, 1, 2,
          0, 5, 4, 0, 1, 5,
          0, 4, 7, 0, 7, 3,
          1, 5, 6, 1, 6, 2,
          3, 2, 6, 3, 6, 7,
          4, 5, 6, 4, 6, 7
        };
      }
      else
      {
        _indices = new ushort[] {
        0, 1, 1, 2, 2, 3, 3, 0,
        0, 4, 1, 5, 2, 6, 3, 7,
        4, 5, 5, 6, 6, 7, 7, 4
      };
      }
    }
  }
}
