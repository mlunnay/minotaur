using System;

namespace Minotaur.Pipeline.Graphics
{
  public class TextureContent : ContentItem
  {
    private MipmapChainCollection _faces;

    public MipmapChainCollection Faces { get { return _faces; } }

    public ImageType ImageType { get { return _faces[0][0].ImageType; } }

    public ImageType OutputType { get; set; }

    public TextureContent(MipmapChainCollection faces)
    {
      _faces = faces;
    }

    public virtual void GenerateMipmaps(bool overwriteExisting = true)
    {
      foreach (MipmapChain  current in _faces)
      {
        if (current.Count != 0)
        {
          if (overwriteExisting)
          {
            while (current.Count > 1)
            {
              current.RemoveAt(current.Count - 1);
            }
          }
          else
          {
            if (current.Count > 1)
              continue;
          }

          BitmapContent bitmap = current[0];
          int width = bitmap.Width;
          int height = bitmap.Height;
          while (width > 1 || height  > 1)
          {
            width = Math.Max(width >> 1, 1);
            height = Math.Max(width >> 1, 1);
            BitmapContent mipmap = bitmap.GetScaledBitmapContent(width, height);
            current.Add(mipmap);
          }
        }
      }
    }

    public void ConvertBitmapType(ImageType newType)
    {
      foreach (MipmapChain current in _faces)
      {
        foreach (BitmapContent bitmap in current)
        {
          bitmap.ConvertBitmap(newType);
        }
      }
    }
  }
}
