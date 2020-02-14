using System;

namespace Minotaur.Content
{
  public class UuidReader : ContentTypeReader<Guid>
  {
    public UuidReader()
      : base(new Guid("2d0cfe01-33af-47d9-9811-b9c30ae5bff0"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadGuid();
    }
  }
}
