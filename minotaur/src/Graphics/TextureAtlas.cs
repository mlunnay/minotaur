using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class TextureAtlas : Texture2D
  {
    public class AtlasSprite
    {
      public string Name;
      public int X;
      public int Y;
      public int Width;
      public int Height;
    }

    private class AtlasSpriteCollection : KeyedCollection<string, AtlasSprite>
    {
      public AtlasSpriteCollection()
        : base(null, 0) { }

      public AtlasSpriteCollection(IEnumerable<AtlasSprite> items)
        : this()
      {
        foreach (AtlasSprite item in items)
        {
          Add(item);
        }
      }

      protected override string GetKeyForItem(AtlasSprite item)
      {
        return item.Name;
      }
    }

    private AtlasSpriteCollection _sprites;

    public TextureAtlas(int width, int height,
      IEnumerable<AtlasSprite> sprites = null,
      PixelInternalFormat internalFormat = PixelInternalFormat.Rgba,
      PixelFormat format = PixelFormat.Rgba,
      PixelType pixelType = PixelType.UnsignedByte,
      TextureMinFilter minFilter = TextureMinFilter.Nearest, TextureMagFilter magFilter = TextureMagFilter.Nearest)
      : base(width, height, internalFormat, format, pixelType, minFilter, magFilter) 
    {
      if (sprites == null)
        _sprites = new AtlasSpriteCollection();
      else
        _sprites = new AtlasSpriteCollection(sprites);
    }

    public AtlasSprite this[int index]
    {
      get { return _sprites[index]; }
    }

    public AtlasSprite this[string name]
    {
      get { return _sprites[name]; }
    }

    public void AddSprite(string name,
      int x, int y, int width, int height)
    {
      _sprites.Add(new AtlasSprite() { Name = name, X = x, Y = y, Width = width, Height = height });
    }
  }
}
