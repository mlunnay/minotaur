using System;

namespace Minotaur.Content
{
  public class SingleReader : ContentTypeReader<Single>
  {
    public SingleReader()
      : base(new Guid("1eec3829-0e15-4d58-814b-60796fbd9b4e"))
    {
      isPrimitiveType = true;
    }

    public override object Read(ContentReader reader)
    {
      return reader.ReadSingle();
    }
  }
}
