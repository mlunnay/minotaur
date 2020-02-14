using System;
using System.Runtime.Serialization;

namespace Minotaur.Content
{
  public class ContentLoadException : Exception
  {
    public ContentLoadException() { }

    public ContentLoadException(string message)
      : base(message) { }

    public ContentLoadException(string message, Exception innerException)
      : base(message, innerException) { }

    public ContentLoadException(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
  }
}
