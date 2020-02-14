using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Minotaur.Graphics
{
  public struct Viewport
  {
    private int _x;
    private int _y;
    private int _width;
    private int _height;
    private float _minDepth;
    private float _maxDepth;

    public int X
    {
      get { return _x; }
      set { _x = value; }
    }

    public int Y
    {
      get { return _y; }
      set { _y = value; }
    }

    public int Width
    {
      get { return _width; }
      set { _width = value; }
    }

    public int Height
    {
      get { return _height; }
      set { _height = value; }
    }

    public float MinDepth
    {
      get { return _minDepth; }
      set { _minDepth = value; }
    }

    public float MaxDepth
    {
      get { return _maxDepth; }
      set { _maxDepth = value; }
    }

    public float AspectRatio
    {
      get
      {
        if ((_height != 0) && (_width != 0))
        {
          return (((float)_width) / ((float)_height));
        }
        return 0f;
      }
    }

    public Viewport(int x, int y, int width, int height)
    {
      _x = x;
      _y = y;
      _width = width;
      _height = height;
      _minDepth = -1f;
      _maxDepth = 1f;
    }

    public Vector3 Project(Vector3 source, Matrix4 projection, Matrix4 view, Matrix4 world)
    {
      Matrix4 matrix = Matrix4.Mult(Matrix4.Mult(world, view), projection);
      Vector3 vector = Vector3.Transform(source, matrix);
      float a = (((source.X * matrix.M41) + (source.Y * matrix.M42)) + (source.Z * matrix.M43)) + matrix.M44;
      if (!WithinEpsilon(a, 1f))
      {
        vector = (Vector3)(vector / a);
      }
      vector.X = (((vector.X + 1f) * 0.5f) * _width) + _x;
      vector.Y = (((-vector.Y + 1f) * 0.5f) * _height) + _y;
      vector.Z = (vector.Z * (_maxDepth - _minDepth)) + _minDepth;
      return vector;
    }

    public Vector3 Unproject(Vector3 source, Matrix4 projection, Matrix4 view, Matrix4 world)
    {
      Matrix4 matrix = Matrix4.Invert(Matrix4.Mult(Matrix4.Mult(world, view), projection));
      source.X = (((source.X - _x) / ((float)_width)) * 2f) - 1f;
      source.Y = -((((source.Y - _y) / ((float)_height)) * 2f) - 1f);
      source.Z = (source.Z - _minDepth) / (_maxDepth - _minDepth);
      Vector3 vector = Vector3.Transform(source, matrix);
      float a = (((source.X * matrix.M41) + (source.Y * matrix.M42)) + (source.Z * matrix.M43)) + matrix.M44;
      if (!WithinEpsilon(a, 1f))
      {
        vector = (Vector3)(vector / a);
      }
      return vector;

    }

    public Vector4 GetAsVector4()
    {
      return new Vector4(_x, _y, _width, _height);
    }

    private static bool WithinEpsilon(float a, float b)
    {
      float num = a - b;
      return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
    }
  }
}
