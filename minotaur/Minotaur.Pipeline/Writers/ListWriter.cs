using System;
using System.Collections.Generic;

namespace Minotaur.Pipeline.Writers
{
  public class ListWriter<T> : ContentTypeWriter<List<T>>
  {
    private ContentTypeWriter elementWriter;

    public ListWriter()
      : base(new Guid("3485d551-c1b0-4316-833a-154b43323697")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      elementWriter = manager.GetTypeWriter(typeof(T));
    }

    public override void Write(ContentWriter writer, List<T> value)
    {
      writer.Write(value.Count);
      foreach (T item in value)
      {
        writer.WriteObject(item, elementWriter);
      }
    }
  }
}
