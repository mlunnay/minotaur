using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueFloat1 : IUniformValue
  {
    float X;

    public UniformValueFloat1(float x)
    {
      X = x;
    }

    public void Apply(int slot)
    {
      GL.Uniform1(slot, X);
    }

    public override bool Equals(object obj)
    {
      UniformValueFloat1 o = obj as UniformValueFloat1;
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
      return new UniformValueFloat1(0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueFloat1 other = source as UniformValueFloat1;
      if(other == null) return;
      X = other.X;
    }

    public object Clone()
    {
      return new UniformValueFloat1(X);
    }
  }
}
