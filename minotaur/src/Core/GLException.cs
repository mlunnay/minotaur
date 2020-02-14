using System;
using System.Runtime.Serialization;

namespace Minotaur.Core
{
  public class GLException : Exception
  {
    public GLException()
    {
    }

    public GLException(string msg)
      : base(msg)
    {
    }

    public GLException(string msg, Exception inner)
      : base(msg, inner)
    {
    }

    public GLException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
