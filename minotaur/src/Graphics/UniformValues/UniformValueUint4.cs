using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueUint4 : IUniformValue
  {
    uint X;
    uint Y;
    uint Z;
    uint W;

    public UniformValueUint4(uint x, uint y, uint z, uint w)
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
      UniformValueUint4 o = obj as UniformValueUint4;
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
      return new UniformValueUint4(0, 0, 0, 0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueUint4 other = source as UniformValueUint4;
      if(other == null) return;
      X = other.X;
      Y = other.Y;
      Z = other.Z;
      W = other.W;
    }

    public object Clone()
    {
      return new UniformValueUint4(X, Y, Z, W);
    }
  }
}
