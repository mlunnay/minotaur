using System;
using Minotaur.Core;
using Minotaur.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Minotaur.Content
{
  public class ShaderSourceReader : ContentTypeReader<Shader>
  {
    private static Regex shaderErrorRegex = new Regex(@"ERROR: (\d+):(\d+): (.*)");

    public ShaderSourceReader()
      : base(new Guid("205801eb-a58e-4627-a2df-7c9dafdd33d6")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {

    }

    public override object Read(ContentReader reader)
    {
      List<string> errors = new List<string>();
      int status;
      string filename = reader.ReadString();
      ShaderType type = ShaderTypeFromID(reader.ReadInt32());
      string source = reader.ReadString();
      Shader shader = new Shader(type);
      GL.ShaderSource(shader._id, source);
      GL.CompileShader(shader._id);
      GL.GetShader(shader._id, ShaderParameter.CompileStatus, out status);
      if (status == 0)
      {
        string log = GL.GetShaderInfoLog(shader._id);
        shader.Dispose();
        foreach (string line in log.Trim().Split(new char[] { '\n' }))
        {
          if(string.IsNullOrEmpty(line))
            continue;
          errors.Add(ParseShaderError(line, filename));
        }
      }

      if (errors.Count != 0)
      {
        shader.Dispose();
        throw new CompileFailedException(string.Format("Compile failure in shaders:\n{0}", string.Join("\\n", errors)));
      }

      return shader;
    }

    private ShaderType ShaderTypeFromID(int id)
    {
      if (id == 0)
        return ShaderType.VertexShader;
      else if (id == 1)
        return ShaderType.FragmentShader;
      else if (id == 2)
        return ShaderType.GeometryShader;
      else
        throw new ArgumentException(string.Format("Unknown shader type id: {0}", id));
    }

    private string ParseShaderError(string error, string filename)
    {
      Match match = shaderErrorRegex.Match(error);
      if (match.Success)
      {
        int fileNumber = int.Parse(match.Groups[1].Value);
        int lineNumber = int.Parse(match.Groups[2].Value);
        string msg = match.Groups[3].Value;
        return string.Format("%s line %d: %s", filename, lineNumber, msg);
      }
      return "";
    }
  }
}
