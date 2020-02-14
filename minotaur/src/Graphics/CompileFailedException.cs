using System;
using System.Runtime.Serialization;

namespace Minotaur.Graphics
{
  public class CompileFailedException : Exception
  {
    public CompileFailedException()
    {
    }

    public CompileFailedException(string msg)
      : base(msg)
    {
    }

    public CompileFailedException(string msg, Exception inner)
      : base(msg, inner)
    {
    }

    public CompileFailedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
