using System;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public class QuaternionKey : IQuaternionKey
  {
    private float _time;
    private Quaternion _quarternion;

    public float Time
    {
      get { return _time; }
    }

    public Quaternion Rotation
    {
      get { return _quarternion; }
    }

    public QuaternionKey(float time, Quaternion value)
    {
      _time = time;
      _quarternion = value;
    }

    public QuaternionKey(float time, float x, float y, float z, float w)
      : this(time, new Quaternion(x, y, z, w)) { }
  }
}
