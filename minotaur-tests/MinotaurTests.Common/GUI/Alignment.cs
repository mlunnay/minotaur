using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinotaurTests.Common.GUI
{
  [Flags]
  public enum Alignment
  {
    Left = 0x1,
    Top = 0x2,
    Center = 0x4,
    Right = 0x8,
    Bottom = 0x10,
  }
}
