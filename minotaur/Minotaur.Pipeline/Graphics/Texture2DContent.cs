using System;

namespace Minotaur.Pipeline.Graphics
{
  public class Texture2DContent : TextureContent
  {
    public Texture2DContent()
      : base(new MipmapChainCollection(1)) { }
  }
}
