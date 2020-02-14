using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class ScreenAlignedQuad : GraphicsResource
  {
    private static ScreenAlignedQuad _instance;

    private VertexBuffer _vbo;
    private VertexArray _vao;

    public static ScreenAlignedQuad Instance { get { return _instance; } }

    public VertexArray VertexArray { get { return _vao; } }

    public ScreenAlignedQuad()
    {
      _vbo = VertexBuffer.Create(new VertexFormat(new List<VertexAttribute>() { new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 2) }),
        new float[] { -1f, -1f, 1f, -1f, -1f, 1f, 1f, 1f }, beginMode: BeginMode.TriangleStrip);
      _vao = new VertexArray(_vbo, null);
    }

    static ScreenAlignedQuad()
    {
      _instance = new ScreenAlignedQuad();
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _vao.Dispose();
          _vao = null;
          _vbo.Dispose();
          _vao = null;
        }
      }
      IsDisposed = true;
    }
  }
}
