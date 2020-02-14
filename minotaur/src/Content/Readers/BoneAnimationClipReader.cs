using System;
using System.Linq;
using System.Collections.Generic;
using Minotaur.Core;
using Minotaur.Graphics.Animation;

namespace Minotaur.Content
{
  public class BoneAnimationClipReader : ContentTypeReader<BoneAnimationClip>
  {
    public BoneAnimationClipReader()
      : base(new Guid("f82fcbb0-8f5f-4d3f-afae-510c5e75cdb7")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<VectorKey>(new VectorKeyReader());
      manager.RegisterTypeReader<QuaternionKey>(new QuaternionKeyReader());
    }

    public override object Read(ContentReader reader)
    {
      string name = reader.ReadString();
      float fps = (float)reader.ReadUInt32();
      ushort totalFrames = (ushort)reader.ReadUInt32();
      KeyframeEnding preferedEnding = reader.ReadEnum<KeyframeEnding>();
      int boneCount = (int)reader.ReadUInt32();
      List<List<IVectorKey>> translations = new List<List<IVectorKey>>(boneCount);
      List<List<IQuaternionKey>> rotations = new List<List<IQuaternionKey>>(boneCount);
      List<List<IVectorKey>> scales = new List<List<IVectorKey>>(boneCount);
      for (int i = 0; i < boneCount; i++)
        translations.Add(reader.ReadObjectRaw<List<VectorKey>>().Cast<IVectorKey>().ToList());
      for (int i = 0; i < boneCount; i++)
        rotations.Add(reader.ReadObjectRaw<List<QuaternionKey>>().Cast<IQuaternionKey>().ToList());
      for (int i = 0; i < boneCount; i++)
        scales.Add(reader.ReadObjectRaw<List<VectorKey>>().Cast<IVectorKey>().ToList());

      return new BoneAnimationClip(fps, totalFrames, preferedEnding, translations, rotations, scales) { Name = name };
    }
  }
}
