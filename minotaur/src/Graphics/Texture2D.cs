using System;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class Texture2D : Texture
  {
    private int _width;
    private int _height;

    public override int Width { get { return _width; } }

    public override int Height { get { return _height; } }

    public TextureMinFilter MinFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)value);
        Unbind();
      }
    }

    public TextureMagFilter MagFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)value);
        Unbind();
      }
    }

    public Texture2D(int width, int height,
      PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
      PixelFormat format = PixelFormat.Rgba,
      PixelType pixelType = PixelType.UnsignedByte,
      TextureMinFilter minFilter = TextureMinFilter.Nearest, TextureMagFilter magFilter = TextureMagFilter.Nearest)
      : base(TextureTarget.Texture2D, internalFormat, format, pixelType)
    {
      _width = width;
      _height = height;
      Bind();
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
      Unbind();
    }

    /// <summary>
    /// Sets the Texture's data from a byte array
    /// </summary>
    /// <param name="data"></param>
    /// <param name="internalFormat"></param>
    /// <param name="format"></param>
    public void SetData(byte[] data, int mipLevel = 0)
    {
      SetData(data, mipLevel, InternalFormat, PixelFormat, PixelType);
    }

    public void SetData<T>(T[] data, int mipLevel, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType) where T : struct
    {
      Bind();
      GL.TexImage2D(Target, mipLevel, internalFormat, _width, _height, 0, format, pixelType, data);
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

    /// <summary>
    /// Creates a Texture from a System.Drawing.Bitmap
    /// </summary>
    /// <param name="bitmap"></param>
    /// <returns></returns>
    public static Texture2D Create(Bitmap bitmap, TextureMinFilter minFilter = TextureMinFilter.Linear, TextureMagFilter magFilter = TextureMagFilter.Linear, bool premultiplyAlpha = true)
    {
      Texture2D tex = new Texture2D(bitmap.Width, bitmap.Height, PixelInternalFormat.Rgba, PixelFormat.Bgra, PixelType.UnsignedByte, minFilter, magFilter);
      System.Drawing.Imaging.BitmapData bmp_data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
      int bytes = bmp_data.Stride * bmp_data.Height;
      byte[] data = new byte[bytes];
      System.Runtime.InteropServices.Marshal.Copy(bmp_data.Scan0, data, 0, bytes);
      bitmap.UnlockBits(bmp_data);
      
      for (int i = 0; i < bytes; i += 4)
      {
        data[i] = ((byte)(data[i] * (data[i + 3] / 255f)));
        data[i + 1] = ((byte)(data[i + 1] * (data[i + 3] / 255f)));
        data[i + 2] = ((byte)(data[i + 2] * (data[i + 3] / 255f)));
      }

      tex.Bind();
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data);
      tex.Unbind();
      return tex;
    }

    public static Texture2D CreateEmpty(int width, int height,
      PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
      PixelFormat format = PixelFormat.Rgba,
      PixelType pixelType = PixelType.UnsignedByte,
      TextureMinFilter minFilter = TextureMinFilter.Linear, TextureMagFilter magFilter = TextureMagFilter.Linear)
    {
      Texture2D tex = new Texture2D(width, height, internalFormat, format, pixelType, minFilter, magFilter);
      tex.Bind();
      GL.TexImage2D(TextureTarget.Texture2D, 0, internalFormat, width, height, 0, format, pixelType, IntPtr.Zero);
      tex.Unbind();
      return tex;
    }
  }
}
