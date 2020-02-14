using System;
using Minotaur.Graphics;
using OpenTK;

namespace MinotaurTests.Common
{
  public class GuiCamera : ICamera
  {
    private Viewport _viewport;
    private Matrix4 _projection;

    #region ICamera Members

    public float Far { get; set; }
    public float Near { get; set; }
    

    public Matrix4 Matrix
    {
      get { return _projection; }
    }


    public Matrix4 Projection
    {
      get { return _projection; }
    }

    public Matrix4 View
    {
      get { return Matrix4.Identity; }
    }

    public Viewport Viewport
    {
      get
      {
        return _viewport;
      }
      set
      {
        _viewport = value;
        Near = _viewport.MinDepth;
        Far = _viewport.MaxDepth;
        _projection = Matrix4.CreateOrthographicOffCenter(_viewport.X, _viewport.Width, _viewport.Height, _viewport.Y, Near, Far);
      }
    }

    #endregion

    public GuiCamera(int x, int y, int width, int height)
    {
      Viewport = new Viewport(x, y, width, height);
    }

    public GuiCamera(int width, int height)
      : this(0, 0, width, height) { }

    public GuiCamera(GraphicsDevice graphicsDevice)
    {
      Viewport = graphicsDevice.ViewPort;
    }
  }
}
