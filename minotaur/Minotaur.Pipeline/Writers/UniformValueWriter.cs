using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Pipeline.Graphics;
using Minotaur.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class UniformValueWriter : ContentTypeWriter<UniformValueContent>
  {
    public UniformValueWriter()
      : base(new Guid("7EAF0135-608C-430F-A7D8-B6A56B284929")) { }

    public override void Write(ContentWriter writer, UniformValueContent value)
    {
      writer.Write((int)value.ValueType);
      if (value.ValueType == UniformValueType.Int)
      {
        writer.WriteRawObject(value.Values.Cast<int>().ToArray());
      }
      else if (value.ValueType == UniformValueType.UInt)
      {
        writer.WriteRawObject(value.Values.Cast<uint>().ToArray());
      }
      else if (value.ValueType == UniformValueType.Float)
      {
        writer.WriteRawObject(value.Values.Cast<float>().ToArray());
      }
      else if (value.ValueType == UniformValueType.Matrix)
      {
        writer.WriteRawObject((OpenTK.Matrix4)value.Values[0]);
      }
      else if (value.ValueType == UniformValueType.Texture)
      {
        writer.WriteRawObject((TextureContent)value.Values[0]);
        writer.Write((byte)value.Values[1]);
      }
      else if (value.ValueType == UniformValueType.Sampler)
      {
        SamplerContent sampler = (SamplerContent)value.Values[0];
        writer.WriteRawObject(sampler.Texture);
        writer.Write(sampler.Slot);
        writer.WriteRawObject(sampler.GetParameters());
      }
    }
  }
}
