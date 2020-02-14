using System;
namespace Minotaur.Pipeline
{
  public interface IContentTypeWriter
  {
    Guid ID { get; }
    void Initialize(ContentTypeWriterManager manager);
    bool IsPrimitiveType { get; }
    Type TargetType { get; }
    void Write(ContentWriter writer, object value);
  }
}
