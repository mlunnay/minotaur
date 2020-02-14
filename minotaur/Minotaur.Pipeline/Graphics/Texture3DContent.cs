using System;

namespace Minotaur.Pipeline.Graphics
{
  public class Texture3DContent : TextureContent
  {
    public Texture3DContent()
      : base(new MipmapChainCollection(0, false)) { }
  }
}
