using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class VertexAttribute
  {
    public VertexUsage Usage { get; internal set; }
    public VertexAttribPointerType Type { get; internal set; }
    public int Index { get; internal set; }
    public int Dimension { get; internal set; }
    public int Offset { get; internal set; }
    public bool Normalized { get; internal set; }

    internal VertexAttribute() { }

    public VertexAttribute(VertexUsage usage,
      VertexAttribPointerType type,
      int index,
      int dimension)
      : this(usage, type, index, dimension, false)
    { }

    public VertexAttribute(VertexUsage usage,
      VertexAttribPointerType type,
      int dimension,
      bool normalized)
      : this(usage, type, 0, dimension, normalized)
    { }

    public VertexAttribute(VertexUsage usage,
      VertexAttribPointerType type,
      int dimension)
      : this(usage, type, 0, dimension, false)
    { }

    public VertexAttribute(VertexUsage usage,
      VertexAttribPointerType type,
      int index,
      int dimension,
      bool normalized)
    {
      Usage = usage;
      Type = type;
      Index = index;
      Dimension = dimension;
      Normalized = normalized;
      Offset = -1;  // set by VertexFormat when attached to it
    }

    public int Stride()
    {
      return Dimension * SizeOfVertexType(Type);
    }

    public static int SizeOfVertexType(VertexAttribPointerType type)
    {
      switch (type)
      {
        case VertexAttribPointerType.Byte: return 1;
        case VertexAttribPointerType.Double: return 8;
        case VertexAttribPointerType.Float: return 4;
        case VertexAttribPointerType.HalfFloat: return 2;
        case VertexAttribPointerType.Int: return 4;
        case VertexAttribPointerType.Short: return 2;
        case VertexAttribPointerType.UnsignedByte: return 1;
        case VertexAttribPointerType.UnsignedInt: return 4;
        case VertexAttribPointerType.UnsignedShort: return 2;
        default: throw new ArgumentException("Invalid VertexAttribPointerType");
      }
    }
  }
}
