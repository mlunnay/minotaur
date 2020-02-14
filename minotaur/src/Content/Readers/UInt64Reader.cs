using System;

namespace Minotaur.Content
{
  public class UInt64Reader : ContentTypeReader<UInt64>
  {
    public UInt64Reader()
      : base(new Guid("608257b0-7beb-486a-8afc-2cb233ce43cf"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadUInt64();
    }
  }
}
