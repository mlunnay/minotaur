using System.IO;

namespace Minotaur.Content
{
  /// <summary>
  /// This interface is used by classes to load content for a ContentManager.
  /// It uses URI semantics for content addressing.
  /// </summary>
  public interface IContentLoader
  {
    string Scheme { get; }

    void HandleRequest(string uri, out Stream stream);
  }
}
