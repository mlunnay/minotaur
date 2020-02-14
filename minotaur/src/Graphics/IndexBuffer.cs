using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Minotaur.Core;

namespace Minotaur.Graphics
{
  public class IndexBuffer : GraphicsResource
  {
    private int _id;
    private int _length;
    private BufferUsageHint _usage;
    private DrawElementsType _elementType;

    public int ID { get { return _id; } }
    public int Length { get { return _length; } }
    public BufferUsageHint UsageHint { get { return _usage; } }
    public DrawElementsType ElementsType { get { return _elementType; } }
    public int ElementSize { get { return SizeOfElementType(_elementType); } }

    public IndexBuffer(BufferUsageHint usage, DrawElementsType elementType, int length)
    {
      _usage = usage;
      _elementType = elementType;
      _length = length;
      Threading.BlockOnUIThread(GenerateIfRequired);
    }

    public static int SizeOfElementType(DrawElementsType type)
    {
      switch (type)
      {
        case DrawElementsType.UnsignedByte: return 1;
        case DrawElementsType.UnsignedShort: return 2;
        case DrawElementsType.UnsignedInt: return 4;
        default:
          throw new ArgumentException("Unknown DrawElementsType value.");
      }
    }

    public void Bind()
    {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
    }

    public void Unbind()
    {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
    }

    private void GenerateIfRequired()
    {
      if (_id == 0)
      {
        int sizeInBytes = SizeOfElementType(_elementType);
        GL.GenBuffers(1, out _id);
        Utilities.CheckGLError();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
        Utilities.CheckGLError();
        GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(sizeInBytes * _length), IntPtr.Zero, _usage);
        Utilities.CheckGLError();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
      }
    }

    public void GetData<T>(int bytesOffset, T[] data, int startIndex, int elementCount, int stride) where T : struct
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (data.Length < (startIndex + elementCount))
        throw new ArgumentException("The array for the data parameter is too small to hold the data requested");
      if ((elementCount * stride) > (_length * SizeOfElementType(_elementType)))
        throw new ArgumentException("The vertex size is larger than the vertex buffer");

      Core.Threading.BlockOnUIThread(() => GetBufferData(bytesOffset, data, startIndex, elementCount, stride));
    }

    public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
    {
      int stride = Marshal.SizeOf(typeof(T));
      GetData(0, data, startIndex, elementCount, stride);
    }

    public void GetData<T>(T[] data) where T : struct
    {
      int stride = Marshal.SizeOf(typeof(T));
      GetData(0, data, 0, data.Length, stride);
    }

    public void SetData<T>(int bytesOffset, T[] data, int startIndex, int elementCount, int stride, bool replaceBuffer = false) where T : struct
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (data.Length < (startIndex + elementCount))
        throw new ArgumentException("The array for the data parameter is too small for the size requested to be set.");

      Core.Threading.BlockOnUIThread(() => SetBufferData(Marshal.SizeOf(typeof(T)), bytesOffset, data, startIndex, elementCount, stride, replaceBuffer));
    }

    public void SetData<T>(T[] data, int startIndex, int elementCount, bool replaceBuffer = false) where T : struct
    {
      int stride = Marshal.SizeOf(typeof(T));
      SetData(0, data, startIndex, elementCount, stride, replaceBuffer);
    }

    public void SetData<T>(T[] data, bool replaceBuffer = false) where T : struct
    {
      int stride = Marshal.SizeOf(typeof(T));
      SetData(0, data, 0, data.Length, stride, replaceBuffer);
    }

    private void GetBufferData<T>(int bytesOffset, T[] data, int startIndex, int elementCount, int vertexStride) where T : struct
    {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
      Utilities.CheckGLError();
      int bytesElementSize = Marshal.SizeOf(typeof(T));
      IntPtr ptr = GL.MapBuffer(BufferTarget.ElementArrayBuffer, BufferAccess.ReadOnly);
      Utilities.CheckGLError();
      // Pointer to the start of data to read in the index buffer
      ptr = new IntPtr(ptr.ToInt64() + bytesOffset);
      if (data is byte[])
      {
        byte[] buffer = data as byte[];
        // If data is already a byte[] we can skip the temporary buffer
        // Copy from the vertex buffer to the destination array
        Marshal.Copy(ptr, buffer, 0, buffer.Length);
      }
      else
      {
        // Temporary buffer to store the copied section of data
        byte[] buffer = new byte[elementCount * vertexStride - bytesOffset];
        // Copy from the vertex buffer to the temporary buffer
        Marshal.Copy(ptr, buffer, 0, buffer.Length);

        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * bytesElementSize);

        // Copy from the temporary buffer to the destination array

        int dataSize = Marshal.SizeOf(typeof(T));
        if (dataSize == vertexStride)
          Marshal.Copy(buffer, 0, dataPtr, buffer.Length);
        else
        {
          // If the user is asking for a specific element within the vertex buffer, copy them one by one...
          for (int i = 0; i < elementCount; i++)
          {
            Marshal.Copy(buffer, i * vertexStride, dataPtr, dataSize);
            dataPtr = (IntPtr)(dataPtr.ToInt64() + dataSize);
          }
        }

        dataHandle.Free();

        //Buffer.BlockCopy(buffer, 0, data, startIndex * elementSizeInByte, elementCount * elementSizeInByte);
      }
      GL.UnmapBuffer(BufferTarget.ElementArrayBuffer);
    }

    private void SetBufferData<T>(int bytesElementSize, int bytesOffset, T[] data, int startIndex, int elementCount, int stride, bool replaceBuffer = false) where T : struct
    {
      GenerateIfRequired();

      int size = bytesElementSize * elementCount;
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, _id);
      Utilities.CheckGLError();

      GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
      IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * bytesElementSize);

      if (replaceBuffer)
        GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)size, dataPtr, _usage);
      else
        GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)bytesOffset, (IntPtr)size, dataPtr);
      Utilities.CheckGLError();

      dataHandle.Free();
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
      _length = elementCount;
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        // no managed object to dispose so skipping if disposing
        DisposalManager.Add(() => GL.DeleteBuffers(1, ref _id));
        _id = 0;
      }
      IsDisposed = true;
    }

    public static IndexBuffer Create<T>(T[] data, BufferUsageHint usage = BufferUsageHint.StaticDraw) where T : struct
    {
      DrawElementsType elementType = DrawElementsType.UnsignedInt;
      
      switch(OpenTK.BlittableValueType<T>.Stride)
      {
        case 1: elementType = DrawElementsType.UnsignedByte; break;
        case 2: elementType = DrawElementsType.UnsignedShort; break;
        case 4: elementType = DrawElementsType.UnsignedInt; break;
        default:
          throw new ArgumentException("data Type does not map to a DrawElementsType.");
      }
      IndexBuffer ib = new IndexBuffer(usage, elementType, data.Length);
      ib.SetData(data);
      return ib;
    }
  }
}
