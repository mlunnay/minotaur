using System;

namespace Minotaur.Pipeline.Writers
{
  public class BooleanWriter : ContentTypeWriter<Boolean>
  {
    public BooleanWriter()
      : base(new Guid("733e2e58-a144-4b0e-992c-28e558a3cac7"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Boolean value)
    {
      writer.Write(value);
    }
  }
}
