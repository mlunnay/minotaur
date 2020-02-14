using System;

namespace Minotaur.Content
{
  public class Int32Reader : ContentTypeReader<Int32>
  {
    public Int32Reader()
      : base(new Guid("a294122f-f330-401d-bf2d-238446f8534c"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadInt32();
    }
  }
}
