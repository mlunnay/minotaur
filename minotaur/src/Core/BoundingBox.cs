using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minotaur.Core
{
  public struct BoundingBox : IEquatable<BoundingBox>
  {
    public Vector3 Min;
    public Vector3 Max;

    public BoundingBox(Vector3 min, Vector3 max)
    {
      Min = min;
      Max = max;
    }

    public bool Equals(BoundingBox other)
    {
      return Min == other.Min && Max == other.Max;
    }

    public override bool Equals(object obj)
    {
      bool result = false;
      if (obj is BoundingBox)
      {
        result = this.Equals((BoundingBox)obj);
      }
      return result;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + Min.GetHashCode();
      hash = hash * 37 + Max.GetHashCode();
      return hash;
    }

    public static bool operator ==(BoundingBox a, BoundingBox b) 
    {
      return a.Equals(b);
    }

    public static bool operator !=(BoundingBox a, BoundingBox b)
    {
      return a.Min != b.Min || a.Max != b.Max;
    }

    public override string ToString()
    {
      return string.Format("{Min:{0} Max:{1}}", Min, Max);
    }

    public static BoundingBox CreateMerged(BoundingBox b1, BoundingBox b2)
    {
      BoundingBox result;
      result.Min = Vector3.Min(b1.Min, b2.Min);
      result.Max = Vector3.Max(b1.Max, b2.Max);
      return result;
    }

    public static void CreateMerged(ref BoundingBox b1, ref BoundingBox b2, out BoundingBox result)
    {
      result.Min = Vector3.Min(b1.Min, b2.Min);
      result.Max = Vector3.Max(b1.Max, b2.Max);
    }

    //public static BoundingBox CreateFromSphere(BoundingSphere sphere)
    //{

    //}

    //public static void CreateFromSphere(ref BoundingSphere sphere, out BoundingBox result)
    //{

    //}

    public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count() == 0)
        throw new ArgumentException("points does not contain any points.");
      Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
      Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
      foreach (Vector3 point in points)
      {
        min = Vector3.Min(min, point);
        max = Vector3.Min(max, point);
      }
      return new BoundingBox(min, max);
    }

    public bool Intersects(BoundingBox other)
    {
      return Max.X >= other.Min.X && Min.X <= other.Max.X && Max.Y >= other.Min.Y && Min.Y <= other.Max.Y && Max.Z >= other.Min.Z && Min.Z <= other.Max.Z;
    }

    public void Intersects(ref BoundingBox other, out bool result)
    {
      result = false;
      if (Max.X < other.Min.X || Min.X > other.Max.X)
      {
        return;
      }
      if (Max.Y < other.Min.Y || Min.Y > other.Max.Y)
      {
        return;
      }
      if (Max.Z < other.Min.Z || Min.Z > other.Max.Z)
      {
        return;
      }
      result = true;
    }

    public PlaneIntersectionType Intersects(Plane plane)
    {
      return plane.Intersects(this);
    }

    public void Intersects(ref Plane plane, out PlaneIntersectionType result)
    {
      plane.Intersects(ref this, out result);
    }

    public bool Intersects(BoundingSphere sphere)
    {
      Vector3 v;
      Vector3.Clamp(ref sphere.Center, ref Min, ref Max, out v);
      return ((sphere.Center - v).LengthSquared <= sphere.Radius * sphere.Radius);
    }

    public void Intersects(ref BoundingSphere sphere, out bool result)
    {
      Vector3 v;
      Vector3.Clamp(ref sphere.Center, ref Min, ref Max, out v);
      result = ((sphere.Center - v).LengthSquared <= sphere.Radius * sphere.Radius);
    }

    public float? Intersects(Ray ray)
    {
      Vector3[] bounds = new Vector3[] { Min, Max };
      float tmin = (bounds[ray.Sign[0]].X - ray.Position.X) * ray.InvDirection.X;
      float tmax = (bounds[1 - ray.Sign[0]].X - ray.Position.X) * ray.InvDirection.X;
      float tymin = (bounds[ray.Sign[1]].Y - ray.Position.Y) * ray.InvDirection.Y;
      float tymax = (bounds[1 - ray.Sign[1]].Y - ray.Position.Y) * ray.InvDirection.Y;
      if (tmin > tymax || tymin > tmax)
        return null;
      if (tymin > tmin)
        tmin = tymin;
      if (tymax < tmax)
        tmax = tymax;
      float tzmin = (bounds[ray.Sign[2]].Z - ray.Position.Z) * ray.InvDirection.Z;
      float tzmax = (bounds[1 - ray.Sign[2]].Z - ray.Position.Z) * ray.InvDirection.Z;
      if (tmin > tzmax || tzmin > tmax)
        return null;
      if (tzmin > tmin)
        tmin = tzmin;
      return new float?(tmin);
    }

    public void Intersects(ref Ray ray, out float? result)
    {
      result = null;
      Vector3[] bounds = new Vector3[] { Min, Max };
      float tmin = (bounds[ray.Sign[0]].X - ray.Position.X) * ray.InvDirection.X;
      float tmax = (bounds[1 - ray.Sign[0]].X - ray.Position.X) * ray.InvDirection.X;
      float tymin = (bounds[ray.Sign[1]].Y - ray.Position.Y) * ray.InvDirection.Y;
      float tymax = (bounds[1 - ray.Sign[1]].Y - ray.Position.Y) * ray.InvDirection.Y;
      if (tmin > tymax || tymin > tmax)
        return;
      if (tymin > tmin)
        tmin = tymin;
      if (tymax < tmax)
        tmax = tymax;
      float tzmin = (bounds[ray.Sign[2]].Z - ray.Position.Z) * ray.InvDirection.Z;
      float tzmax = (bounds[1 - ray.Sign[2]].Z - ray.Position.Z) * ray.InvDirection.Z;
      if (tmin > tzmax || tzmin > tmax)
        return;
      if (tzmin > tmin)
        tmin = tzmin;
      result = new float?(tmin);
    }

    public ContainmentType Contains(BoundingBox box)
    {
      if (Max.X < box.Min.X || Min.X > box.Max.X)
      {
        return ContainmentType.Disjoint;
      }
      if (Max.Y < box.Min.Y || Min.Y > box.Max.Y)
      {
        return ContainmentType.Disjoint;
      }
      if (Max.Z < box.Min.Z || Min.Z > box.Max.Z)
      {
        return ContainmentType.Disjoint;
      }
      if (Min.X > box.Min.X || box.Max.X > Max.X || Min.Y > box.Min.Y || box.Max.Y > Max.Y || Min.Z > box.Min.Z || box.Max.Z > Max.Z)
      {
        return ContainmentType.Intersects;
      }
      return ContainmentType.Contains;
    }

    public void Contains(ref BoundingBox box, out ContainmentType result)
    {
      if (Max.X < box.Min.X || Min.X > box.Max.X)
      {
        result = ContainmentType.Disjoint;
      }
      else if (Max.Y < box.Min.Y || Min.Y > box.Max.Y)
      {
        result = ContainmentType.Disjoint;
      }
      else if (Max.Z < box.Min.Z || Min.Z > box.Max.Z)
      {
        result = ContainmentType.Disjoint;
      }
      else if (Min.X > box.Min.X || box.Max.X > Max.X || Min.Y > box.Min.Y || box.Max.Y > Max.Y || Min.Z > box.Min.Z || box.Max.Z > Max.Z)
      {
        result = ContainmentType.Intersects;
      }
      else
        result = ContainmentType.Contains;
    }

    public ContainmentType Contains(Vector3 point)
    {
      if (Min.X > point.X || point.X > Max.X || Min.Y > point.Y || point.Y > Max.Y || Min.Z > point.Z || point.Z > Max.Z)
        return ContainmentType.Disjoint;
      return ContainmentType.Contains;
    }

    public void Contains(ref Vector3 point, out ContainmentType result)
    {
      if (Min.X > point.X || point.X > Max.X || Min.Y > point.Y || point.Y > Max.Y || Min.Z > point.Z || point.Z > Max.Z)
        result = ContainmentType.Disjoint;
      else
        result = ContainmentType.Contains;
    }

    public ContainmentType Contains(BoundingSphere sphere)
    {
      Vector3 vector;
      Vector3.Clamp(ref sphere.Center, ref Min, ref Max, out vector);
      float num = (sphere.Center - vector).LengthSquared;
      if (num > sphere.Radius * sphere.Radius)
        return ContainmentType.Disjoint;
      if (Min.X + sphere.Radius > sphere.Center.X || sphere.Center.X > Max.X - sphere.Radius || Max.X - Min.X <= sphere.Radius || Min.Y + sphere.Radius > sphere.Center.Y || sphere.Center.Y > Max.Y - sphere.Radius || Max.Y - Min.Y <= sphere.Radius || Min.Z + sphere.Radius > sphere.Center.Z || sphere.Center.Z > Max.Z - sphere.Radius || Max.Z - Min.Z <= sphere.Radius)
        return ContainmentType.Intersects;
      return ContainmentType.Contains;
    }

    public void Contains(ref BoundingSphere sphere, out ContainmentType result)
    {
      result = Contains(sphere);
    }

    public Vector3[] GetCorners()
    {
      return new Vector3[] {
                new Vector3(Min.X, Max.Y, Max.Z),
                new Vector3(Max.X, Max.Y, Max.Z),
                new Vector3(Max.X, Min.Y, Max.Z),
                new Vector3(Min.X, Min.Y, Max.Z),
                new Vector3(Min.X, Max.Y, Min.Z),
                new Vector3(Max.X, Max.Y, Min.Z),
                new Vector3(Max.X, Min.Y, Min.Z),
                new Vector3(Min.X, Min.Y, Min.Z)
            };
    }
  }
}
