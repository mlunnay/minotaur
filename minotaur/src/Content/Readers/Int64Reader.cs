using System;

namespace Minotaur.Content
{
  public class Int64Reader : ContentTypeReader<Int64>
  {
    public Int64Reader()
      : base(new Guid("55a66a07-0916-4b4b-a508-c15c2622a854"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadInt64();
    }
  }
}
