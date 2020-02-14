using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using OpenTK;

namespace Minotaur.Graphics
{
  [StructLayout(LayoutKind.Sequential)]
  public struct VertexPositionTexture
  {
    #region Fields

    public Vector3 Position;
    public Vector2 TextureCoordinates;

    #endregion

    public VertexFormat VertexFormat { get { return VertexFormat.PositionTexture; } }

    #region Constructors

    public VertexPositionTexture(Vector3 position, Vector2 textureCoordinates)
    {
      Position = position;
      TextureCoordinates = textureCoordinates;
    }

    public VertexPositionTexture(float x, float y, float z, float s, float t)
    {
      Position = new Vector3(x, y, z);
      TextureCoordinates = new Vector2(s, t);
    }

    #endregion
  }
}
