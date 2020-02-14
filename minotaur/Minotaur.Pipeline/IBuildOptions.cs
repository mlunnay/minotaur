using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Pipeline
{
  public interface IBuildOptions
  {
    bool ForceRebuild { get; }
    bool CompressOutput { get; }
    string GetIdentifierString(Type type);
    string GetExtension(Type type);
    string GetOutputFilename(Type type, string inputName);
  }
}
