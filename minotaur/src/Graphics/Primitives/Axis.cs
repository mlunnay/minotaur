using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using Minotaur.Core;
using System.Collections.Generic;

namespace Minotaur.Graphics.Primitives
{
  public class Axis : IPrimitive
  {
    private Color4 _xColor;
    private Color4 _yColor;
    private Color4 _zColor;
    private float _length;
    private float _headWidth;

    private class DistanceToCameraComparer : IComparer<Vector3>
    {
      private Vector3 _cameraPosition;

      public DistanceToCameraComparer(GraphicsDevice graphicsDevice)
      {
        _cameraPosition = graphicsDevice.Camera.View.Translation();
      }

      public int Compare(Vector3 x, Vector3 y)
      {
        return (_cameraPosition - x).LengthSquared.CompareTo((_cameraPosition - y).LengthSquared);
      }
    }

    public OpenTK.Graphics.OpenGL.BeginMode BeginMode
    {
      get { return OpenTK.Graphics.OpenGL.BeginMode.Lines; }
    }

    public Axis(float length = 1f,
      float headWidth = 0f,
      Color4? colorX = null,
      Color4? colorY = null,
      Color4? colorZ = null)
    {
      _length = length;
      _headWidth = headWidth;
      _xColor = colorX.HasValue ? colorX.Value : new Color4(1f, 0f, 0f, 1f);
      _yColor = colorY.HasValue ? colorY.Value : new Color4(0f, 1f, 0f, 1f);
      _zColor = colorZ.HasValue ? colorZ.Value : new Color4(0f, 0f, 1f, 1f);
    }

    public VertexPositionColor[] GetVertices(GraphicsDevice graphicsDevice, Matrix4 world, Color4 color, out ushort[] indices)
    {
      indices = new ushort[18];
      VertexPositionColor[] vertices = new VertexPositionColor[12];
      ushort[] tmpIndices;
      int count = 0;
      foreach (var item in new[] { new { Dir = Vector3.UnitX, Color = _xColor }, new { Dir = Vector3.UnitY, Color = _yColor }, new { Dir = Vector3.UnitZ, Color = _zColor } }.OrderByDescending(i=> i.Dir, new DistanceToCameraComparer(graphicsDevice)))
      {
        new Arrow(new Vector3(0), item.Dir * _length, _headWidth, item.Color).GetVertices(graphicsDevice, world, color, out tmpIndices).CopyTo(vertices, count * 4);
        tmpIndices.Select(i => (ushort)(i + count * 4)).ToArray().CopyTo(indices, count * 6);
        count++;
      }
      return vertices;
    }
  }
}
