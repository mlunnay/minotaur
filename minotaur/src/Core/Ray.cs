using System;
using OpenTK;

namespace Minotaur.Core
{
  public struct Ray : IEquatable<Ray>
  {
    public Vector3 Position;
    private Vector3 _direction;
    public Vector3 InvDirection;
    public byte[] Sign;

    public Vector3 Direction
    {
      get { return _direction; }
      set
      {
        _direction = value;
        InvDirection = new Vector3(1 / value.X, 1 / value.Y, 1 / value.Z);
        Sign[0] = (byte)(InvDirection.X < 0 ? 1 : 0);
        Sign[1] = (byte)(InvDirection.Y < 0 ? 1 : 0);
        Sign[2] = (byte)(InvDirection.Z < 0 ? 1 : 0);
      }
    }

    public Ray(Vector3 position, Vector3 direction)
    {
      Position = position;
      _direction = direction;
      InvDirection = new Vector3(1 / direction.X, 1 / direction.Y, 1 / direction.Z);
      Sign = new byte[3];
      Sign[0] = (byte)(InvDirection.X < 0 ? 1 : 0);
      Sign[1] = (byte)(InvDirection.Y < 0 ? 1 : 0);
      Sign[2] = (byte)(InvDirection.Z < 0 ? 1 : 0);
    }

    public bool Equals(Ray other)
    {
      return Position == other.Position && Direction == other.Direction;
    }

    public override bool Equals(object obj)
    {
      bool result = false;
      if (obj is Ray)
        result = Equals((Ray)obj);
      return result;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + Position.GetHashCode();
      hash = hash * 37 + Direction.GetHashCode();
      return hash;
    }

    public static bool operator ==(Ray lhs, Ray rhs)
    {
      return lhs.Equals(rhs);
    }

    public static bool operator !=(Ray lhs, Ray rhs)
    {
      return !lhs.Equals(rhs);
    }

    public override string ToString()
    {
      return string.Format("{Ray Position:{0} Direction:{1}}", Position, Direction);
    }

    public float? Intersects(BoundingBox box)
    {
      return box.Intersects(this);
    }

    public void Intersects(ref BoundingBox box, out float? result)
    {
      box.Intersects(ref this, out result);
    }

    public float? Intersects(BoundingSphere sphere)
    {
      // Find the vector between where the ray starts the the sphere's centre
      Vector3 difference = sphere.Center - Position;

      float differenceLengthSquared = difference.LengthSquared;
      float sphereRadiusSquared = sphere.Radius * sphere.Radius;

      float distanceAlongRay;

      // If the distance between the ray start and the sphere's centre is less than
      // the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
      if (differenceLengthSquared < sphereRadiusSquared)
      {
        return new float?(0.0f);
      }

      Vector3.Dot(ref _direction, ref difference, out distanceAlongRay);
      // If the ray is pointing away from the sphere then we don't ever intersect
      if (distanceAlongRay < 0)
      {
        return null;
      }

      // Next we kinda use Pythagoras to check if we are within the bounds of the sphere
      // if x = radius of sphere
      // if y = distance between ray position and sphere centre
      // if z = the distance we've travelled along the ray
      // if x^2 + z^2 - y^2 < 0, we do not intersect
      float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;

      return (dist < 0) ? null : distanceAlongRay - (float?)Math.Sqrt(dist);
    }

    public void Intersects(ref BoundingSphere sphere, out float? result)
    {
      // Find the vector between where the ray starts the the sphere's centre
      Vector3 difference = sphere.Center - Position;

      float differenceLengthSquared = difference.LengthSquared;
      float sphereRadiusSquared = sphere.Radius * sphere.Radius;

      float distanceAlongRay;

      // If the distance between the ray start and the sphere's centre is less than
      // the radius of the sphere, it means we've intersected. N.B. checking the LengthSquared is faster.
      if (differenceLengthSquared < sphereRadiusSquared)
      {
        result = new float?(0.0f);
        return;
      }

      Vector3.Dot(ref _direction, ref difference, out distanceAlongRay);
      // If the ray is pointing away from the sphere then we don't ever intersect
      if (distanceAlongRay < 0)
      {
        result = null;
        return;
      }

      // Next we kinda use Pythagoras to check if we are within the bounds of the sphere
      // if x = radius of sphere
      // if y = distance between ray position and sphere centre
      // if z = the distance we've travelled along the ray
      // if x^2 + z^2 - y^2 < 0, we do not intersect
      float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;

      result = (dist < 0) ? null : distanceAlongRay - (float?)Math.Sqrt(dist);
    }
  }
}
