using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Pipeline
{
  public interface IBuildSource
  {
    bool HasBuildItem(string path);
  }
}
