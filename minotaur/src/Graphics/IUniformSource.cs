using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
  public interface IUniformSource
  {
    bool GetUniformValue(UniformUsage usage, out IUniformValue value);
  }
}
