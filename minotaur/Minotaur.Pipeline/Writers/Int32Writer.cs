using System;

namespace Minotaur.Pipeline.Writers
{
  public class Int32Writer : ContentTypeWriter<Int32>
  {
    public Int32Writer()
      : base(new Guid("a294122f-f330-401d-bf2d-238446f8534c"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Int32 value)
    {
      writer.Write(value);
    }
  }
}
