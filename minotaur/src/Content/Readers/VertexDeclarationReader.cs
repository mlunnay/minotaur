using System;
using Minotaur.Core;
using Minotaur.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Content
{
  public class VertexDeclarationReader : ContentTypeReader<VertexFormat>
  {
    public VertexDeclarationReader()
      : base(new Guid("6e2e62d4-fbd1-4c3d-abeb-942c5b2739ce")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {

    }

    public override object Read(ContentReader reader)
    {
      VertexFormat format = new VertexFormat();
      uint stride = reader.ReadUInt32();
      uint elementCount = reader.ReadUInt32();
      for (int i = 0; i < elementCount; i++)
      {
        int dimension = reader.ReadSByte();
        VertexAttribPointerType type = EnumToPointerType(reader.ReadSByte());
        VertexUsage usage = EnumToVertexUsage(reader.ReadUInt32());
        bool normalized = usage == VertexUsage.TextureCoordinate;
        format.Add(new VertexAttribute(usage, type, dimension, normalized));
      }
      
      return format;
    }

    private VertexAttribPointerType EnumToPointerType(sbyte e)
    {
      switch (e)
      {
        case 0:
          return VertexAttribPointerType.Byte;
        case 1:
          return VertexAttribPointerType.UnsignedByte;
        case 2:
          return VertexAttribPointerType.Short;
        case 3:
          return VertexAttribPointerType.UnsignedShort;
        case 4:
          return VertexAttribPointerType.Int;
        case 5:
          return VertexAttribPointerType.UnsignedInt;
        case 6:
          return VertexAttribPointerType.Float;
        case 7:
          return VertexAttribPointerType.Double;
        case 8:
          return VertexAttribPointerType.HalfFloat;
        default:
          throw new ArgumentException(string.Format("Unknown VertexAttribPointerType for {0}", e));
      }
    }

    private VertexUsage EnumToVertexUsage(uint e)
    {
      switch (e)
      {
        case 0:
          return VertexUsage.Position;
        case 1:
          return VertexUsage.Color;
        case 2:
          return VertexUsage.TextureCoordinate;
        case 3:
          return VertexUsage.Normal;
        case 4:
          return VertexUsage.Binormal;
        case 5:
          return VertexUsage.Tangent;
        case 6:
          return VertexUsage.BlendIndices;
        case 7:
          return VertexUsage.BlendWeight;
        case 8:
          return VertexUsage.Depth;
        case 9:
          return VertexUsage.Fog;
        case 10:
          return VertexUsage.PointSize;
        case 11:
          return VertexUsage.Sample;
        case 12:
          return VertexUsage.TesselateFactor;
        default:
          throw new ArgumentException(string.Format("Unknown VertexUsage for {0}", e));
      }
    }
  }
}
