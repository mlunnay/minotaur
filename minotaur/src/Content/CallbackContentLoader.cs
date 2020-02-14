using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Minotaur.Content
{
  /// <summary>
  /// A content loader that uses a callback to load the content.
  /// </summary>
  public class CallbackContentLoader : IContentLoader
  {
    private Func<string, Stream> OpenFile;
    public string Scheme { get; private set; }

    public CallbackContentLoader(string scheme, Func<string, Stream> openFile)
    {
      Scheme = scheme;
      OpenFile = openFile;
    }

    public void HandleRequest(string uri, out System.IO.Stream stream)
    {
      Uri _uri = new Uri(uri);
      string path = _uri.GetLeftPart(UriPartial.Path).Replace(string.Format("{0}:", Scheme), "");

      if (path.EndsWith("/"))
        path = path.Substring(0, path.Length - 1);

      stream = OpenFile(path);
    }
  }
}
