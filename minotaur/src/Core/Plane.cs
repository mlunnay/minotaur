using System;
using OpenTK;

namespace Minotaur.Core
{
  public enum PlaneIntersectionType
  {
    Front,
    Back,
    Intersecting
  }

  public struct Plane : IEquatable<Plane>
  {
    public Vector3 Normal;
    public float D;

    public Plane(float a, float b, float c, float d)
    {
      Normal = new Vector3(a, b, c);
      D = d;
    }

    public Plane(Vector3 normal, float d)
    {
      Normal = normal;
      D = d;
    }

    public Plane(Vector4 v)
    {
      Normal = new Vector3(v.X, v.Y, v.Z);
      D = v.W;
    }

    /// <summary>
    /// Construct a plane from 3 vectors.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    public Plane(Vector3 v0, Vector3 v1, Vector3 v2)
    {
      Vector3 dist1 = v1 - v0;
      Vector3 dist2 = v2 - v0;

      Normal = Vector3.Normalize(Vector3.Cross(dist1, dist2));
      D = Vector3.Dot(Normal, v0);
    }

    public bool Equals(Plane other)
    {
      return Normal == other.Normal && D == other.D;
    }

    public override bool Equals(Object other)
    {
      bool result = false;
      if (other is Plane)
      {
        result = Equals((Plane)other);
      }
      return result;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + Normal.GetHashCode();
      hash = hash * 37 + D.GetHashCode();
      return hash;
    }

    public static bool operator ==(Plane lhs, Plane rhs) 
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Plane lhs, Plane rhs) 
    {
      return !lhs.Equals(rhs);
    }

    public PlaneIntersectionType Intersects(BoundingBox box)
    {
      Vector3 vector;
      vector.X = ((Normal.X >= 0f) ? box.Min.X : box.Max.X);
      vector.Y = ((Normal.Y >= 0f) ? box.Min.Y : box.Max.Y);
      vector.Z = ((Normal.Z >= 0f) ? box.Min.Z : box.Max.Z);
      Vector3 vector2;
      vector2.X = ((Normal.X >= 0f) ? box.Max.X : box.Min.X);
      vector2.Y = ((Normal.Y >= 0f) ? box.Max.Y : box.Min.Y);
      vector2.Z = ((Normal.Z >= 0f) ? box.Max.Z : box.Min.Z);
      float num = Normal.X * vector.X + Normal.Y * vector.Y + Normal.Z * vector.Z;
      if (num + D > 0f)
      {
        return PlaneIntersectionType.Front;
      }
      num = Normal.X * vector2.X + Normal.Y * vector2.Y + Normal.Z * vector2.Z;
      if (num + D < 0f)
      {
        return PlaneIntersectionType.Back;
      }
      return PlaneIntersectionType.Intersecting;
    }

    public void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
    {
      Vector3 vector;
      vector.X = ((Normal.X >= 0f) ? box.Min.X : box.Max.X);
      vector.Y = ((Normal.Y >= 0f) ? box.Min.Y : box.Max.Y);
      vector.Z = ((Normal.Z >= 0f) ? box.Min.Z : box.Max.Z);
      Vector3 vector2;
      vector2.X = ((Normal.X >= 0f) ? box.Max.X : box.Min.X);
      vector2.Y = ((Normal.Y >= 0f) ? box.Max.Y : box.Min.Y);
      vector2.Z = ((Normal.Z >= 0f) ? box.Max.Z : box.Min.Z);
      float num = Normal.X * vector.X + Normal.Y * vector.Y + Normal.Z * vector.Z;
      if (num + D > 0f)
      {
        result = PlaneIntersectionType.Front;
        return;
      }
      num = Normal.X * vector2.X + Normal.Y * vector2.Y + Normal.Z * vector2.Z;
      if (num + D < 0f)
      {
        result = PlaneIntersectionType.Back;
        return;
      }
      result = PlaneIntersectionType.Intersecting;
    }

    //public PlaneIntersectionType Intersects(BoundingSphere sphere)
    //{

    //}

    //public void Intersects(BoundingSphere sphere, PlaneIntersectionType result)
    //{

    //}
  }
}
