using System;
using System.Linq;
using System.Collections.Generic;
using Assimp;
using Minotaur.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Pipeline.Graphics
{
  public abstract class VertexAttribute : ContentItem
  {
    public int Dimension;
    public VertexAttribPointerType Type;
    public VertexUsage Usage;

    public int Stride
    {
      get
      {
        int size = 0;
        switch (Type)
        {
          case VertexAttribPointerType.Byte:
            size = BlittableValueType<sbyte>.Stride;
            break;
          case VertexAttribPointerType.Double:
            size = BlittableValueType<double>.Stride;
            break;
          case VertexAttribPointerType.Float:
            size = BlittableValueType<float>.Stride;
            break;
          case VertexAttribPointerType.HalfFloat:
            size = BlittableValueType<Half>.Stride;
            break;
          case VertexAttribPointerType.Int:
            size = BlittableValueType<int>.Stride;
            break;
          case VertexAttribPointerType.Short:
            size = BlittableValueType<short>.Stride;
            break;
          case VertexAttribPointerType.UnsignedByte:
            size = BlittableValueType<byte>.Stride;
            break;
          case VertexAttribPointerType.UnsignedInt:
            size = BlittableValueType<uint>.Stride;
            break;
          case VertexAttribPointerType.UnsignedShort:
            size = BlittableValueType<ushort>.Stride;
            break;
          default:
            break;
        }
        return size * Dimension;
      }
    }

    public VertexAttribute(int dimensions, VertexAttribPointerType type, VertexUsage usage)
    {
      Dimension = dimensions;
      Type = type;
      Usage = usage;
    }

    public abstract void AddValues(Mesh mesh, int index);

    public abstract IEnumerable<byte> GetBytes(int index);
  }

  public class PositionAttribute : VertexAttribute
  {
    public List<Vector3D> Values = new List<Vector3D>();

    public PositionAttribute()
      : base(3, VertexAttribPointerType.Float, VertexUsage.Position) { }

    public override void AddValues(Mesh mesh, int index)
    {
      Values.Add(mesh.Vertices[index]);
    }

    public override IEnumerable<byte> GetBytes(int index)
    {
      List<byte> bytes = new List<byte>();
      bytes.AddRange(BitConverter.GetBytes(Values[index].X));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Y));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Z));
      return bytes;
    }
  }

  public class TextureCoordinateAttribute : VertexAttribute
  {
    public List<Vector3D> Values = new List<Vector3D>();

    public TextureCoordinateAttribute()
      : base(2, VertexAttribPointerType.Float, VertexUsage.TextureCoordinate) { }

    public override void AddValues(Mesh mesh, int index)
    {
      Vector3D v = mesh.GetTextureCoords(0)[index];
      v.Y = 1 - v.Y;
      Values.Add(v);
    }

    public override IEnumerable<byte> GetBytes(int index)
    {
      List<byte> bytes = new List<byte>();
      bytes.AddRange(BitConverter.GetBytes(Values[index].X));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Y));
      return bytes;
    }
  }

  public class NormalAttribute : VertexAttribute
  {
    public  List<Vector3D> Values = new List<Vector3D>();

    public NormalAttribute()
      : base(3, VertexAttribPointerType.Float, VertexUsage.Normal) { }

    public override void AddValues(Mesh mesh, int index)
    {
      Values.Add(mesh.Normals[index]);
    }

    public override IEnumerable<byte> GetBytes(int index)
    {
      List<byte> bytes = new List<byte>();
      bytes.AddRange(BitConverter.GetBytes(Values[index].X));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Y));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Z));
      return bytes;
    }
  }

  public class BlendIndicesAttribute : VertexAttribute
  {
    public List<byte[]> Values = new List<byte[]>();

    public BlendIndicesAttribute()
      : base(4, VertexAttribPointerType.Byte, VertexUsage.BlendIndices) { }

    public override void AddValues(Mesh mesh, int index)
    {
      Values.Add(new byte[4]);
    }

    public void AddValues(byte[] values)
    {
      if (values.Length != 4)
        throw new ArgumentException("values must contain 4 bytes.");
      Values.Add(values);
    }

    public override IEnumerable<byte> GetBytes(int index)
    {
      List<byte> bytes = new List<byte>();
      bytes.Add(Values[index][0]);
      bytes.Add(Values[index][1]);
      bytes.Add(Values[index][2]);
      bytes.Add(Values[index][3]);
      return bytes;
    }
  }

  public class BlendWeightsAttribute : VertexAttribute
  {
    public List<OpenTK.Vector4> Values = new List<OpenTK.Vector4>();

    public BlendWeightsAttribute()
      : base(4, VertexAttribPointerType.Int, VertexUsage.BlendWeight) { }

    public override void AddValues(Mesh mesh, int index)
    {
      Values.Add(new OpenTK.Vector4(0,0,0,0));
    }

    public void AddValues(float[] values)
    {
      if(values.Length != 4)
        throw new ArgumentException("values must contain 4 floats.");
      Values.Add(new OpenTK.Vector4(values[0],values[1],values[2],values[3]));
    }

    public override IEnumerable<byte> GetBytes(int index)
    {
      List<byte> bytes = new List<byte>();
      bytes.AddRange(BitConverter.GetBytes(Values[index].X));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Y));
      bytes.AddRange(BitConverter.GetBytes(Values[index].Z));
      bytes.AddRange(BitConverter.GetBytes(Values[index].W));
      return bytes;
    }
  }
}
