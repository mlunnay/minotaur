using System;

namespace Minotaur.Pipeline.Writers
{
  public class Int64Writer : ContentTypeWriter<Int64>
  {
    public Int64Writer()
      : base(new Guid("55a66a07-0916-4b4b-a508-c15c2622a854"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Int64 value)
    {
      writer.Write(value);
    }
  }
}
