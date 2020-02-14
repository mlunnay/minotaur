using System;
using Minotaur.Core;
using Minotaur.Graphics;

namespace Minotaur.Content
{
  public class VertexBufferReader : ContentTypeReader<VertexBuffer>
  {
    public VertexBufferReader()
      : base(new Guid("1da5cdcd-2f1e-41d5-b7ab-fde163a5336e")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<VertexFormat>(new VertexDeclarationReader());
    }

    public override object Read(ContentReader reader)
    {
      VertexFormat format = reader.ReadObjectRaw<VertexFormat>();
      uint vertexCount = reader.ReadUInt32();
      byte[] data = reader.ReadBytes(format.Stride * (int)vertexCount);

      VertexBuffer buffer = VertexBuffer.Create(format, data);
      return buffer;
    }
  }
}
