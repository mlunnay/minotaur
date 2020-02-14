using System;
using OpenTK;

namespace Minotaur.Pipeline.Writers
{
  public class Vector3Writer : ContentTypeWriter<Vector3>
  {
    public Vector3Writer()
      : base(new Guid("adc3a259-a7a5-4c71-9d7e-1a25899f4dec")) { }

    public override void Write(ContentWriter writer, Vector3 value)
    {
      writer.Write(value.X);
      writer.Write(value.Y);
      writer.Write(value.Z);
    }
  }
}
