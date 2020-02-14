using System;
using System.Collections.Generic;

namespace Minotaur.Pipeline.Writers
{
  public class DictionaryWriter<TKey, TValue> : ContentTypeWriter<Dictionary<TKey, TValue>>
  {
    private ContentTypeWriter keyWriter;
    private ContentTypeWriter valueWriter;
    private ContentTypeWriterManager _manager;

    public DictionaryWriter()
      : base(new Guid("f8af2eb6-48e5-40aa-882b-5b06af1f8b35")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      _manager = manager;
      keyWriter = manager.GetTypeWriter(typeof(TKey));
      // if TValue is an object (a non generic dictionary) then we want to look up each objects writer at runtime.
      valueWriter = typeof(TValue) == typeof(object) ? null : manager.GetTypeWriter(typeof(TValue));
    }

    public override void Write(ContentWriter writer, Dictionary<TKey, TValue> value)
    {
      writer.Write(value.Count);
      if (valueWriter != null)
      {
        foreach (KeyValuePair<TKey, TValue> pair in value)
        {
          writer.WriteObject(pair.Key, keyWriter);
          writer.WriteObject(pair.Value, valueWriter);
        }
      }
      else
      {
        foreach (KeyValuePair<TKey, TValue> pair in value)
        {
          writer.WriteObject(pair.Key, keyWriter);
          writer.WriteObject(pair.Value);
        }
      }
    }
  }
}
