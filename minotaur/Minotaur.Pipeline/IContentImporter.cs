using System.IO;
using System;

namespace Minotaur.Pipeline
{
  public interface IContentImporter
  {
    Type OutputType { get; }
    object Import(FileStream stream, ContentManager manager);
  }
}
