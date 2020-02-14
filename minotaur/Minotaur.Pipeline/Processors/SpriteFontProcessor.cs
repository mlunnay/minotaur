using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Processors
{
  [ContentProcessor(DisplayName = "Sprite Font Processor")]
  public class SpriteFontProcessor : ContentProcessor<SpriteFontContent, SpriteFontContent>
  {
    public override SpriteFontContent Process(SpriteFontContent input, ContentProcessorContext context)
    {
      return input;
    }
  }
}
