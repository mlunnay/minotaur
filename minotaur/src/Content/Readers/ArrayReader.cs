using System;

namespace Minotaur.Content
{
  public class ArrayReader<T> : ContentTypeReader<T[]>
  {
    private ContentTypeReader elementReader;

    public ArrayReader()
      : base(new Guid("b2a7311a-d317-4cd6-9e78-3ea3f33725f1")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      elementReader = manager.GetTypeReader(typeof(T));
    }

    public override object Read(ContentReader reader)
    {
      int length = reader.ReadInt32();
      T[] array = new T[length];
      for (int i = 0; i < length; i++)
      {
        array[i] = reader.ReadObject<T>(elementReader);
      }

      return array;
    }
  }
}
