using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Minotaur.Pipeline
{
  public class ContentProcessorContext
  {
    private Stack<string> _directoryStack = new Stack<string>();

    public ContentManager ContentManager { get; internal set; }

    public string BaseDirectory { get { return _directoryStack.Peek(); } }

    public void PushDirectory(string directory)
    {
      _directoryStack.Push(directory);
    }

    public void PopDirectory()
    {
      _directoryStack.Pop();
    }

    public string GetFilenamePath(string filename)
    {
      if (filename.StartsWith("\\"))
        return filename;

      string path = "";
      foreach (string s in _directoryStack)
      {
        path = Path.Combine(s, path);
        if (path.StartsWith("\\") || path.StartsWith("/"))
          break;
      }

      return Path.Combine(path, filename);
    }
  }
}
