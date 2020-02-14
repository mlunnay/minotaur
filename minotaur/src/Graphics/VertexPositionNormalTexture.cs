using System.Runtime.InteropServices;
using OpenTK;

namespace Minotaur.Graphics
{
  [StructLayout(LayoutKind.Sequential)]
  public struct VertexPositionNormalTexture
  {
    #region Fields

    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TextureCoordinate;

    #endregion

    public VertexFormat VertexFormat { get { return VertexFormat.PositionNormalTexture; } }

    #region Constructors

    public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 textureCoordinate)
    {
      Position = position;
      Normal = normal;
      TextureCoordinate = textureCoordinate;
    }

    public VertexPositionNormalTexture(float x, float y, float z, float nx, float ny, float nz, float s, float t)
    {
      Position = new Vector3(x, y, z);
      Normal = new Vector3(nx, ny, nz);
      TextureCoordinate = new Vector2(s, t);
    }

    #endregion
  }
}
