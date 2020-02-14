using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Graphics;
using OpenTK;
using System.Reflection;
using Minotaur.Core;

namespace Minotaur.Content
{
  public class UniformValueReader : ContentTypeReader<IUniformValue>
  {
    public UniformValueReader()
      : base(new Guid("7EAF0135-608C-430F-A7D8-B6A56B284929")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<Matrix4>(new MatrixReader());
    }

    public override object Read(ContentReader reader)
    {
      UniformValueType type = (UniformValueType)reader.ReadInt32();
      if (type == UniformValueType.Int)
      {
        int[] vals = reader.ReadObjectRaw<int[]>();
        if (vals.Length == 1)
          return UniformValue.Create(vals[0]);
        else if (vals.Length == 2)
          return UniformValue.Create(vals[0], vals[1]);
        else if (vals.Length == 3)
          return UniformValue.Create(vals[0], vals[1], vals[2]);
        else if (vals.Length == 4)
          return UniformValue.Create(vals[0], vals[1], vals[2], vals[3]);
      }
      else if (type == UniformValueType.Float)
      {
        float[] vals = reader.ReadObjectRaw<float[]>();
        if (vals.Length == 1)
          return UniformValue.Create(vals[0]);
        else if (vals.Length == 2)
          return UniformValue.Create(vals[0], vals[1]);
        else if (vals.Length == 3)
          return UniformValue.Create(vals[0], vals[1], vals[2]);
        else if (vals.Length == 4)
          return UniformValue.Create(vals[0], vals[1], vals[2], vals[3]);
      }
      else if (type == UniformValueType.UInt)
      {
        uint[] vals = reader.ReadObjectRaw<uint[]>();
        if (vals.Length == 1)
          return UniformValue.Create(vals[0]);
        else if (vals.Length == 2)
          return UniformValue.Create(vals[0], vals[1]);
        else if (vals.Length == 3)
          return UniformValue.Create(vals[0], vals[1], vals[2]);
        else if (vals.Length == 4)
          return UniformValue.Create(vals[0], vals[1], vals[2], vals[3]);
      }
      else if (type == UniformValueType.Matrix)
      {
        return UniformValue.Create(reader.ReadObjectRaw<Matrix4>());
      }
      else if (type == UniformValueType.Texture)
      {
        Texture texture = reader.ReadExternalReference<Texture>();
        int slot = reader.ReadByte();
        return new UniformValueTexture(texture) { TextureSlot = slot };
      }
      else if (type == UniformValueType.Sampler)
      {
        Texture texture = reader.ReadExternalReference<Texture>();
        int slot = reader.ReadByte();
        Dictionary<string, object> parameters = reader.ReadObjectRaw<Dictionary<string, object>>();
        MethodInfo methodInfo = typeof(Sampler).GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
        Sampler sampler = (Sampler)methodInfo.InvokeWithNamedParameters(null, parameters);
        sampler.Texture = texture;
        return new UniformValueSampler(sampler, slot);
      }
      throw new ContentLoadException("Error reading UniformValue.");
    }
  }
}
