using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueUint1 : IUniformValue
  {
    uint X;

    public UniformValueUint1(uint x)
    {
      X = x;
    }

    public void Apply(int slot)
    {
      GL.Uniform1(slot, X);
    }

    public override bool Equals(object obj)
    {
      UniformValueUint1 o = obj as UniformValueUint1;
      if(o == null) return false;
      return X == o.X;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + X.GetHashCode();
      return hash;
    }

    public IUniformValue Default()
    {
      return new UniformValueUint1(0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueUint1 other = source as UniformValueUint1;
      if(other == null) return;
      X = other.X;
    }

    public object Clone()
    {
      return new UniformValueUint1(X);
    }
  }
}
