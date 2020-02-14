using System;

namespace Minotaur.Pipeline.Writers
{
  public class UuidWriter : ContentTypeWriter<Guid>
  {
    public UuidWriter()
      : base(new Guid("2d0cfe01-33af-47d9-9811-b9c30ae5bff0"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Guid value)
    {
      writer.Write(value);
    }
  }
}
