using System;
using Minotaur.Graphics;
using Minotaur.Pipeline.Graphics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Minotaur.Pipeline.Writers
{
  public class IndexBufferWriter : ContentTypeWriter<IndexCollection>
  {
    public IndexBufferWriter()
      : base(new Guid("f6eded0f-1342-4249-b231-c166a860b224")) { }

    public override void Write(ContentWriter writer, IndexCollection value)
    {
      List<byte> data = new List<byte>();
      foreach (int i in value)
      {
        data.AddRange(BitConverter.GetBytes(i));
      }
      writer.Write(data.Count);
      writer.Write(data.ToArray());
    }
  }
}
