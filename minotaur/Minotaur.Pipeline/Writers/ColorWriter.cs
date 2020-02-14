using System;
using OpenTK.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class ColorWriter : ContentTypeWriter<Color4>
  {
    public ColorWriter()
      : base(new Guid("00bfc716-ac97-4012-9a35-60d037ea30b6")) { }

    public override void Write(ContentWriter writer, Color4 value)
    {
      writer.Write(value.R);
      writer.Write(value.G);
      writer.Write(value.B);
      writer.Write(value.A);
    }
  }
}
