using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Minotaur.Pipeline
{
  public class BasicBuildOptions : IBuildOptions
  {
    public bool CompressOutput { get; private set; }

    public bool ForceRebuild { get; private set; }

    public BasicBuildOptions(bool compressOutput = false, bool forceRebuild = false)
    {
      CompressOutput = compressOutput;
      ForceRebuild = forceRebuild;
    }

    public virtual string GetIdentifierString(Type type)
    {
      return "MEB";
    }

    public virtual string GetExtension(Type type)
    {
      return "meb";
    }

    public virtual string GetOutputFilename(Type type, string inputName)
    {
      string name = Path.GetFileNameWithoutExtension(inputName);
      string ext = Path.GetExtension(inputName);
      if (type == typeof(Minotaur.Pipeline.Graphics.ShaderSourceContent))
      {
        if (!name.EndsWith("_vert", StringComparison.InvariantCultureIgnoreCase) || !name.EndsWith("_frag", StringComparison.InvariantCultureIgnoreCase))
          return string.Format("{0}_{1}.{2}", name, ext == ".vert" ? "vert" : "frag", GetExtension(type));
      }

      return string.Format("{0}.{1}", name, GetExtension(type)); ;
    }
  }
}
