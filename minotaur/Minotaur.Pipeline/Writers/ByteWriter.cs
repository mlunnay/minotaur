using System;

namespace Minotaur.Pipeline.Writers
{
  public class ByteWriter : ContentTypeWriter<Byte>
  {
    public ByteWriter()
      : base(new Guid("c8b3b167-d6c3-4ba1-9a58-e3bb1c0c51d4"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Byte value)
    {
      writer.Write(value);
    }
  }
}
