using System.Runtime.InteropServices;
using OpenTK;

namespace Minotaur.Graphics
{
  [StructLayout(LayoutKind.Sequential)]
  public struct VertexPositionColor
  {
    #region Fields

    public Vector3 Position;
    public Vector4 Color;

    #endregion

    public VertexFormat VertexFormat { get { return VertexFormat.PositionColor; } }

    #region Constructors

    public VertexPositionColor(Vector3 position, Vector4 color)
    {
      Position = position;
      Color = color;
    }

    public VertexPositionColor(float x, float y, float z, float r, float g, float b, float a)
    {
      Position = new Vector3(x, y, z);
      Color = new Vector4(r, g, b, a);
    }

    #endregion
  }
}
