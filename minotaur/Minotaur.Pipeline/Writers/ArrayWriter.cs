using System;

namespace Minotaur.Pipeline.Writers
{
  public class ArrayWriter<T> : ContentTypeWriter<T[]>
  {
    private ContentTypeWriter elementWriter;

    public ArrayWriter()
      : base(new Guid("b2a7311a-d317-4cd6-9e78-3ea3f33725f1")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      elementWriter = manager.GetTypeWriter(typeof(T));
    }

    public override void Write(ContentWriter writer, T[] value)
    {
      writer.Write(value.Length);
      foreach (T item in value)
      {
        writer.WriteObject(item, elementWriter);
      }
    }
  }
}
