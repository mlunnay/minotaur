using System.Collections.ObjectModel;

namespace Minotaur.Pipeline.Graphics
{
  public class MipmapChain : Collection<BitmapContent>
  {
    public MipmapChain() { }

    public MipmapChain(BitmapContent bitmap)
    {
      base.Add(bitmap);
    }

    public static implicit operator MipmapChain(BitmapContent bitmap)
    {
      return new MipmapChain(bitmap);
    }
  }
}
