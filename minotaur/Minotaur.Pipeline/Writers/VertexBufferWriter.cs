using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class VertexBufferWriter : ContentTypeWriter<VertexBufferContent>
  {
    public VertexBufferWriter()
      : base(new Guid("1da5cdcd-2f1e-41d5-b7ab-fde163a5336e")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<VertexAttribute>(new VertexAttributeWriter());
    }

    public override void Write(ContentWriter writer, VertexBufferContent value)
    {
      // vertex declaration
      writer.Write((UInt32)value.Stride);
      writer.Write((UInt32)value.Count);
      foreach (VertexAttribute item in value.Attributes)
      {
        writer.WriteRawObject(item);
      }
      // vertex data
      byte[] data = value.GetBytes();
      writer.Write(data.Length / value.Stride);     
      writer.Write(data);
    }
  }
}
