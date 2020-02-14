using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Graphics;

namespace MinotaurTests.Common.GUI
{
  public class Label : GuiComponent
  {
    private BitmapFont _font;
    private SpriteBatch _spriteBatch;

    public string Content { get; set; }

    public Label(string label, BitmapFont font)
    {
      Content = label;
      _font = font;
      Size = font.MeasureString(label);
    }

    public override void Render(GraphicsDevice graphicsDevice)
    {
      if (_spriteBatch == null)
        _spriteBatch = new SpriteBatch(graphicsDevice);

      _spriteBatch.Begin(SpriteSortMode.Deffered, new RenderState() { BlendState = BlendState.AlphaBlend }, null, Shaders.FontShader);
      _spriteBatch.DrawString(_font, Content, (int)AbsolutePosition.X, (int)AbsolutePosition.Y);
      _spriteBatch.End();
    }
  }
}
