using System;
using System.ComponentModel;
using FreeImageAPI;
using Minotaur.Pipeline.Graphics;
using System.IO;

namespace Minotaur.Pipeline.Importers
{
  [ContentImporter(".bmp",
      ".jpg",
      ".jpeg",
      ".gif",
      ".hdr",
      ".exr",
      ".ico",
      ".png",
      ".psd",
      ".tga",
      ".tif",
      ".tiff",
      DisplayName = "2D Texture Importer",
      DefaultProcessor = "TextureProcessor")]
  public class TextureImporter : ContentImporter<TextureContent>
  {
    public bool IsLinear { get; set; }

    [DefaultValue(TextureType.Texture2D)]
    public TextureType TextureType { get; set; }

    public string[] CubeFaces { get; set; }

    public TextureImporter()
    {
      TextureType = Graphics.TextureType.Texture2D;
    }

    public override TextureContent Import(FileStream stream, ContentManager manager)
    {
      TextureContent content = null;
      if (TextureType != Graphics.TextureType.Texture3D)
      {
        FREE_IMAGE_FORMAT format = FreeImage.GetFIFFromFilename(stream.Name);
        FIBITMAP bitmap = FreeImage.LoadFromStream(stream, ref format);
        ImageType imageType;
        try
        {
          imageType = ImageTypeFromBitmap(ref bitmap);
        }
        catch (Exception e)
        {
          manager.Log.Error(string.Format("TextureContent Import error: {0}", e.Message));
          return null;
        }

        if (TextureType == Graphics.TextureType.Texture1D)
        {
          content = new Texture1DContent();
          content.Faces[0][0] = new BitmapContent((int)FreeImage.GetWidth(bitmap), (int)FreeImage.GetHeight(bitmap), imageType, bitmap);
        }
        else if (TextureType == Graphics.TextureType.Texture2D)
        {
          content = new Texture2DContent();
          content.Faces[0] = new MipmapChain(new BitmapContent((int)FreeImage.GetWidth(bitmap), (int)FreeImage.GetHeight(bitmap), imageType, bitmap));
        }
        else if (TextureType == Graphics.TextureType.TextureCube)
        {
          return ImportCubeTexture(bitmap, imageType, manager);
        } 
      }
      else
      {
        throw new NotImplementedException("Importing of 3D textures is not implemented.");
      }

      return content;
    }

    private TextureCubeContent ImportCubeTexture(FIBITMAP bitmap, ImageType imageType, ContentManager manager)
    {
      TextureCubeContent content = new TextureCubeContent();
      if (CubeFaces == null || CubeFaces.Length == 0)
      {
        int width = (int)FreeImage.GetWidth(bitmap);
        int height = (int)FreeImage.GetHeight(bitmap);
        // image is a cube cross
        if (width > height)  // horizontal cross
        {
          width = width / 4;
          height = height / 3;
          // +x
          content.Faces[0][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width * 2, height, width * 3, height * 2));
          // -x
          content.Faces[1][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, 0, height, width, height * 2));
          // +y
          content.Faces[2][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, 0, width * 2, height));
          // -y
          content.Faces[3][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, height * 2, width * 2, height * 3));
          // +z
          content.Faces[4][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, height, width * 2, height * 2));
          // -z
          content.Faces[5][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width * 3, height, width * 4, height * 2));
        }
        else  // vertical cross
        {
          width = width / 3;
          height = height / 4;
          // +x
          content.Faces[0][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width * 2, height, width * 3, height * 2));
          // -x
          content.Faces[1][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, 0, height, width, height * 2));
          // +y
          content.Faces[2][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, 0, width * 2, height));
          // -y
          content.Faces[3][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, height * 2, width * 2, height * 3));
          // +z
          content.Faces[4][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, height, width * 2, height * 2));
          // -z
          content.Faces[5][0] = new BitmapContent(width, height, imageType, FreeImage.Copy(bitmap, width, height * 3, width * 2, height * 4));
        }
      }
      else
      {
        int width = (int)FreeImage.GetWidth(bitmap);
        int height = (int)FreeImage.GetHeight(bitmap);

        try
        {
          // main image file is +x and CubeFaces contains images for [-x, +y, -y, +z, -z]
          content.Faces[0][0] = new BitmapContent(width, height, imageType, bitmap);
          FIBITMAP bitmap2 = FreeImage.LoadEx(CubeFaces[0]);  // -x
          content.Faces[1][0] = new BitmapContent(width, height, imageType, bitmap2);
          bitmap2 = FreeImage.LoadEx(CubeFaces[1]); // +y
          content.Faces[2][0] = new BitmapContent(width, height, imageType, bitmap2);
          bitmap2 = FreeImage.LoadEx(CubeFaces[2]); // -y
          content.Faces[3][0] = new BitmapContent(width, height, imageType, bitmap2);
          bitmap2 = FreeImage.LoadEx(CubeFaces[3]); // +z
          content.Faces[4][0] = new BitmapContent(width, height, imageType, bitmap2);
          bitmap2 = FreeImage.LoadEx(CubeFaces[4]); // -z
          content.Faces[5][0] = new BitmapContent(width, height, imageType, bitmap2);
        }
        catch (Exception)
        {
          manager.Log.Error("Error loading cubemap files.");
          content = null;
        }
      }

      return content;
    }

    private ImageType ImageTypeFromBitmap(ref FIBITMAP bitmap)
    {
      // returns the ImageType for the given bitmap, also converts the bitmap to a 24 or 32 bit image if it is pallatized.
      FREE_IMAGE_TYPE type = FreeImage.GetImageType(bitmap);
      uint bpp = FreeImage.GetBPP(bitmap);
      FIBITMAP tmp;
      ImageType imageType = ImageType.RGBA;

      if (type == FREE_IMAGE_TYPE.FIT_RGBF)
        return ImageType.RGBf;
      else if (type == FREE_IMAGE_TYPE.FIT_RGBAF)
        return ImageType.RGBAf;
      else if (type == FREE_IMAGE_TYPE.FIT_BITMAP)
      {
        if (bpp == 8 && FreeImage.GetTransparencyCount(bitmap) > 0)
        {
          tmp = bitmap;
          bitmap = FreeImage.ConvertTo32Bits(tmp);
          FreeImage.Unload(tmp);
          imageType = ImageType.RGBA;
        }
        else if (bpp == 8)
        {
          tmp = bitmap;
          bitmap = FreeImage.ConvertTo24Bits(tmp);
          FreeImage.Unload(tmp);
          imageType = ImageType.RGB;
        }
        else if (bpp == 24)
          imageType = ImageType.RGB;
        else if (bpp == 32)
          imageType = ImageType.RGBA;
        else
        {
          // if its another type convert it to a 24 bit image
          tmp = bitmap;
          bitmap = FreeImage.ConvertTo24Bits(tmp);
          FreeImage.Unload(tmp);
          imageType = ImageType.RGB;
        }
      }
      else
        throw new InvalidOperationException(string.Format("Unable to handle FREE_IMAGE_TYPE: {0}", type));

      if (IsLinear)
      {
        if (imageType == ImageType.RGB)
          imageType = ImageType.sRGB8;
        else if (imageType == ImageType.RGBA)
          imageType = ImageType.sRGBA8;
      }

      return imageType;
    }
  }
}
