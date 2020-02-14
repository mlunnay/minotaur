using System;
using OpenTK;

namespace Minotaur.Helpers
{
  public static class QuaternionHelper
  {
    public static Quaternion FromMatrix(Matrix4 m)
    {
      Quaternion q = new Quaternion();
      float num8 = (m.M11 + m.M22) + m.M33;
      if (num8 > 0f)
      {
        float num = (float)Math.Sqrt((double)(num8 + 1f));
        q.W = num * 0.5f;
        num = 0.5f / num;
        q.X = (m.M23 - m.M32) * num;
        q.Y = (m.M31 - m.M13) * num;
        q.Z = (m.M12 - m.M21) * num;
        return q;
      }
      if ((m.M11 >= m.M22) && (m.M11 >= m.M33))
      {
        float num7 = (float)Math.Sqrt((double)(((1f + m.M11) - m.M22) - m.M33));
        float num4 = 0.5f / num7;
        q.X = 0.5f * num7;
        q.Y = (m.M12 + m.M21) * num4;
        q.Z = (m.M13 + m.M31) * num4;
        q.W = (m.M23 - m.M32) * num4;
        return q;
      }
      if (m.M22 > m.M33)
      {
        float num6 = (float)Math.Sqrt((double)(((1f + m.M22) - m.M11) - m.M33));
        float num3 = 0.5f / num6;
        q.X = (m.M21 + m.M12) * num3;
        q.Y = 0.5f * num6;
        q.Z = (m.M32 + m.M23) * num3;
        q.W = (m.M31 - m.M13) * num3;
        return q;
      }
      float num5 = (float)Math.Sqrt((double)(((1f + m.M33) - m.M11) - m.M22));
      float num2 = 0.5f / num5;
      q.X = (m.M31 + m.M13) * num2;
      q.Y = (m.M32 + m.M23) * num2;
      q.Z = 0.5f * num5;
      q.W = (m.M12 - m.M21) * num2;

      return q;
    }

    public static Quaternion RotationBetweenVectors(Vector3 start, Vector3 dest)
    {
      start.Normalize();
      dest.Normalize();

      float cosTheta = Vector3.Dot(start, dest);
      Vector3 rotationAxis;

      if (cosTheta < -1.001f)
      {
        // special case when vectors in opposite directions:
        // there is no "ideal" rotation axis
        // so guess one; any will do as long as its perpendicular to start
        rotationAxis = Vector3.Cross(Vector3.UnitZ, start);
        if (rotationAxis.LengthSquared < 0.01f)  // bad luck, they were parallel try again
          rotationAxis = Vector3.Cross(Vector3.UnitY, start);

        rotationAxis.Normalize();
        return Quaternion.FromAxisAngle(rotationAxis, (float)Math.PI);
      }

      rotationAxis = Vector3.Cross(start, dest);

      float s = (float)Math.Sqrt((1 + cosTheta) * 2);
      float invs = 1 / s;

      return new Quaternion(rotationAxis.X * invs, rotationAxis.Y * invs, rotationAxis.Z * invs, s * 0.5f);
    }
  }
}
