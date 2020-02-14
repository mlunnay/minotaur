using System;

namespace Minotaur.Content
{
  public class CharReader : ContentTypeReader<Char>
  {
    public CharReader()
      : base(new Guid("0b5332fe-8dc5-4eb3-abda-43cf6fd78441"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadChar();
    }
  }
}
