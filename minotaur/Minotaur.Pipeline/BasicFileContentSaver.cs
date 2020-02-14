using System;
using System.IO;

namespace Minotaur.Pipeline
{
  public class BasicFileContentSaver : IContentSaver
  {
    private bool _firstRun = true;
    public string BasePath { get; private set; }

    public BasicFileContentSaver(string basePath)
    {
      BasePath = basePath;
    }

    public void Initialise() { }

    public void Finish() { }

    public DateTime GetLastModified(string filename, Type contentType)
    {
      string path = Path.Combine(BasePath, filename);
      return File.Exists(path) ? File.GetLastWriteTime(path) : DateTime.MinValue;
    }

    public virtual string GetPath(string filename, Type contentType)
    {
      return Path.Combine(BasePath, filename);
    }

    public virtual string Save(System.IO.Stream stream, string filename, Type contentType)
    {
      if (_firstRun)
      {
        if (!string.IsNullOrEmpty(BasePath) && !Directory.Exists(BasePath))
          Directory.CreateDirectory(BasePath);
        _firstRun = false;
      }

      string path = Path.Combine(BasePath, filename);
      using (Stream file = File.Create(path))
      {
        stream.Position = 0;
        stream.CopyTo(file);
      }

      return path;
    }
  }
}
