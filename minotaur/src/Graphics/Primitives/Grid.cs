using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Minotaur.Graphics.Primitives
{
  public class Grid : PrimitiveBase
  {
    private Vector3[] _positions;
    private ushort[] _indices;

    protected override IEnumerable<Vector3> Positions
    {
      get { return _positions; }
    }

    protected override IEnumerable<ushort> Indices
    {
      get { return _indices; }
    }

    public Grid(float step, int countX, int countY)
      : this(-step * countX * 0.5f, 0, -step * countY * 0.5f, step * countX, step * countY, countX, countY) { }

    public Grid(float x, float y, float z,
      float step, int countX, int countY)
      : this(x, y, z, step * countX, step * countY, countX, countY) { }

    public Grid(float x, float y, float z,
      float width, float height, 
      int countX, int countY)
      :base(OpenTK.Graphics.OpenGL.BeginMode.Lines)
    {
      float incU = width / countX;
      float incV = height / countY;
      int index = 0;
      _positions = new Vector3[(countX + countY + 2) * 2];
      _indices = new ushort[(countX + countY + 2) * 2];

      for (int u = 0; u <= countX; u++)
      {
        _positions[index] = new Vector3(x, y, z + u * incU);
        _indices[index] = (ushort)index++;
        _positions[index] = new Vector3(x + width, y, z + u * incU);
        _indices[index] = (ushort)index++;        
      }

      for (int v = 0; v <= countX; v++)
      {
        _positions[index] = new Vector3(x + v * incV, y, z);
        _indices[index] = (ushort)index++;
        _positions[index] = new Vector3(x + v * incV, y, z + height);
        _indices[index] = (ushort)index++;
      }
    }
  }
}
