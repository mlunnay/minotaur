using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueInt4 : IUniformValue
  {
    int X;
    int Y;
    int Z;
    int W;

    public UniformValueInt4(int x, int y, int z, int w)
    {
      X = x;
      Y = y;
      Z = z;
      W = w;
    }

    public void Apply(int slot)
    {
      GL.Uniform4(slot, X, Y, Z, W);
    }

    public override bool Equals(object obj)
    {
      UniformValueInt4 o = obj as UniformValueInt4;
      if(o == null) return false;
      return X == o.X && Y == o.Y && Z == o.Z && W == o.W;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + X.GetHashCode();
      hash = hash * 37 + Y.GetHashCode();
      hash = hash * 37 + Z.GetHashCode();
      hash = hash * 37 + W.GetHashCode();
      return hash;
    }

    public IUniformValue Default()
    {
      return new UniformValueInt4(0, 0, 0, 0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueInt4 other = source as UniformValueInt4;
      if(other == null) return;
      X = other.X;
      Y = other.Y;
      Z = other.Z;
      W = other.W;
    }

    public object Clone()
    {
      return new UniformValueInt4(X, Y, Z, W);
    }
  }
}
