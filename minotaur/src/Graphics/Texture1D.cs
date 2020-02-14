using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class Texture1D : Texture
  {
    private int _width;

    public override int Width { get { return _width; } }

    public TextureMinFilter MinFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)value);
        Unbind();
      }
    }

    public TextureMinFilter MagFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)value);
        Unbind();
      }
    }

    public Texture1D(int width,
      PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
      PixelFormat format = PixelFormat.Rgba,
      PixelType pixelType = PixelType.UnsignedByte,
      TextureMinFilter minFilter = TextureMinFilter.Nearest,
      TextureMagFilter magFilter = TextureMagFilter.Nearest)
      : base(OpenTK.Graphics.OpenGL.TextureTarget.Texture1D, internalFormat, format, pixelType)
    {
      _width = width;
      Bind();
      GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMinFilter, (int)minFilter);
      GL.TexParameter(TextureTarget.Texture1D, TextureParameterName.TextureMagFilter, (int)magFilter);
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
      GL.TexImage1D(Target, mipLevel, internalFormat, _width, 0, format, pixelType, data);
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
