using System;

namespace Minotaur.Content
{
  public class DoubleReader : ContentTypeReader<Double>
  {
    public DoubleReader()
      : base(new Guid("f27d55f4-cefe-469b-b964-720fd3b33611"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadDouble();
    }
  }
}
