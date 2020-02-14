using System;

namespace Minotaur.Pipeline.Writers
{
  public class StringWriter : ContentTypeWriter<String>
  {
    public StringWriter()
      : base(new Guid("069ff0a2-333d-4f77-ab06-0d59a75b7605"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, String value)
    {
      writer.Write(value);
    }
  }
}
