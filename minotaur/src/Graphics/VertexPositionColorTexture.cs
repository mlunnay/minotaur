using System.Runtime.InteropServices;
using OpenTK;

namespace Minotaur.Graphics
{
  [StructLayout(LayoutKind.Sequential)]
  public struct VertexPositionColorTexture
  {
    #region Fields

    public Vector3 Position;
    public Vector4 Color;
    public Vector2 TextureCoordinate;

    #endregion

    public VertexFormat VertexFormat { get { return VertexFormat.PositionColorTexture; } }

    #region Constructors

    public VertexPositionColorTexture(Vector3 position, Vector4 color, Vector2 textureCoordinate)
    {
      Position = position;
      Color = color;
      TextureCoordinate = textureCoordinate;
    }

    public VertexPositionColorTexture(float x, float y, float z, float r, float g, float b, float a, float s, float t)
    {
      Position = new Vector3(x, y, z);
      Color = new Vector4(r, g, b, a);
      TextureCoordinate = new Vector2(s, t);
    }

    #endregion
  }
}
