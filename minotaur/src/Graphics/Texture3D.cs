using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class Texture3D : Texture
  {
    private int _width;
    private int _height;
    private int _depth;

    public override int Width { get { return _width; } }

    public override int Height { get { return _height; } }

    public override int Depth { get { return _depth; } }

    public TextureMinFilter MinFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)value);
        Unbind();
      }
    }

    public TextureMinFilter MagFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)value);
        Unbind();
      }
    }

    public Texture3D(int width, int height, int depth,
      PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
      PixelFormat format = PixelFormat.Rgba,
      PixelType pixelType = PixelType.UnsignedByte,
      TextureMinFilter minFilter = TextureMinFilter.Nearest, TextureMagFilter magFilter = TextureMagFilter.Nearest)
      : base(TextureTarget.Texture3D, internalFormat, format, pixelType)
    {
      _width = width;
      _height = height;
      _depth = depth;
      Bind();
      GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMinFilter, (int)minFilter);
      GL.TexParameter(TextureTarget.Texture3D, TextureParameterName.TextureMagFilter, (int)magFilter);
      Unbind();
    }

    /// <summary>
    /// Sets the Texture's data from a byte array
    /// </summary>
    /// <param name="data"></param>
    /// <param name="internalFormat"></param>
    /// <param name="format"></param>
    public void SetData(byte[] data, PixelInternalFormat internalFormat = PixelInternalFormat.Rgba, PixelFormat format = PixelFormat.Rgba)
    {
      SetData(data, 0, internalFormat, format, PixelType.UnsignedByte);
    }

    public void SetData<T>(T[] data, int mipLevel, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType) where T : struct
    {
      Bind();
      GL.TexImage3D(Target, mipLevel, internalFormat, _width, _height, _depth, 0, format, pixelType, data);
      Unbind();
    }

    public void GetData<T>(T[] data, int mipLevel = 0) where T : struct
    {
      Bind();
      GL.GetTexImage(Target, mipLevel, PixelFormat, PixelType, data);
      Unbind();
    }

    public void GetData<T>(T[] data, int mipLevel, PixelFormat format, PixelType pixelType) where T : struct
    {
      Bind();
      GL.GetTexImage(Target, mipLevel, format, pixelType, data);
      Unbind();
    }
  }
}
