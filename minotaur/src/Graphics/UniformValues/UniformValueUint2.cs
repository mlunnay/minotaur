using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueUint2 : IUniformValue
  {
    uint X;
    uint Y;

    public UniformValueUint2(uint x, uint y)
    {
      X = x;
      Y = y;
    }

    public void Apply(int slot)
    {
      GL.Uniform2(slot, X, Y);
    }

    public override bool Equals(object obj)
    {
      UniformValueUint2 o = obj as UniformValueUint2;
      if(o == null) return false;
      return X == o.X && Y == o.Y;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + X.GetHashCode();
      hash = hash * 37 + Y.GetHashCode();
      return hash;
    }

    public IUniformValue Default()
    {
      return new UniformValueUint2(0, 0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueUint2 other = source as UniformValueUint2;
      if(other == null) return;
      X = other.X;
      Y = other.Y;
    }

    public object Clone()
    {
      return new UniformValueUint2(X, Y);
    }
  }
}
