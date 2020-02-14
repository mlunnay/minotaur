using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class TextureCubeWriter : ContentTypeWriter<TextureCubeContent>
  {
    public TextureCubeWriter()
      : base(new Guid("e6994102-482c-4bac-927d-19ef7169499e")) { }

    public override void Write(ContentWriter writer, TextureCubeContent value)
    {
      writer.Write((uint)value.ImageType);
      writer.Write(value.Faces[0][0].Width);
      writer.Write(value.Faces[0][0].Height);
      writer.Write(value.Faces[0].Count);
      foreach (MipmapChain current in value.Faces)
      {
        foreach (BitmapContent item in current)
        {
          byte[] data = item.Data();
          writer.Write((uint)data.Length);
          writer.Write(data);
        }
      }
    }
  }
}
