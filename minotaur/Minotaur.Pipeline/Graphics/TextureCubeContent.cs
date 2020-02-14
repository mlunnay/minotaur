using System;

namespace Minotaur.Pipeline.Graphics
{
  public class TextureCubeContent : TextureContent
  {
    public TextureCubeContent()
      : base(new MipmapChainCollection(6)) { }
  }
}
