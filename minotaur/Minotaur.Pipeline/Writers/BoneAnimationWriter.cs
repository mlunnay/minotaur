using System;
using System.Collections.Generic;
using System.Linq;
using Minotaur.Pipeline.Graphics;
using Minotaur.Graphics.Animation;

namespace Minotaur.Pipeline.Writers
{
  public class BoneAnimationWriter : ContentTypeWriter<BoneAnimationsContent>
  {
    public BoneAnimationWriter()
      : base(new Guid("f82fcbb0-8f5f-4d3f-afae-510c5e75cdb7")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<VectorKey>(new VectorKeyWriter());
      manager.RegisterTypeWriter<QuaternionKey>(new QuaternionKeyWriter());
    }

    public override void Write(ContentWriter writer, BoneAnimationsContent value)
    {
      writer.Write(value.Name);
      writer.Write((uint)value.FramesPerSecond);
      writer.Write((uint)value.TotalFrames);
      writer.Write(value.preferedEnding);
      int count = value.TranslationKeys.Count;
      writer.Write((uint)count);
      for (int i = 0; i < count; i++)
        writer.WriteRawObject(value.TranslationKeys[i]);
      for (int i = 0; i < count; i++)
        writer.WriteRawObject(value.QuaternionKeys[i]);
      for (int i = 0; i < count; i++)
        writer.WriteRawObject(value.ScaleKeys[i]);
    }
  }
}
