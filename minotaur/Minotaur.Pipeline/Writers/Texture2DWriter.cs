using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class Texture2DWriter : ContentTypeWriter<Texture2DContent>
  {
    public Texture2DWriter()
      : base(new Guid("e5e29004-6f77-4be4-a9c6-c3eb363ca021")) { }

    public override void Write(ContentWriter writer, Texture2DContent value)
    {
      writer.Write((uint)value.ImageType);
      writer.Write(value.Faces[0][0].Width);
      writer.Write(value.Faces[0][0].Height);
      writer.Write(value.Faces[0].Count);
      for (int i = 0; i < value.Faces[0].Count; i++)
      {
        byte[] data = value.Faces[0][i].Data();
        writer.Write((uint)data.Length);
        writer.Write(data);
      }
    }
  }
}
