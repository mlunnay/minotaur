using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueFloat2 : IUniformValue
  {
    float X;
    float Y;

    public UniformValueFloat2(float x, float y)
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
      UniformValueFloat2 o = obj as UniformValueFloat2;
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
      return new UniformValueFloat2(0, 0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueFloat2 other = source as UniformValueFloat2;
      if(other == null) return;
      X = other.X;
      Y = other.Y;
    }

    public object Clone()
    {
      return new UniformValueFloat2(X, Y);
    }
  }
}
