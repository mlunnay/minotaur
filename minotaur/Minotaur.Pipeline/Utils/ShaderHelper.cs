using System.Collections.Generic;
using System.Text.RegularExpressions;
using log4net;
using Minotaur.Pipeline.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Pipeline.Utils
{
  /// <summary>
  /// This class provides helper functions for testing the compilation of shader source files.
  /// </summary>
  public static class ShaderHelper
  {
    private static Regex shaderErrorRegex = new Regex(@"ERROR: (\d+):(\d+): (.*)");

    public static bool BuildShaderProgram(ShaderSourceContent content, ILog log)
    {
      bool succesful = true;
      int status;
      int shaderID = GL.CreateShader(content.ShaderType);
      GL.ShaderSource(shaderID, content.Source);
      GL.CompileShader(shaderID);
      GL.GetShader(shaderID, ShaderParameter.CompileStatus, out status);
      if (status == 0)
      {
        succesful = false;
        string l = GL.GetShaderInfoLog(shaderID);
        foreach (string line in l.Trim().Split(new char[] { '\n' }))
        {
          string error = "";
          if (string.IsNullOrEmpty(line) || (error = ParseShaderError(line, content.FileName)) == "")
            error = string.Format("Unkown error.", content.FileName);
          throw new ContentException(string.Format("Error compiling shader: {0}", error));
        }
      }
      GL.DeleteShader(shaderID);
      return succesful;
    }

    public static bool BuildShaderProgram(List<ShaderSourceContent> content, ILog log)
    {
      bool succesful = true;
      foreach (ShaderSourceContent item in content)
      {
        succesful &= BuildShaderProgram(content, log);
      }

      return succesful;
    }

    public static string ParseShaderError(string error, string filename)
    {
      Match match = shaderErrorRegex.Match(error);
      if (match.Success)
      {
        int fileNumber = int.Parse(match.Groups[1].Value);
        int lineNumber = int.Parse(match.Groups[2].Value);
        string msg = match.Groups[3].Value;
        return string.Format("{0} line {1}: {2}", filename, lineNumber, msg);
      }
      return "";
    }
  }
}
