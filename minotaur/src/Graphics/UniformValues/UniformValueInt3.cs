using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueInt3 : IUniformValue
  {
    int X;
    int Y;
    int Z;

    public UniformValueInt3(int x, int y, int z)
    {
      X = x;
      Y = y;
      Z = z;
    }

    public void Apply(int slot)
    {
      GL.Uniform3(slot, X, Y, Z);
    }

    public override bool Equals(object obj)
    {
      UniformValueInt3 o = obj as UniformValueInt3;
      if(o == null) return false;
      return X == o.X && Y == o.Y && Z == o.Z;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + X.GetHashCode();
      hash = hash * 37 + Y.GetHashCode();
      hash = hash * 37 + Z.GetHashCode();
      return hash;
    }

    public IUniformValue Default()
    {
      return new UniformValueInt3(0, 0, 0);
    }

    public void Set(IUniformValue source)
    {
      UniformValueInt3 other = source as UniformValueInt3;
      if(other == null) return;
      X = other.X;
      Y = other.Y;
      Z = other.Z;
    }

    public object Clone()
    {
      return new UniformValueInt3(X, Y, Z);
    }
  }
}
