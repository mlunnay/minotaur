using System;
using System.IO;

namespace Minotaur.Pipeline
{
  public abstract class ContentImporter<T> : IContentImporter
  {
    protected ContentImporter() { }

    public Type OutputType { get { return typeof(T); } }

    public abstract T Import(FileStream stream, ContentManager manager);

    object IContentImporter.Import(FileStream stream, ContentManager manager)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (manager == null)
        throw new ArgumentNullException("manager");
      return Import(stream, manager);
    }
  }
}
