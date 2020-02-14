using System;
using System.Collections.Generic;

namespace Minotaur.Content
{
  public class ListReader<T> : ContentTypeReader<List<T>>
  {
    private ContentTypeReader elementReader;

    public ListReader()
      : base(new Guid("3485d551-c1b0-4316-833a-154b43323697")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      elementReader = manager.GetTypeReader(typeof(T));
    }

    public override object Read(ContentReader reader)
    {
      int length = reader.ReadInt32();
      List<T> list = new List<T>(length);
      for (int i = 0; i < length; i++)
      {
        list.Add(reader.ReadObject<T>(elementReader));
      }
      return list;
    }
  }
}
