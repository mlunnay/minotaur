using System;
using System.IO;

namespace Minotaur.Pipeline
{
  public interface IContentSaver
  {
    void Initialise();
    void Finish();
    DateTime GetLastModified(string filename, Type contentType);
    string Save(Stream stream, string filename, Type contentType);
    string GetPath(string filename, Type contentType);
  }
}
