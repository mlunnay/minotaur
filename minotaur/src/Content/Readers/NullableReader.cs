using System;
using Minotaur.Core;

namespace Minotaur.Content
{
  public class NullableReader<T> : ContentTypeReader<Nullable<T>> where T : struct
  {
    private ContentTypeReader typeReader;

    public NullableReader()
      : base(new Guid("544091d3-34bd-49ba-9c53-17611b60a407")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      typeReader = manager.GetTypeReader(typeof(T));
    }

    public override object Read(ContentReader reader)
    {
      if (!reader.ReadBoolean())
        return null;
      return (T?)reader.ReadObjectRaw<T>(typeReader);
    }
  }
}
