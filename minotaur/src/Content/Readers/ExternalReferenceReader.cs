using System;

namespace Minotaur.Content
{
  public class ExternalReferenceReader : ContentTypeReader<object>
  {
    public ExternalReferenceReader()
      : base(new Guid("c9553b80-f03b-4e76-b75d-2e339aba0713")) { }

    public override object Read(ContentReader reader)
    {
      return reader.ReadExternalReference<object>();
    }
  }
}
