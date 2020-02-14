using System;
using System.Runtime.Serialization;

namespace Minotaur.Graphics
{
  public class LinkFailedException : Exception
  {
    public LinkFailedException()
    {
    }

    public LinkFailedException(string msg)
      : base(msg)
    {
    }

    public LinkFailedException(string msg, Exception inner)
      : base(msg, inner)
    {
    }

    public LinkFailedException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
