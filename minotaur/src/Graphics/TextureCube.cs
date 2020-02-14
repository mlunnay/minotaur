using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class TextureCube : Texture
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

    public TextureMinFilter MagFilter
    {
      set
      {
        Bind();
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)value);
        Unbind();
      }
    }

    public TextureCube(int width, int height,
      PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
      PixelFormat format = PixelFormat.Rgba,
      PixelType pixelType = PixelType.UnsignedByte,
      TextureMinFilter minFilter = TextureMinFilter.Nearest, TextureMagFilter magFilter = TextureMagFilter.Nearest)
      : base(TextureTarget.TextureCubeMap, internalFormat, format, pixelType)
    {
      _width = width;
      _height = height;
      Bind();
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)minFilter);
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)magFilter);
      Unbind();
    }

    /// <summary>
    /// Sets the Texture's data from a byte array
    /// </summary>
    /// <param name="data"></param>
    /// <param name="internalFormat"></param>
    /// <param name="format"></param>
    public void SetData(CubeMapFace face, byte[] data, int mipLevel = 0)
    {
      SetData(face, data, mipLevel, InternalFormat, PixelFormat, PixelType);
    }

    public void SetData<T>(CubeMapFace face, T[] data, int mipLevel, PixelInternalFormat internalFormat, PixelFormat format, PixelType pixelType) where T : struct
    {
      Bind();
      GL.TexImage2D(GetCubeFace(face), mipLevel, internalFormat, _width, _height, 0, format, pixelType, data);
      Unbind();
    }

    public void GetData<T>(CubeMapFace face, T[] data, int mipLevel = 0) where T : struct
    {
      Bind();
      GL.GetTexImage(GetCubeFace(face), mipLevel, PixelFormat, PixelType, data);
      Unbind();
    }

    public void GetData<T>(CubeMapFace face, T[] data, int mipLevel, PixelFormat format, PixelType pixelType) where T : struct
    {
      Bind();
      GL.GetTexImage(GetCubeFace(face), mipLevel, format, pixelType, data);
      Unbind();
    }

    private TextureTarget GetCubeFace(CubeMapFace face)
    {
      switch (face)
      {
        case CubeMapFace.PositiveX: return TextureTarget.TextureCubeMapPositiveX;
        case CubeMapFace.NegativeX: return TextureTarget.TextureCubeMapNegativeX;
        case CubeMapFace.PositiveY: return TextureTarget.TextureCubeMapPositiveY;
        case CubeMapFace.NegativeY: return TextureTarget.TextureCubeMapNegativeY;
        case CubeMapFace.PositiveZ: return TextureTarget.TextureCubeMapPositiveZ;
        case CubeMapFace.NegativeZ: return TextureTarget.TextureCubeMapNegativeZ;
      }
      throw new ArgumentException();
    }
  }
}
