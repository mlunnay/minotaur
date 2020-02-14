using System;

namespace Minotaur.Pipeline.Writers
{
  public class CharWriter : ContentTypeWriter<Char>
  {
    public CharWriter()
      : base(new Guid("0b5332fe-8dc5-4eb3-abda-43cf6fd78441"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Char value)
    {
      writer.Write(value);
    }
  }
}
