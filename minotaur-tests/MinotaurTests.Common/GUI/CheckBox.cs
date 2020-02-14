using System;
using Minotaur.Graphics;
using OpenTK;
using OpenTK.Input;

namespace MinotaurTests.Common.GUI
{
  public class CheckBox : GuiComponent
  {
    private Binding _binding;
    private BitmapFont _font;
    private Sprite _onSprite;
    private Sprite _offSprite;
    private SpriteBatch _spriteBatch;

    public bool Value
    {
      get { return _binding.Get<bool>(); }
      set
      {
        _binding.Set(value);
      }
    }

    public string Content { get; set; }

    public CheckBox(string label,
      Binding binding,
      Sprite onSprite,
      Sprite offSprite,
      BitmapFont font)
    {
      Content = label;
      _binding = binding;
      _onSprite = onSprite;
      _offSprite = offSprite;
      _font = font;

      Vector2 size = new Vector2(offSprite.Width, offSprite.Height);
      Vector2 labelSize = font.MeasureString(label);
      size.X = size.X + labelSize.X + 6;
      size.Y = Math.Max(size.Y, labelSize.Y);
      Size = size;
    }

    public override void ComputeAbsoluteSizePosition()
    {
      base.ComputeAbsoluteSizePosition();
      Vector2 labelSize = _font.MeasureString(Content);
      _absSize.Y = Math.Max(labelSize.Y, _offSprite.Height);
    }

    public override void Render(GraphicsDevice graphicsDevice)
    {
      // render the button
      if(_spriteBatch == null)
        _spriteBatch = new SpriteBatch(graphicsDevice);

      _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
      _spriteBatch.Draw(Value ? _onSprite : _offSprite, (int)AbsolutePosition.X, (int)(AbsolutePosition.Y + (AbsoluteSize.Y - _offSprite.Height) / 2),
        _offSprite.Width, _offSprite.Width, this.Color);
      _spriteBatch.End();

      _spriteBatch.Begin(SpriteSortMode.Deffered, new RenderState() { BlendState = BlendState.AlphaBlend }, null, Shaders.FontShader);
      _spriteBatch.DrawString(_font, Content, (int)(AbsolutePosition.X + _offSprite.Width + 6), (int)(AbsolutePosition.Y + ((_offSprite.Height - _font.CharacterSet.LineHeight) / 2)));
      _spriteBatch.End();
    }

    public override bool MouseDown(Vector2 position, MouseButton button)
    {
      if (button != MouseButton.Left)
        return false;
      if (!(Visible && Enabled))
        return false;
      if (!IsInside(position))
        return false;

      Focus();
      return true;
    }

    public override bool MouseUp(Vector2 position, MouseButton button)
    {
      if (button != MouseButton.Left)
        return false;
      if (!(Visible && Enabled))
        return false;
      if (!IsInside(position))
        return false;

      if (Parent != null && Parent.FocusChild == this)
      {
        Value = !Value;
        Unfocus();
        return true;
      }

      return false;
    }
  }
}
