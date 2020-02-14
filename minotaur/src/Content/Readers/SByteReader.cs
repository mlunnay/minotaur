using System;

namespace Minotaur.Content
{
  public class SByteReader : ContentTypeReader<SByte>
  {
    public SByteReader()
      : base(new Guid("af811ab3-7642-4c6b-8944-2472eefd1975"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadSByte();
    }
  }
}
