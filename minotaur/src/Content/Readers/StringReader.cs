using System;

namespace Minotaur.Content
{
  public class StringReader : ContentTypeReader<String>
  {
    public StringReader()
      : base(new Guid("069ff0a2-333d-4f77-ab06-0d59a75b7605"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadString();
    }
  }
}
