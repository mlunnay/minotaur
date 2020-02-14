using System;
using OpenTK;

namespace Minotaur.Pipeline.Writers
{
  public class QuaternionWriter : ContentTypeWriter<Quaternion>
  {
    public QuaternionWriter()
      : base(new Guid("c48876b4-4c12-4b2d-bf52-18f3b022e4c3")) { }

    public override void Write(ContentWriter writer, Quaternion value)
    {
      writer.Write(value.X);
      writer.Write(value.Y);
      writer.Write(value.Z);
      writer.Write(value.W);
    }
  }
}
