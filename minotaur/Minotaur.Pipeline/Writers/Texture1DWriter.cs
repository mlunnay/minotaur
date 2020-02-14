using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class Texture1DWriter : ContentTypeWriter<Texture1DContent>
  {
    public Texture1DWriter()
      : base(new Guid("046c54b9-65ca-4b0c-bced-d8c05c323d42")) { }

    public override void Write(ContentWriter writer, Texture1DContent value)
    {
      writer.Write((uint)value.ImageType);
      writer.Write(value.Faces[0][0].Width);
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
