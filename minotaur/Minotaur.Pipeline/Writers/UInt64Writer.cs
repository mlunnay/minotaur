using System;

namespace Minotaur.Pipeline.Writers
{
  public class UInt64Writer : ContentTypeWriter<UInt64>
  {
    public UInt64Writer()
      : base(new Guid("608257b0-7beb-486a-8afc-2cb233ce43cf"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, UInt64 value)
    {
      writer.Write(value);
    }
  }
}
