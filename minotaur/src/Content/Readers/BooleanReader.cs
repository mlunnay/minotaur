using System;

namespace Minotaur.Content
{
  public class BooleanReader : ContentTypeReader<Boolean>
  {
    public BooleanReader()
      : base(new Guid("733e2e58-a144-4b0e-992c-28e558a3cac7"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadBoolean();
    }
  }
}
