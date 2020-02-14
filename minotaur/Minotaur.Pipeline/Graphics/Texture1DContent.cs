using System;

namespace Minotaur.Pipeline.Graphics
{
  public class Texture1DContent : TextureContent
  {
    public Texture1DContent()
      : base(new MipmapChainCollection(1)) { }
  }
}
