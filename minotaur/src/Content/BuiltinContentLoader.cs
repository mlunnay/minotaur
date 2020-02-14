using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Minotaur.Content
{
  /// <summary>
  /// This content loader loads builtin resources from this assembly.
  /// </summary>
  public class BuiltinContentLoader : IContentLoader
  {
    public string Scheme
    {
      get { return "builtin"; }
    }

    public void HandleRequest(string uri, out Stream stream)
    {
      Uri _uri = new Uri(uri);
      string path = _uri.GetLeftPart(UriPartial.Path).Replace(string.Format("{0}://", Scheme), "");
      if (path.EndsWith("/"))
        path = path.Substring(0, path.Length - 1);
      stream = typeof(BuiltinContentLoader).Assembly.GetManifestResourceStream(string.Format("Minotaur.Content.Resources.{0}", path.Replace('/', '.')));
    }
  }
}
