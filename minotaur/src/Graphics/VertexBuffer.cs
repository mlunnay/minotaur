using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using Minotaur.Core;

namespace Minotaur.Graphics
{
  public class VertexBuffer : GraphicsResource
  {
    private int _id;
    private VertexFormat _vertexFormat;
    private int _count;
    private BufferUsageHint _usageHint;
    private BeginMode _beginMode;

    public int ID { get { return _id; } }
    public VertexFormat VertexFormat { get { return _vertexFormat; } }
    public int Count { get { return _count; } }
    public BufferUsageHint UsageHint { get { return _usageHint; } }
    public BeginMode BeginMode { get { return _beginMode; } }
    public int Size { get { return _count * _vertexFormat.Stride; } }

    public VertexBuffer(VertexFormat vertexFormat, int size, BufferUsageHint usage = BufferUsageHint.StaticDraw, BeginMode beginMode = OpenTK.Graphics.OpenGL.BeginMode.Triangles)
    {
      if (vertexFormat == null)
        _vertexFormat = new VertexFormat();
      else
        _vertexFormat = vertexFormat;
      _usageHint = usage;
      _count = size;
      _beginMode = beginMode;
      Threading.BlockOnUIThread(GenerateIfRequired);
    }

    public void Bind()
    {
      GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
    }

    public void Unbind()
    {
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void GetData<T>(int bytesOffset, T[] data, int startIndex, int elementCount, int stride) where T : struct
    {
      if (data == null)
        throw new ArgumentNullException("data");
      if (data.Length < (startIndex + elementCount))
        throw new ArgumentException("The array for the data parameter is too small to hold the data requested");
      if ((elementCount * stride) > Size)
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

    // setting replaceBuffer replaces the OpenGL buffer which can avoid pipeline stalls.
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
      GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
      Utilities.CheckGLError();
      int bytesElementSize = Marshal.SizeOf(typeof(T));
      IntPtr ptr = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadOnly);
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
      GL.UnmapBuffer(BufferTarget.ArrayBuffer);
    }

    private void SetBufferData<T>(int bytesElementSize, int bytesOffset, T[] data, int startIndex, int elementCount, int stride, bool replaceBuffer = false) where T : struct
    {
      GenerateIfRequired();

      int size = bytesElementSize * elementCount;
      GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
      Utilities.CheckGLError();

      GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
      IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * bytesElementSize);
      if (replaceBuffer)
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)size, dataPtr, _usageHint);
      else
        GL.BufferSubData(BufferTarget.ArrayBuffer, (IntPtr)bytesOffset, (IntPtr)size, dataPtr);
      Utilities.CheckGLError();

      dataHandle.Free();
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      _count = size;
    }

    private void GenerateIfRequired()
    {
      if (_id == 0)
      {
        GL.GenBuffers(1, out _id);
        Utilities.CheckGLError();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _id);
        Utilities.CheckGLError();
        GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(_vertexFormat.Stride * _count), IntPtr.Zero, _usageHint);
        Utilities.CheckGLError();
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      }
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

    public static VertexBuffer Create<T>(VertexFormat format, T[] data, BufferUsageHint usageHint = BufferUsageHint.StaticDraw, BeginMode beginMode = BeginMode.Triangles) where T : struct
    {
      VertexBuffer vb = new VertexBuffer(format, data.Length, usageHint, beginMode);
      vb.SetData(data);
      return vb;
    }

    public static VertexBuffer CreateFromBytes(VertexFormat format, Byte[] data, BufferUsageHint usageHint = BufferUsageHint.StaticDraw, BeginMode beginMode = BeginMode.Triangles)
    {
      VertexBuffer vb = new VertexBuffer(format, data.Length / format.Stride, usageHint, beginMode);
      vb.SetData(data);
      return vb;
    }
  }
}
