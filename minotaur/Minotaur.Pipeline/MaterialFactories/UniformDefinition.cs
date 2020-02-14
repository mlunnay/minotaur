using System.Collections.Generic;
using System.Linq;
using Minotaur.Pipeline.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Pipeline.MaterialFactories
{
  public class UniformDefinition
  {
    public string Name { get; set; }
    public ActiveUniformType Type { get; set; }
    public List<string> Aliases { get; set; }
    public UniformValueContent Value { get; set; }

    public UniformDefinition(string name, ActiveUniformType type, List<string> aliases, UniformValueContent value)
    {
      Name = name;
      Type = type;
      Aliases = new List<string>(aliases);
      Value = value;
    }

    public UniformDefinition(string name, ActiveUniformType type, UniformValueContent value)
      : this(name, type, new List<string>(), value) { }

    public static UniformDefinition Create(string name, Color4 color, List<string> aliases = null)
    {
      if(aliases == null)
        aliases = new List<string>();
      return new UniformDefinition(name, ActiveUniformType.FloatVec4, aliases, 
        new UniformValueContent(Minotaur.Graphics.UniformValueType.Float, new object[] { color.R, color.G, color.B, color.A }));
    }

    public static UniformDefinition Create(string name, string path, List<string> aliases = null)
    {
      if (aliases == null)
        aliases = new List<string>();
      SamplerContent sampler = new SamplerContent() { Texture = new ExternalReferenceContent<TextureContent>(path) };
      return new UniformDefinition(name, ActiveUniformType.Sampler2D, aliases,
        new UniformValueContent(Minotaur.Graphics.UniformValueType.Sampler, new object[] { sampler }));
    }

    public static UniformDefinition Create(string name, SamplerContent sampler, List<string> aliases = null)
    {
      if (aliases == null)
        aliases = new List<string>();
      return new UniformDefinition(name, ActiveUniformType.Sampler2D, aliases,
        new UniformValueContent(Minotaur.Graphics.UniformValueType.Sampler, new object[] { sampler }));
    }

    public static UniformDefinition Create(string name, int[] values, List<string> aliases = null)
    {
      if (values.Length > 4)
        throw new ContentException("Values must contain 1 - 4 integers.");
      if (aliases == null)
        aliases = new List<string>();
      ActiveUniformType type = ActiveUniformType.Int;
      if (values.Length == 2)
        type = ActiveUniformType.IntVec2;
      if (values.Length == 3)
        type = ActiveUniformType.IntVec3;
      if (values.Length == 4)
        type = ActiveUniformType.IntVec4;
      return new UniformDefinition(name, type, aliases,
        new UniformValueContent(Minotaur.Graphics.UniformValueType.Int, values.Cast<object>().ToArray()));
    }

    public static UniformDefinition Create(string name, uint[] values, List<string> aliases = null)
    {
      if (values.Length > 4)
        throw new ContentException("Values must contain 1 - 4 unsigned integers.");
      if (aliases == null)
        aliases = new List<string>();
      ActiveUniformType type = ActiveUniformType.UnsignedInt;
      if (values.Length == 2)
        type = ActiveUniformType.UnsignedIntVec2;
      if (values.Length == 3)
        type = ActiveUniformType.UnsignedIntVec3;
      if (values.Length == 4)
        type = ActiveUniformType.UnsignedIntVec4;
      return new UniformDefinition(name, type, aliases,
        new UniformValueContent(Minotaur.Graphics.UniformValueType.UInt, values.Cast<object>().ToArray()));
    }

    public static UniformDefinition Create(string name, float[] values, List<string> aliases = null)
    {
      if (values.Length > 4)
        throw new ContentException("Values must contain 1 - 4 floats.");
      if (aliases == null)
        aliases = new List<string>();
      ActiveUniformType type = ActiveUniformType.Float;
      if (values.Length == 2)
        type = ActiveUniformType.FloatVec2;
      if (values.Length == 3)
        type = ActiveUniformType.FloatVec3;
      if (values.Length == 4)
        type = ActiveUniformType.FloatVec4;
      return new UniformDefinition(name, type, aliases,
        new UniformValueContent(Minotaur.Graphics.UniformValueType.Float, values.Cast<object>().ToArray()));
    }

    public static UniformDefinition Create(string name, Matrix4 value, List<string> aliases = null)
    {
      if (aliases == null)
        aliases = new List<string>();

      return new UniformDefinition(name, ActiveUniformType.FloatMat4, aliases,
        new UniformValueContent(Minotaur.Graphics.UniformValueType.Matrix, new object[] { value }));
    }
  }
}
