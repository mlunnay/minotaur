using System;
using System.ComponentModel;
using Minotaur.Pipeline.Graphics;
using FreeImageAPI;

namespace Minotaur.Pipeline.Processors
{
  [ContentProcessor(DisplayName="TextureProcessor")]
  public class TextureProcessor : ContentProcessor<TextureContent, TextureContent>
  {
    public virtual bool GenerateMipmaps { get; set; }

    [DefaultValueAttribute(true)]
    public virtual bool PremultiplyAlpha { get; set; }

    [DefaultValue(ImageType.RGB)]
    public ImageType OutputType { get; set; }

    public TextureProcessor()
    {
      PremultiplyAlpha = true;
    }

    public override TextureContent Process(TextureContent input, ContentProcessorContext context)
    {
      foreach (MipmapChain current in input.Faces)
      {
        foreach (BitmapContent item in current)
        {
          item.ConvertBitmap(OutputType);
        }
      }

      if (PremultiplyAlpha)
      {
        ImageType imageType = input.Faces[0][0].ImageType;
        // only premultiply if there is an alpha channel
        if (imageType == ImageType.CompressedRGBA ||
          imageType == ImageType.CompressedsRGBA ||
          imageType == ImageType.RGBA ||
          imageType == ImageType.sRGBA8)
        {
          foreach (MipmapChain current in input.Faces)
          {
            foreach (BitmapContent item in current)
            {
              FreeImage.PreMultiplyWithAlpha(item.Bitmap);
            }
          }
        }
      }

      if (GenerateMipmaps)
      {
        input.GenerateMipmaps(false);
      }

      return input;
    }
  }
}
