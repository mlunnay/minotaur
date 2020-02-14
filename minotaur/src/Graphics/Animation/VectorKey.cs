using System;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public class VectorKey : IVectorKey
  {
    private float _time;
    private Vector3 _vector;

    public float Time
    {
      get { return _time; }
    }

    public Vector3 Vector
    {
      get { return _vector; }
    }

    public VectorKey(float time, Vector3 vector)
    {
      _time = time;
      _vector = vector;
    }

    public VectorKey(float time, float x, float y, float z)
      : this(time, new Vector3(x, y, z)) { }
  }
}
