using System;

namespace Minotaur.Content
{
  public class ByteReader : ContentTypeReader<Byte>
  {
    public ByteReader()
      : base(new Guid("c8b3b167-d6c3-4ba1-9a58-e3bb1c0c51d4"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadByte();
    }
  }
}
