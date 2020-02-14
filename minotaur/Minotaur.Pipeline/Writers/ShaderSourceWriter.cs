using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class ShaderSourceWriter : ContentTypeWriter<ShaderSourceContent>
  {
    public ShaderSourceWriter()
      : base(new Guid("205801eb-a58e-4627-a2df-7c9dafdd33d6")) { }

    public override void Write(ContentWriter writer, ShaderSourceContent value)
    {
      writer.Write(value.FileName);
      writer.Write(ShaderTypeToInt(value.ShaderType));
      writer.Write(value.Source);
    }

    public static int ShaderTypeToInt(OpenTK.Graphics.OpenGL.ShaderType type)
    {
      switch (type)
      {
        case OpenTK.Graphics.OpenGL.ShaderType.FragmentShader:
          return 1;
        case OpenTK.Graphics.OpenGL.ShaderType.GeometryShader:
          return 2;
        case OpenTK.Graphics.OpenGL.ShaderType.VertexShader:
          return 0;
        default:
          throw new ContentException("Unknown ShaderType.");
      }
    }
  }
}
