using System;
using System.Collections.Generic;

namespace Minotaur.Content
{
  public class DictionaryReader<TKey, TValue> : ContentTypeReader<Dictionary<TKey, TValue>>
  {
    private ContentTypeReader keyReader;
    private ContentTypeReader valueReader;

    public DictionaryReader()
      : base(new Guid("f8af2eb6-48e5-40aa-882b-5b06af1f8b35")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      keyReader = manager.GetTypeReader(typeof(TKey));
      valueReader = manager.GetTypeReader(typeof(TValue));
    }

    public override object Read(ContentReader reader)
    {
      int length = reader.ReadInt32();
      Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>(length);
      for (int i = 0; i < length; i++)
      {
        TKey key = reader.ReadObject<TKey>(keyReader);
        TValue value = reader.ReadObject<TValue>(valueReader);
        dict.Add(key, value);
      }
      return dict;
    }
  }
}
