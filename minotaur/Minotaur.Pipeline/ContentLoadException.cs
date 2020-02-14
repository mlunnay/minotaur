using System;
using System.Runtime.Serialization;

namespace Minotaur.Pipeline
{
  public class ContentException : Exception
  {
    public ContentException() { }

    public ContentException(string message)
      : base(message) { }

    public ContentException(string message, Exception innerException)
      : base(message, innerException) { }

    public ContentException(SerializationInfo info, StreamingContext context)
      : base(info, context) { }
  }
}
