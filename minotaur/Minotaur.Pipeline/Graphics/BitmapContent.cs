using System;
using System.Linq;
using FreeImageAPI;
using Minotaur.Content;

namespace Minotaur.Pipeline.Graphics
{
  public class BitmapContent : ContentItem, IDisposable
  {
    private FIBITMAP _bitmap;
    private bool _isDisposed = false;

    public int Width { get; set; }

    public int Height { get; set; }

    public ImageType ImageType { get; private set; }

    public FIBITMAP Bitmap { get { return _bitmap; } }

    public BitmapContent()
      : this(0, 0) { }

    public BitmapContent(int width, int height)
    {
      Width = width;
      Height = height;
      _bitmap = FreeImage.Allocate(width, height, 24);  // default to RGB image
      ImageType = ImageType.RGB;
    }

    public BitmapContent(int width, int height, ImageType imageType, byte[] data)
    {
      Width = width;
      Height = height;
      ImageType = imageType;
      int bpp;
      FREE_IMAGE_TYPE type = ImageTypeToFreeImageType(imageType, out bpp);
      _bitmap = FreeImage.ConvertFromRawBits(data, type, width, height, width * bpp, (uint)bpp, 0, 0, 0, false);
    }

    internal BitmapContent(int width, int height, ImageType imageType, FIBITMAP bitmap) 
    {
      Width = width;
      Height = height;
      ImageType = imageType;
      _bitmap = bitmap;
    }

    public byte[] Data()
    {
      byte[] data = new byte[FreeImage.GetHeight(_bitmap) * FreeImage.GetPitch(_bitmap)];
      FreeImage.ConvertToRawBits(data, _bitmap, (int)FreeImage.GetPitch(_bitmap),
        FreeImage.GetBPP(_bitmap), FreeImage.GetRedMask(_bitmap),
        FreeImage.GetGreenMask(_bitmap), FreeImage.GetBlueMask(_bitmap), true);
      // need to swith red and blue bytes
      int stride = (int)(FreeImage.GetLine(_bitmap) / FreeImage.GetWidth(_bitmap));
      for (int i = 0; i < FreeImage.GetHeight(_bitmap) * FreeImage.GetWidth(_bitmap); i += stride)
      {
        byte tmp = data[i];
        data[i] = data[i + 2];
        data[i + 2] = tmp;
      }

      if (ImageType == Graphics.ImageType.R8 || ImageType == Graphics.ImageType.RG8)
      {
        if (ImageType == Graphics.ImageType.R8)
          data = data.Where((b, i) => i % stride == 0).ToArray();
        else
          data = data.Where((b, i) => i % stride == 0 || i % stride == 1).ToArray();
      }

      // TODO: handle compressed textures.

      return data;
    }

    public void ConvertBitmap(ImageType newImageType)
    {
      if (ImageType == newImageType)
        return;

      int newbpp, oldbpp;
      FREE_IMAGE_TYPE newType = ImageTypeToFreeImageType(newImageType, out newbpp);
      FREE_IMAGE_TYPE oldType = ImageTypeToFreeImageType(ImageType, out oldbpp);
      if (newType != oldType || newbpp != oldbpp)
      {
        FIBITMAP newBitmap = FreeImage.ConvertToType(_bitmap, newType, false);
        FreeImage.UnloadEx(ref _bitmap);
        _bitmap = newBitmap;
      }

      // check if a change in color space is needed.
      if (IsGammaCorrected(ImageType) != IsGammaCorrected(newImageType))
      {
        double gamma;
        if (IsGammaCorrected(ImageType)) // from linear color space to non linear
          gamma = 1d / 2.2d;
        else
          gamma = 2.2d;

        if (!FreeImage.AdjustGamma(_bitmap, gamma))
          throw new ContentLoadException("FreeImage was unable to adjust the bitmap's gamma");
      }

      ImageType = newImageType;
    }

    public BitmapContent GetScaledBitmapContent(int width, int height, FREE_IMAGE_FILTER filter = FREE_IMAGE_FILTER.FILTER_CATMULLROM)
    {
      FIBITMAP bitmap = FreeImage.Rescale(_bitmap, width, height, filter);
      return new BitmapContent(width, height, ImageType, bitmap);
    }

    private static bool IsGammaCorrected(ImageType type)
    {
      return type == ImageType.sRGB8 || type == ImageType.sRGBA8 || type == ImageType.CompressedsRGBA;
    }

    private FREE_IMAGE_TYPE ImageTypeToFreeImageType(ImageType imageType, out int bpp)
    {
      FREE_IMAGE_TYPE output = FREE_IMAGE_TYPE.FIT_BITMAP;
      bpp = 32;
      switch (imageType)
      {
        case ImageType.RGB:
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 24;
          break;
        case ImageType.RGBA:
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 32;
          break;
        case ImageType.RGB32ui:
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 128;
          break;
        case ImageType.RBG16f:
          throw new NotSupportedException("RBG16f is not currently supported");
        case ImageType.R8:  // dont do any change
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 24;
          break;
        case ImageType.RG8: // dont do any change
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 24;
          break;
        case ImageType.CompressedRGBA:  // Image compression is handled in the writer
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 32;
          break;
        case ImageType.CompressedsRGBA: // Image compression is handled in the writer
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 32;
          break;
        case ImageType.RGBAf:
          output = FREE_IMAGE_TYPE.FIT_RGBAF;
          bpp = 128;
          break;
        case ImageType.sRGB8:
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 24;
          break;
        case ImageType.sRGBA8:
          output = FREE_IMAGE_TYPE.FIT_BITMAP;
          bpp = 32;
          break;
        case ImageType.Depth32f:
          output = FREE_IMAGE_TYPE.FIT_FLOAT;
          bpp = 32;
          break;
        case ImageType.RGBf:
          output = FREE_IMAGE_TYPE.FIT_RGBF;
          bpp = 96;
          break;
        default:
          break;
      }

      return output;
    }

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
      if (!_isDisposed)
      {
        if (disposing)
        {
          FreeImage.Unload(_bitmap);
        }

        _isDisposed = true;
      }
    }

    #endregion
  }
}
