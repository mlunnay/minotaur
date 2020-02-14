using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Graphics;
using OpenTK;

namespace MinotaurTests.Common
{
  public class LookAtCamera : ICamera
  {
    private GraphicsDevice _graphicsDevice;

    public Vector3 Position { get; set; }

    public Vector3 Up { get; set; }

    public Vector3 LookAt { get; set; }

    #region ICamera Members

    public float Far { get; set; }

    public Matrix4 Matrix
    {
      get { return View * Projection; }
    }

    public float Near { get; set; }

    public Matrix4 Projection
    {
      get { return Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 4), _graphicsDevice.ViewPort.AspectRatio, Near, Far); }
    }

    public Matrix4 View
    {
      get { return Matrix4.LookAt(Position, LookAt, Up); }// * Matrix4.CreateTranslation(Position); }
    }

    public Viewport Viewport { get; set; }

    #endregion

    public LookAtCamera(GraphicsDevice graphicsDevice,
      Vector3 position,
      Vector3 up,
      Vector3 lookAt)
    {
      _graphicsDevice = graphicsDevice;
      Position = position;
      Up = up;
      LookAt = lookAt;
      Near = 1.0f;
      Far = 1000.0f;
    }

    public LookAtCamera(GraphicsDevice graphicsDevice)
      : this(graphicsDevice, new Vector3(0, -5f, 0), Vector3.UnitY, Vector3.Zero) { }
  }
}
