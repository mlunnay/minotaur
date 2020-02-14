using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public interface IBoneAnimationFrame
  {
    Quaternion Rotation { get; }
    Vector3 Translation { get; }
    Vector3 Scale { get; }
  }
}
