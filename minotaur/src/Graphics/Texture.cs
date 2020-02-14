using System;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using Minotaur.Core;

using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace Minotaur.Graphics
{
  /// <summary>
  /// Represents an OpenGL Texture
  /// </summary>
  public abstract class Texture : GraphicsResource
  {
    #region Declarations

    private int _id;
    private TextureTarget _target;
    private PixelInternalFormat _internalFormat;
    private PixelFormat _format;
    private PixelType _pixelType;

    #endregion

    #region Properties

    public int ID { get { return _id; } }
    public TextureTarget Target { get { return _target; } }

    public PixelInternalFormat InternalFormat { get { return _internalFormat; } }
    public PixelFormat PixelFormat { get { return _format; } }
    public PixelType PixelType { get { return _pixelType; } }

    public virtual int Width { get { return 0; } }
    public virtual int Height { get { return 0; } }
    public virtual int Depth { get { return 0; } }

    #endregion

    #region Constructor

    protected Texture(TextureTarget target,
      PixelInternalFormat internalFormat,
      PixelFormat pixelFormat,
      PixelType pixelType)
    {
      _id = GL.GenTexture();
      _target = target;
      _internalFormat = internalFormat;
      _format = pixelFormat;
      _pixelType = pixelType;
    }

    #endregion

    #region Public Methods

    public void Bind()
    {
      GL.BindTexture(_target, _id);
    }

    public void Unbind()
    {
      GL.BindTexture(_target, 0);
    }

    #endregion

    #region Private Methods

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        // no managed object to dispose so skipping if disposing
        DisposalManager.Add(() => GL.DeleteTexture(_id));
        _id = 0;
      }
      IsDisposed = true;
    }

    #endregion
  }
}
