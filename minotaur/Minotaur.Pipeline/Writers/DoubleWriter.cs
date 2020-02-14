using System;

namespace Minotaur.Pipeline.Writers
{
  public class DoubleWriter : ContentTypeWriter<Double>
  {
    public DoubleWriter()
      : base(new Guid("f27d55f4-cefe-469b-b964-720fd3b33611"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Double value)
    {
      writer.Write(value);
    }
  }
}
