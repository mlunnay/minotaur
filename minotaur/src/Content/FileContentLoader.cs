using System;
using System.IO;

namespace Minotaur.Content
{
  public class FileContentLoader : IContentLoader
  {
    public string Scheme { get; private set; }
    public string RootDirectory { get; private set; }

    public FileContentLoader(string scheme, string rootDirectory)
    {
      Scheme = scheme;
      RootDirectory = rootDirectory;
    }

    public void HandleRequest(string uri, out Stream stream)
    {
      Uri _uri = new Uri(uri);
      string path = _uri.GetLeftPart(UriPartial.Path).Replace(string.Format("{0}://", Scheme), "");

      if (path.EndsWith("/"))
        path = path.Substring(0, path.Length - 1);

      stream = new FileStream(Path.Combine(RootDirectory, path), FileMode.Open, FileAccess.Read);
    }
  }
}
