using System;
using OpenTK.Graphics.OpenGL;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class VertexAttributeWriter : ContentTypeWriter<VertexAttribute>
  {
    public VertexAttributeWriter()
      : base(new Guid("6e2e62d4-fbd1-4c3d-abeb-942c5b2739ce")) { }

    public override void Write(ContentWriter writer, VertexAttribute value)
    {
      writer.Write((byte)value.Dimension);
      writer.Write(AttributeTypeToByte(value.Type));
      writer.Write((UInt32)value.Usage);
    }

    private byte AttributeTypeToByte(VertexAttribPointerType type)
    {
      switch (type)
      {
        case VertexAttribPointerType.Byte:
          return 0;
        case VertexAttribPointerType.UnsignedByte:
          return 1;
        case VertexAttribPointerType.Short:
          return 2;
        case VertexAttribPointerType.UnsignedShort:
          return 3;
        case VertexAttribPointerType.Int:
          return 4;
        case VertexAttribPointerType.UnsignedInt:
          return 5;
        case VertexAttribPointerType.Float:
          return 6;
        case VertexAttribPointerType.Double:
          return 7;
        case VertexAttribPointerType.HalfFloat:
          return 8;
        default:
          throw new ArgumentException(string.Format("Unknown VertexAttribPointerType: {0}", type));
      }
    }
  }
}
