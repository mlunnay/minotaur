using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class ExternalReferenceWriter<T> : ContentTypeWriter<ExternalReferenceContent<T>>
  {
    public ExternalReferenceWriter()
      : base(new Guid("c9553b80-f03b-4e76-b75d-2e339aba0713")) { }

    public override void Write(ContentWriter writer, ExternalReferenceContent<T> value)
    {
      if (!value.HasBeenBuilt)
        value.Build();
      writer.Write(value.FileName);
    }
  }
}
