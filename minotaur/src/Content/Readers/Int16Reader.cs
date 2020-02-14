using System;

namespace Minotaur.Content
{
  public class Int16Reader : ContentTypeReader<Int16>
  {
    public Int16Reader()
      : base(new Guid("c6e37941-8528-4e20-9e09-ffc56cdca50b"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadInt16();
    }
  }
}
