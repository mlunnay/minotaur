using System;

namespace Minotaur.Pipeline.Writers
{
  public class UInt32Writer : ContentTypeWriter<UInt32>
  {
    public UInt32Writer()
      : base(new Guid("48afe632-567b-4b68-847e-0dabfbd57f07"))
    {
      isPrimitiveType = true;
    }

    public override void Write(ContentWriter writer, UInt32 value)
    {
      writer.Write(value);
    }
  }
}
