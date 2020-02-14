using System;

namespace Minotaur.Content
{
  public class UInt16Reader : ContentTypeReader<UInt16>
  {
    public UInt16Reader()
      : base(new Guid("03d355ea-8e06-428d-b3f7-c1641a32371f"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadUInt16();
    }
  }
}
