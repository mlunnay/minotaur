using System;
using OpenTK;

namespace Minotaur.Core
{
  public static class MatrixExtensions
  {
    public static Vector3 Translation(this Matrix4 m)
    {
      //return new Vector3(m.Row3);
      return new Vector3(m.M14, m.M24, m.M34);
    }
  }
}
