using System;

namespace Minotaur.Pipeline
{
  public interface IContentProcessor
  {
    Type InputType { get; }
    Type OutputType { get; }
    object Process(object input, ContentProcessorContext context);
  }
}
