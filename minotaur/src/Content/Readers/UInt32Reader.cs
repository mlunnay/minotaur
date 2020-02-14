using System;

namespace Minotaur.Content
{
  public class UInt32Reader : ContentTypeReader<UInt32>
  {
    public UInt32Reader()
      : base(new Guid("48afe632-567b-4b68-847e-0dabfbd57f07"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadUInt32();
    }
  }
}
