using System;
using Minotaur.Core;

namespace Minotaur.Content
{
  public class EnumReader<T> : ContentTypeReader<T> where T : struct, IConvertible
  {
    public EnumReader()
      : base(new Guid("58c9092c-635f-40c0-afa1-29aed90b64ca")) { }

    public override object Read(ContentReader reader)
    {
      return Enum.ToObject(typeof(T), reader.ReadInt32());
    }
  }
}
