using System;

namespace Minotaur.Pipeline.Writers
{
  public class SingleWriter : ContentTypeWriter<Single>
  {
    public SingleWriter()
      : base(new Guid("1eec3829-0e15-4d58-814b-60796fbd9b4e"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, Single value)
    {
      writer.Write(value);
    }
  }
}
