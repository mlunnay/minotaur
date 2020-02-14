using System;
using System.Collections.Generic;
using Minotaur.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class VertexArray : GraphicsResource
  {
    #region Declarations

    private int _id;
    private VertexBuffer _vertexBuffer;
    private IndexBuffer _indexBuffer;
    private bool _dirty;
    private List<AttributeBinding> _bindings = new List<AttributeBinding>();

    public class AttributeBinding
    {
      public int Index { get; private set; }
      public int Size { get; private set; }
      public VertexAttribPointerType Type { get; private set; }
      public bool Normalized { get; private set; }
      public int Stride { get; private set; }
      public int Offset { get; private set; }

      public AttributeBinding(
        int index,
        int size,
        VertexAttribPointerType type,
        bool normalized,
        int stride,
        int offset)
      {
        Index = index;
        Size = size;
        Type = type;
        Normalized = normalized;
        Stride = stride;
        Offset = offset;
      }
    }

    #endregion

    #region Properties

    public int ID { get { return _id; } }

    public VertexBuffer VertexBuffer { get { return _vertexBuffer; } }

    public IndexBuffer IndexBuffer { get { return _indexBuffer; } }

    #endregion

    #region Constructor

    public VertexArray(VertexBuffer vb, IndexBuffer ib)
    {
      _vertexBuffer = vb;
      _indexBuffer = ib;
      GL.GenVertexArrays(1, out _id);
      Utilities.CheckGLError();
    }

    #endregion

    #region Public Methods

    public void Bind()
    {
      if (_dirty)
        ApplyBindings();
      GL.BindVertexArray(_id);      
    }

    public void Unbind()
    {
      GL.BindVertexArray(0);
    }

    public void Clear()
    {
      _bindings.Clear();
      _dirty = true;
    }

    public AttributeBinding AddBinding(
      int index,
      int size,
      VertexAttribPointerType type,
      bool normalized,
      int stride,
      int offset)
    {
      AttributeBinding binding = new AttributeBinding(index, size, type, normalized, stride, offset);
      _bindings.Add(binding);
      _dirty = true;
      return binding;
    }

    public AttributeBinding AddBinding(
      int index,
      VertexUsage usage,
      int usageIndex = 0)
    {
      VertexAttribute v = _vertexBuffer.VertexFormat.GetAttribute(usage, usageIndex);
      AttributeBinding binding = new AttributeBinding(index, v.Dimension, v.Type, v.Normalized, _vertexBuffer.VertexFormat.Stride, v.Offset);
      _bindings.Add(binding);
      _dirty = true;
      return binding;
    }

    public int PrimitiveCount(int offset)
    {
      return GetPrimitiveCount(_vertexBuffer.BeginMode, _indexBuffer != null ? _indexBuffer.Length - offset : _vertexBuffer.Count);
    }

    #endregion

    #region Private Methods

    private void ApplyBindings()
    {
      GL.BindVertexArray(_id);
      _vertexBuffer.Bind();
      if(_indexBuffer != null)
        _indexBuffer.Bind();
      foreach (AttributeBinding binding in _bindings)
      {
        GL.EnableVertexAttribArray(binding.Index);
        if (binding.Type == VertexAttribPointerType.Double ||
            binding.Type == VertexAttribPointerType.Float ||
            binding.Type == VertexAttribPointerType.HalfFloat)
        {
          GL.VertexAttribPointer(
              binding.Index,
              binding.Size,
              binding.Type,
              binding.Normalized,
              binding.Stride,
              binding.Offset
          );
        }
        else
        {
          GL.VertexAttribIPointer(
              binding.Index,
              binding.Size,
              (VertexAttribIPointerType)binding.Type,
              binding.Stride,
              new IntPtr(binding.Offset)
          );
        }
      }
      _dirty = false;
      GL.BindVertexArray(0);
      _vertexBuffer.Unbind();
      if (_indexBuffer != null)
        _indexBuffer.Unbind();
    }

    private static VertexAttribPointerType VertexAttribPointerTypeFromType(Type type)
    {
      VertexAttribPointerType o;
      if (type == typeof(sbyte))
        o = VertexAttribPointerType.Byte;
      else if (type == typeof(double))
        o = VertexAttribPointerType.Double;
      else if (type == typeof(float))
        o = VertexAttribPointerType.Float;
      else if (type == typeof(int))
        o = VertexAttribPointerType.Int;
      else if (type == typeof(short))
        o = VertexAttribPointerType.Short;
      else if (type == typeof(byte))
        o = VertexAttribPointerType.UnsignedByte;
      else if (type == typeof(uint))
        o = VertexAttribPointerType.UnsignedInt;
      else if (type == typeof(ushort))
        o = VertexAttribPointerType.UnsignedShort;
      else if (type == typeof(Vector3))
        o = VertexAttribPointerType.Float;
      else
        throw new ArgumentException("Type does not map to a VertexAttribPointerType");

      return o;
    }

    private int GetPrimitiveCount(BeginMode beginMode, int indexSize)
    {
      switch (beginMode)
      {
        case BeginMode.LineLoop:
          return indexSize;
        case BeginMode.LineStrip:
          return indexSize - 1;
        case BeginMode.LineStripAdjacency:
          return indexSize - 1;
        case BeginMode.Lines:
          return indexSize / 2;
        case BeginMode.LinesAdjacency:
          return indexSize / 2;
        case BeginMode.Patches:
          return indexSize / 4;
        case BeginMode.Points:
          return indexSize;
        case BeginMode.Polygon:
          return indexSize / 4;
        case BeginMode.QuadStrip:
          return indexSize / 2 - 1;
        case BeginMode.Quads:
          return indexSize / 4;
        case BeginMode.TriangleFan:
          return indexSize - 2;
        case BeginMode.TriangleStrip:
          return indexSize - 2;
        case BeginMode.TriangleStripAdjacency:
          return indexSize - 2;
        case BeginMode.Triangles:
          return indexSize / 3;
        case BeginMode.TrianglesAdjacency:
          return indexSize / 3;
        default:
          return indexSize / 4;
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _vertexBuffer.Dispose();
          _vertexBuffer = null;
          _indexBuffer.Dispose();
          _indexBuffer = null;
          _bindings = null;
        }
        DisposalManager.Add(() => GL.DeleteVertexArrays(1, ref _id));
        _id = 0;
      }
      IsDisposed = true;
    }

    #endregion
  }
}
