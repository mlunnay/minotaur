using System;
using Minotaur.Core;
using Minotaur.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace Minotaur.Content
{
  public class Texture2DReader : ContentTypeReader<Texture2D>
  {
    public Texture2DReader()
      : base(new Guid("e5e29004-6f77-4be4-a9c6-c3eb363ca021")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {

    }

    public override object Read(ContentReader reader)
    {
      UInt32 format = reader.ReadUInt32();
      int width = reader.ReadInt32();
      int height = reader.ReadInt32();
      int mipmapCount = reader.ReadInt32();
      PixelInternalFormat internalFormat;
      PixelFormat pixelFormat;
      PixelType pixelType;
      PixelFormatFromInt((int)format, out internalFormat, out pixelFormat, out pixelType);
      //Texture2D texture = new Texture2D(width, height, internalFormat);
      Texture2D texture = Texture2D.CreateEmpty(width, height, internalFormat);

      for (int i = 0; i < mipmapCount; i++)
      {
        UInt32 dataSize = reader.ReadUInt32();
        byte[] imageData = reader.ReadBytes((int)dataSize);
        SetMipmap(texture, i, width, height, imageData, internalFormat, pixelFormat, pixelType, format == 6 || format == 7);
        width /= 2;
        height /= 2;
      }

      return texture;
    }

    private void SetMipmap(Texture2D texture, int level, int width, int height, byte[] data, PixelInternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType, bool compressed)
    {
      if (compressed)
      {
        int size = ((width+3)/4)*((height+3)/4)*16; // 16 is the block size
        texture.Bind();
        GL.CompressedTexImage2D(TextureTarget.Texture2D, level, internalFormat, width, height, 0, 0, data);
        texture.Unbind();
      }
      else
      {
        //int pbo;
        //GL.GenBuffers(1, out pbo);
        //GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pbo);
        //IntPtr ptr = GL.MapBuffer(BufferTarget.PixelUnpackBuffer, BufferAccess.WriteOnly);
        //Marshal.Copy(data, 0, (IntPtr)ptr, data.Length);
        //texture.Bind();
        //GL.TexSubImage2D(TextureTarget.Texture2D, level, 0, 0, width, height, pixelFormat, pixelType, IntPtr.Zero);
        //texture.Unbind();
        //GL.UnmapBuffer(BufferTarget.PixelUnpackBuffer);
        //GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        texture.Bind();
        GL.TexImage2D(TextureTarget.Texture2D, level, internalFormat, width, height, 0, pixelFormat, pixelType, data);
        texture.Unbind();
      }
      Utilities.CheckGLError();
    }

    public static void PixelFormatFromInt(int value, out PixelInternalFormat internalFormat, out PixelFormat pixelFormat, out PixelType pixelType)
    {
      pixelType = PixelType.UnsignedByte;
      switch (value)
      {
        case 0:
          internalFormat = PixelInternalFormat.Rgb;
          pixelFormat = PixelFormat.Rgb;
          break;
        case 1:
          internalFormat = PixelInternalFormat.Rgba;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 2:
          internalFormat = PixelInternalFormat.Rgba32ui;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 3:
          internalFormat = PixelInternalFormat.Rgba16f;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 4:
          internalFormat = PixelInternalFormat.R8;
          pixelFormat = PixelFormat.Red;
          break;
        case 5:
          internalFormat = PixelInternalFormat.Rg8;
          pixelFormat = PixelFormat.Rg;
          break;
        case 6:
          internalFormat = PixelInternalFormat.CompressedRgbaS3tcDxt5Ext;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 7:
          internalFormat = PixelInternalFormat.CompressedSrgbAlphaS3tcDxt5Ext;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 8:
          internalFormat = PixelInternalFormat.Rgba32f;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 9:
          internalFormat = PixelInternalFormat.Srgb8;
          pixelFormat = PixelFormat.Rgb;
          break;
        case 10:
          internalFormat = PixelInternalFormat.Srgb8Alpha8;
          pixelFormat = PixelFormat.Rgba;
          break;
        case 11:
          internalFormat = PixelInternalFormat.DepthComponent32f;
          pixelFormat = PixelFormat.DepthComponent;
          break;
        case 12:
          internalFormat = PixelInternalFormat.Rgb32f;
          pixelFormat = PixelFormat.Rgb;
          break;
        default:
          throw new ArgumentOutOfRangeException(string.Format("Unknown PixelFormat Enumeration: {0}", value));
      }
    }
  }
}
