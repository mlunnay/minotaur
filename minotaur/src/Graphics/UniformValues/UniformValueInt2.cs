using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueInt2 : IUniformValue
  {
    int X;
    int Y;

    public UniformValueInt2(int x, int y)
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
      UniformValueInt2 o = obj as UniformValueInt2;
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
      return new UniformValueInt2(0, 0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueInt2 other = source as UniformValueInt2;
      if(other == null) return;
      X = other.X;
      Y = other.Y;
    }

    public object Clone()
    {
      return new UniformValueInt2(X, Y);
    }
  }
}
