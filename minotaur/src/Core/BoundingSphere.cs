using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Minotaur.Core
{
  public struct BoundingSphere : IEquatable<BoundingSphere>
  {
    public Vector3 Center;
    public float Radius;

    public BoundingSphere(Vector3 center, float radius)
    {
      Center = center;
      Radius = radius;
    }

    public bool Equals(BoundingSphere s)
    {
      return Center == s.Center && Radius == s.Radius;
    }

    public override bool Equals(object obj)
    {
      bool result = false;
      if (obj is BoundingSphere)
        result = Equals((BoundingSphere)obj);
      return result;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + Center.GetHashCode();
      hash = hash * 37 + Radius.GetHashCode();
      return hash;
    }

    public override string ToString()
    {
      return string.Format("{BoundingSphere Center:{0} Radius:{1}}", Center, Radius);
    }

    public static bool operator ==(BoundingSphere lhs, BoundingSphere rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(BoundingSphere lhs, BoundingSphere rhs)
    {
      return !lhs.Equals(rhs);
    }

    public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
    {
      BoundingSphere result;
      Vector3.Lerp(ref box.Min, ref box.Max, 0.5f, out result.Center);
      result.Radius = (box.Min - box.Max).Length * 0.5f;
      return result;
    }

    public static void CreateFromBoundingBox(ref BoundingBox box, out BoundingSphere result)
    {
      Vector3.Lerp(ref box.Min, ref box.Max, 0.5f, out result.Center);
      result.Radius = (box.Min - box.Max).Length * 0.5f;
    }

    public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
    {
      // this is by far not the most efficient method.
      // For points that are also ICollections .Count() just returns points.Count, otherwise it loops the whole enumerable and manually counts the items.
      int numPoints = points.Count();
      Vector3 center = new Vector3();
      foreach (Vector3 point in points)
      {
        center += point / numPoints;
      }
      float radius = 0;
      foreach (Vector3 point in points)
      {
        float distance = (point - center).Length;
        if (distance > radius)
          radius = distance;
      }
      return new BoundingSphere(center, radius);
    }

    public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
    {
      Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
      float distance = ocenterToaCenter.Length;
      if (distance <= original.Radius + additional.Radius)//intersect
      {
        if (distance <= original.Radius - additional.Radius)//original contain additional
          return original;
        if (distance <= additional.Radius - original.Radius)//additional contain original
          return additional;
      }

      //else find center of new sphere and radius
      float leftRadius = Math.Max(original.Radius - distance, additional.Radius);
      float Rightradius = Math.Max(original.Radius + distance, additional.Radius);
      ocenterToaCenter = ocenterToaCenter + (((leftRadius - Rightradius) / (2 * ocenterToaCenter.Length)) * ocenterToaCenter);//oCenterToResultCenter

      BoundingSphere result = new BoundingSphere();
      result.Center = original.Center + ocenterToaCenter;
      result.Radius = (leftRadius + Rightradius) / 2;
      return result;
    }

    public static void CreateMerged(ref BoundingSphere original, ref BoundingSphere additional, out BoundingSphere result)
    {
      result = CreateMerged(original, additional);
    }

    public BoundingSphere Transform(Matrix4 matrix)
    {
      BoundingSphere sphere = new BoundingSphere();
      sphere.Center = Vector3.Transform(this.Center, matrix);
      sphere.Radius = this.Radius * ((float)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
      return sphere;
    }

    public void Transform(ref Matrix4 matrix, out BoundingSphere result)
    {
      result = new BoundingSphere();
      result.Center = Vector3.Transform(this.Center, matrix);
      result.Radius = this.Radius * ((float)Math.Sqrt((double)Math.Max(((matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12)) + (matrix.M13 * matrix.M13), Math.Max(((matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22)) + (matrix.M23 * matrix.M23), ((matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32)) + (matrix.M33 * matrix.M33)))));
    }

    public ContainmentType Contains(BoundingBox box)
    {
      bool inside = true;
      foreach (Vector3 v in box.GetCorners())
      {
        if (Contains(v) == ContainmentType.Disjoint)
        {
          inside = false;
          break;
        }
      }
      if (inside)
        return ContainmentType.Contains;

      //check if the distance from sphere center to cube face < radius
      double dmin = 0;

      if (Center.X < box.Min.X)
        dmin += (Center.X - box.Min.X) * (Center.X - box.Min.X);
      else if (Center.X > box.Max.X)
        dmin += (Center.X - box.Max.X) * (Center.X - box.Max.X);
      
      if (Center.Y < box.Min.Y)
        dmin += (Center.Y - box.Min.Y) * (Center.Y - box.Min.Y);
      else if (Center.Y > box.Max.Y)
        dmin += (Center.Y - box.Max.Y) * (Center.Y - box.Max.Y);

      if (Center.Z < box.Min.Z)
        dmin += (Center.Z - box.Min.Z) * (Center.Z - box.Min.Z);
      else if (Center.Z > box.Max.Z)
        dmin += (Center.Z - box.Max.Z) * (Center.Z - box.Max.Z);

      if (dmin <= Radius * Radius)
        return ContainmentType.Intersects;

      //else disjoint
      return ContainmentType.Disjoint;
    }

    public void Contains(ref BoundingBox box, out ContainmentType result)
    {
      result = Contains(box);
    }

    public ContainmentType Contains(BoundingSphere sphere)
    {
      float val = (sphere.Center - Center).Length;

      if (val > sphere.Radius + Radius)
        return ContainmentType.Disjoint;

      else if (val <= Radius - sphere.Radius)
        return ContainmentType.Contains;

      else
        return ContainmentType.Intersects;
    }

    public void Contains(ref BoundingSphere sphere, out ContainmentType result)
    {
      result = Contains(sphere);
    }

    public ContainmentType Contains(Vector3 v)
    {
      float distance = (Center - v).Length;
      if (distance < Radius)
        return ContainmentType.Contains;
      else if (distance > Radius)
        return ContainmentType.Disjoint;
      return ContainmentType.Intersects;
    }

    public void Contains(ref Vector3 v, out ContainmentType result)
    {
      result = Contains(v);
    }

    public bool Intersects(BoundingBox box)
    {
      return box.Intersects(this);
    }

    public void Intersects(ref BoundingBox box, out bool result)
    {
      result = box.Intersects(this);
    }

    public bool Intersects(BoundingSphere sphere)
    {
      float distSquared = (Center - sphere.Center).LengthSquared;
      return Radius * Radius + 2f * Radius * sphere.Radius + sphere.Radius * sphere.Radius > distSquared;
    }

    public void Intersects(ref BoundingSphere sphere, out bool result)
    {
      float distSquared = (Center - sphere.Center).LengthSquared;
      result = Radius * Radius + 2f * Radius * sphere.Radius + sphere.Radius * sphere.Radius > distSquared;
    }

    public PlaneIntersectionType Intersects(Plane plane)
    {
      float distance;
      Vector3.Dot(ref plane.Normal, ref this.Center, out distance);
      distance += plane.D;
      if (distance > this.Radius)
        return PlaneIntersectionType.Front;
      else if (distance < -this.Radius)
        return PlaneIntersectionType.Back;
      else
        return PlaneIntersectionType.Intersecting;
    }

    public void Intersects(ref Plane plane, out PlaneIntersectionType result)
    {
      float distance;
      Vector3.Dot(ref plane.Normal, ref this.Center, out distance);
      distance += plane.D;
      if (distance > this.Radius)
        result = PlaneIntersectionType.Front;
      else if (distance < -this.Radius)
        result = PlaneIntersectionType.Back;
      else
        result = PlaneIntersectionType.Intersecting;
    }

    public float? Intersects(Ray ray)
    {
      return ray.Intersects(this);
    }

    public void Intersects(ref Ray ray, out float? result)
    {
      ray.Intersects(ref this, out result);
    }
  }
}
