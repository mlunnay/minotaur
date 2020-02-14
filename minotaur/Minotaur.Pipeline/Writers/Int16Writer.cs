using System;

namespace Minotaur.Pipeline.Writers
{
  public class Int16Writer : ContentTypeWriter<Int16>
  {
    public Int16Writer()
      : base(new Guid("c6e37941-8528-4e20-9e09-ffc56cdca50b"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Int16 value)
    {
      writer.Write(value);
    }
  }
}
