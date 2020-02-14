using System;
using OpenTK;
using OpenTK.Graphics;
namespace Minotaur.Graphics
{
  public static class UniformValue
  {
    public static IUniformValue Create(int[] args)
    {
      if (args.Length == 1)
        return new UniformValueInt1(args[0]);
      else if (args.Length == 2)
        return new UniformValueInt2(args[0], args[1]);
      else if (args.Length == 3)
        return new UniformValueInt3(args[0], args[1], args[2]);
      else if (args.Length == 4)
        return new UniformValueInt4(args[0], args[1], args[2], args[3]);

      throw new ArgumentException("Args must contain between 1 and 4 values.");
    }

    public static UniformValueInt1 Create(int x)
    {
      return new UniformValueInt1(x);
    }

    public static UniformValueInt2 Create(int x, int y)
    {
      return new UniformValueInt2(x, y);
    }

    public static UniformValueInt3 Create(int x, int y, int z)
    {
      return new UniformValueInt3(x, y, z);
    }

    public static UniformValueInt4 Create(int x, int y, int z, int w)
    {
      return new UniformValueInt4(x, y, z, w);
    }

    public static IUniformValue Create(uint[] args)
    {
      if (args.Length == 1)
        return new UniformValueUint1(args[0]);
      else if (args.Length == 2)
        return new UniformValueUint2(args[0], args[1]);
      else if (args.Length == 3)
        return new UniformValueUint3(args[0], args[1], args[2]);
      else if (args.Length == 4)
        return new UniformValueUint4(args[0], args[1], args[2], args[3]);

      throw new ArgumentException("Args must contain between 1 and 4 values.");
    }

    public static UniformValueUint1 Create(uint x)
    {
      return new UniformValueUint1(x);
    }

    public static UniformValueUint2 Create(uint x, uint y)
    {
      return new UniformValueUint2(x, y);
    }

    public static UniformValueUint3 Create(uint x, uint y, uint z)
    {
      return new UniformValueUint3(x, y, z);
    }

    public static UniformValueUint4 Create(uint x, uint y, uint z, uint w)
    {
      return new UniformValueUint4(x, y, z, w);
    }

    public static IUniformValue Create(float[] args)
    {
      if (args.Length == 1)
        return new UniformValueFloat1(args[0]);
      else if (args.Length == 2)
        return new UniformValueFloat2(args[0], args[1]);
      else if (args.Length == 3)
        return new UniformValueFloat3(args[0], args[1], args[2]);
      else if (args.Length == 4)
        return new UniformValueFloat4(args[0], args[1], args[2], args[3]);

      throw new ArgumentException("Args must contain between 1 and 4 values.");
    }

    public static UniformValueFloat1 Create(float x)
    {
      return new UniformValueFloat1(x);
    }

    public static UniformValueFloat2 Create(float x, float y)
    {
      return new UniformValueFloat2(x, y);
    }

    public static UniformValueFloat3 Create(float x, float y, float z)
    {
      return new UniformValueFloat3(x, y, z);
    }

    public static UniformValueFloat4 Create(float x, float y, float z, float w)
    {
      return new UniformValueFloat4(x, y, z, w);
    }

    public static UniformValueMatrix Create(Matrix4 m, bool transpose = false)
    {
      return new UniformValueMatrix(m, transpose);
    }

    public static UniformValueFloat2 Create(Vector2 v)
    {
      return new UniformValueFloat2(v.X, v.Y);
    }

    public static UniformValueFloat3 Create(Vector3 v)
    {
      return new UniformValueFloat3(v.X, v.Y, v.Z);
    }

    public static UniformValueFloat4 Create(Vector4 v)
    {
      return new UniformValueFloat4(v.X, v.Y, v.Z, v.W);
    }

    public static UniformValueFloat4 Create(Color4 c)
    {
      return new UniformValueFloat4(c.R, c.G, c.B, c.A);
    }

    public static UniformValueFloat4 Create(Quaternion q)
    {
      return new UniformValueFloat4(q.X, q.Y, q.Z, q.W);
    }

    public static UniformValueTexture Create(Texture t)
    {
      return new UniformValueTexture(t);
    }

    public static UniformValueSampler Create(Sampler s, int textureSlot = 0)
    {
      return new UniformValueSampler(s, textureSlot);
    }
  }
}
