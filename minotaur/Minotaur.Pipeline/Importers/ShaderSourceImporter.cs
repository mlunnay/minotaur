using System.IO;
using Minotaur.Pipeline.Graphics;
using Minotaur.Pipeline.Utils;

namespace Minotaur.Pipeline.Importers
{
  [ContentImporter(".glsl",
    ".frag",
    ".vert",
    DefaultProcessor = "PassThroughProcessor")]
  public class ShaderSourceImporter : ContentImporter<ShaderSourceContent>
  {
    public OpenTK.Graphics.OpenGL.ShaderType ShaderType { get; set; }

    public ShaderSourceImporter() { }

    public override ShaderSourceContent Import(FileStream stream, ContentManager manager)
    {
      ShaderSourceContent content = new ShaderSourceContent() { FileName = stream.Name };

      string ext = Path.GetExtension(stream.Name);
      if (ext == ".glsl")
        content.ShaderType = ShaderType;
      else
        content.ShaderType = ext == ".vert" ? OpenTK.Graphics.OpenGL.ShaderType.VertexShader : OpenTK.Graphics.OpenGL.ShaderType.FragmentShader;

      using (StreamReader reader = new StreamReader(stream))
      {
        content.Source = reader.ReadToEnd();
      }

      if (!ShaderHelper.BuildShaderProgram(content, manager.Log))
        content = null;

      return content;
    }
  }
}
