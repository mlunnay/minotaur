using System;
using Minotaur.Core;
using Minotaur.Graphics;

namespace Minotaur.Content
{
  public class IndexBufferReader : ContentTypeReader<IndexBuffer>
  {
    public IndexBufferReader()
      : base(new Guid("f6eded0f-1342-4249-b231-c166a860b224")) { }

    public override object Read(ContentReader reader)
    {
      uint length = reader.ReadUInt32();
      byte[] data = reader.ReadBytes((int)length);
      UInt32[] indexes = new UInt32[length / 4];
      for (int i = 0; i < length / 4; i++)
      {
        indexes[i] = BitConverter.ToUInt32(data, i * 4);
      }

      return IndexBuffer.Create(indexes);
    }
  }
}
