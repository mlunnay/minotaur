using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
  [Flags]
  public enum SpriteEffect
  {
    None = 0x0,
    FlipHorizontally = 0x1,
    FlipVertically = 0x2
  }
}
