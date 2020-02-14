using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Graphics;
using Minotaur.Pipeline.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Pipeline.Utils
{
  /// <summary>
  /// This class hold methods to aid in the processing of Materials.
  /// </summary>
  public static class MaterialHelper
  {
    private static Dictionary<int, List<Minotaur.Graphics.ProgramUniform>> _shaderUniformCache = new Dictionary<int, List<Minotaur.Graphics.ProgramUniform>>();

    /// <summary>
    /// This method gets the uniform values that are used by the program compiled from the list of source strings supplied.
    /// </summary>
    /// <param name="sources">A list containing source strings to complie and link.</param>
    /// <returns></returns>
    public static List<Minotaur.Graphics.ProgramUniform> GetShaderUniforms(IEnumerable<ShaderSourceContent> sources, bool useCache = true)
    {
      // first calculate a hash for the sources for the caching key
      int hash = string.Join("", sources.Select(s => s.FileName).OrderBy(s => s)).GetHashCode();
      List<Minotaur.Graphics.ProgramUniform> uniforms;
      if (!useCache || !_shaderUniformCache.TryGetValue(hash, out uniforms))
      {
        uniforms = GetUniforms(sources);
        if(useCache)
          _shaderUniformCache[hash] = uniforms;
      }

      return uniforms;
    }

    private static List<ProgramUniform> GetUniforms(IEnumerable<ShaderSourceContent> sources)
    {
      List<ProgramUniform> uniforms = new List<ProgramUniform>();
      List<int> shaderIDs = new List<int>();
      int programID = GL.CreateProgram();
      int status;

      foreach (ShaderSourceContent content in sources)
      {
        int shaderID = GL.CreateShader(content.ShaderType);
        GL.ShaderSource(shaderID, content.Source);
        GL.CompileShader(shaderID);
        GL.GetShader(shaderID, ShaderParameter.CompileStatus, out status);
        if (status == 0)
        {
          string l = GL.GetShaderInfoLog(shaderID);
          GL.DeleteShader(shaderID);
          foreach (int id in shaderIDs)
            GL.DeleteShader(id);
          GL.DeleteProgram(programID);
          throw new ContentException(string.Format("Error compiling shader: {0}", content.FileName));
        }
        GL.AttachShader(programID, shaderID);
        shaderIDs.Add(shaderID);
      }

      GL.LinkProgram(programID);
      GL.GetProgram(programID, ProgramParameter.LinkStatus, out status);
      if (status == 0)
      {
        foreach (int id in shaderIDs)
          GL.DeleteShader(id);
        GL.DeleteProgram(programID);
        throw new LinkFailedException(string.Format("Error linking program:"));
      }

      // shaders no longer needed.
      foreach (int id in shaderIDs)
        GL.DeleteShader(id);

      // now map the uniform attributes
      int count = 0;

      GL.GetProgram(programID, ProgramParameter.ActiveUniforms, out count);

      for (int i = 0; i < count; i++)
      {
        int size;
        ActiveUniformType type;

        string name = GL.GetActiveUniform(programID, i, out size, out type);
        int slot = GL.GetUniformLocation(programID, name);

        ProgramUniform attribute = new ProgramUniform(name, slot, size, type);
        uniforms.Add(attribute);
      }

      GL.DeleteProgram(programID);
      return uniforms;
    }
  }
}
