using System;
using System.Collections.Generic;
using OpenTK;

namespace Minotaur.Helpers
{
  /// <summary>
  /// Helper class to interpolate common types.
  /// </summary>
  public static class LerpHelper
  {
    public static byte Lerp(byte x, byte y, float amount)
    {
      return (byte)(x * (1 - amount) + y * amount);
    }

    public static char Lerp(char x, char y, float amount)
    {
      return (char)(x * (1 - amount) + y * amount);
    }

    public static int Lerp(int x, int y, float amount)
    {
      return (int)(x * (1 - amount) + y * amount);
    }

    public static short Lerp(short x, short y, float amount)
    {
      return (short)(x * (1 - amount) + y * amount);
    }

    public static long Lerp(long x, long y, float amount)
    {
      return (long)(x * (1 - amount) + y * amount);
    }

    public static double Lerp(double x, double y, float amount)
    {
      return x * (1 - amount) + y * amount;
    }

    public static decimal Lerp(decimal x, decimal y, float amount)
    {
      return x * (decimal)(1 - amount) + y * (decimal)amount;
    }

    public static bool Lerp(bool x, bool y, float amount)
    {
      return amount < 0.5f ? x : y;
    }

    //public static Rectangle Lerp(Rectangle x, Rectangle y, float amount)
    //{
    //  Rectangle rc = new Rectangle();

    //  rc.X = Lerp(x.X, y.X, amount);
    //  rc.Y = Lerp(x.Y, y.Y, amount);
    //  rc.Width = Lerp(x.Width, y.Width, amount);
    //  rc.Height = Lerp(x.Height, y.Height, amount);

    //  return rc;
    //}

    //public static Point Lerp(Point x, Point y, float amount)
    //{
    //  Point rc = new Point();

    //  rc.X = Lerp(x.X, y.X, amount);
    //  rc.Y = Lerp(x.Y, y.Y, amount);

    //  return rc;
    //}

    //public static Ray Lerp(Ray x, Ray y, float amount)
    //{
    //  Ray r;

    //  r.Position = Vector3.Lerp(x.Position, y.Position, amount);
    //  r.Direction = Vector3.Lerp(x.Direction, y.Direction, amount);

    //  return r;
    //}

    /// <summary>
    /// Roughly decomposes two matrices and performs spherical linear interpolation
    /// </summary>
    /// <param name="a">Source matrix for interpolation</param>
    /// <param name="b">Destination matrix for interpolation</param>
    /// <param name="amount">Ratio of interpolation</param>
    /// <returns>The interpolated matrix</returns>
    public static Matrix4 Slerp(Matrix4 a, Matrix4 b, float amount)
    {
      throw new NotImplementedException();
      //Quaternion rotationA, rotationB, rotation;
      //Vector3 scaleA, scaleB, scale;
      //Vector3 translationA, translationB, translation;
      //Matrix4 mxScale, mxRotation;
      
      //if (a.Decompose(out scaleA, out  rotationA, out translationA) &&
      //    b.Decompose(out scaleB, out  rotationB, out translationB))
      //{
      //  Vector3.Lerp(ref scaleA, ref scaleB, amount, out scale);
      //  Vector3.Lerp(ref translationA, ref translationB, amount, out translation);
      //  rotation = Quaternion.Slerp(rotationA, rotationB, amount);

      //  mxScale = Matrix4.Scale(scale);
      //  mxRotation = Matrix4.Rotate(rotation);
      //  Matrix4.Mult(ref mxScale, ref mxRotation, out mxRotation);

      //  mxRotation.M41 += translation.X;
      //  mxRotation.M42 += translation.Y;
      //  mxRotation.M43 += translation.Z;
      //  return mxRotation;
      //}

      //throw new InvalidOperationException("Target matrix cannot be decomposed.");
    }

    /// <summary>
    /// Implemements a weighted sum algorithm of quaternions.
    /// See http://en.wikipedia.org/wiki/Generalized_quaternion_interpolation for detailed math formulars.
    /// </summary>
    public static Quaternion WeightedSum(IEnumerable<Quaternion> quaternions, IEnumerable<float> weights, int steps)
    {
      throw new NotImplementedException();
    }
  }
}
