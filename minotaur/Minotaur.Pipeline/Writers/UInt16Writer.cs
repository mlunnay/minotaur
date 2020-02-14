using System;

namespace Minotaur.Pipeline.Writers
{
  public class UInt16Writer : ContentTypeWriter<UInt16>
  {
    public UInt16Writer()
      : base(new Guid("03d355ea-8e06-428d-b3f7-c1641a32371f"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, UInt16 value)
    {
      writer.Write(value);
    }
  }
}
