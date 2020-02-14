using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueInt1 : IUniformValue
  {
    int X;

    public UniformValueInt1(int x)
    {
      X = x;
    }

    public void Apply(int slot)
    {
      GL.Uniform1(slot, X);
    }

    public override bool Equals(object obj)
    {
      UniformValueInt1 o = obj as UniformValueInt1;
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
      return new UniformValueInt1(0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueInt1 other = source as UniformValueInt1;
      if(other == null) return;
      X = other.X;
    }

    public object Clone()
    {
      return new UniformValueInt1(X);
    }
  }
}
