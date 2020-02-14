using System;
using Minotaur.Graphics;
using OpenTK;
using OpenTK.Input;

namespace MinotaurTests.Common.GUI
{
  public class Slider : GuiComponent
  {
    private Binding _binding;
    private BitmapFont _font;
    Sprite _trackSprite;
    Sprite _thumbSprite;
    SpriteBatch _spriteBatch;
    private float _min;
    private float _max;

    public float Value
    {
      get { return _binding.Get<float>(); }
      set
      {
        _binding.Set(value);
      }
    }

    public float Min
    {
      get { return _min; }
      set { _min = value; }
    }

    public float Max
    {
      get { return _max; }
      set { _max = value; }
    }

    public string Content { get; set; }

    public Slider(string label,
      Binding binding,
      float min,
      float max,
      Sprite trackSprite,
      Sprite thumbSprite,
      BitmapFont font

      )
    {
      Content = label;
      _binding = binding;
      _trackSprite = trackSprite;
      _thumbSprite = thumbSprite;
      _font = font;
      _min = min;
      _max = max;

      Vector2 size = new Vector2(_trackSprite.Width, Math.Max(_trackSprite.Height, _thumbSprite.Height));
      Vector2 labelSize = font.MeasureString(String.Format(label, Value));
      size.X = Math.Max(size.X, labelSize.X);
      size.Y += labelSize.Y;
      _size = size;
    }

    public override void ComputeAbsoluteSizePosition()
    {
      Vector2 labelSize = _font.MeasureString(String.Format(Content, Value));
      _size.X = Math.Max(_size.X, labelSize.X);
      base.ComputeAbsoluteSizePosition();
    }

    public override void Render(GraphicsDevice graphicsDevice)
    {
      if (_spriteBatch == null)
        _spriteBatch = new SpriteBatch(graphicsDevice);

      Vector2 labelSize = _font.MeasureString(String.Format(Content, Value));
      int yOffset = (int)labelSize.Y;

      // limit knob pos so the knob doesn't go outside the edges!
	    float ks = (float)_thumbSprite.Width * 0.5f / _absSize.X;
	    float kn = (Value - _min) / (_max - _min);
      kn = kn < ks ? ks : kn > 1f - ks ? 1f - ks : kn;

      _spriteBatch.Begin(SpriteSortMode.Deffered, new RenderState() { BlendState = BlendState.AlphaBlend }, null, Shaders.FontShader);
      _spriteBatch.DrawString(_font, string.Format(Content, Value), (int)_absPosition.X, (int)_absPosition.Y);
      _spriteBatch.End();
      
      _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
      _spriteBatch.Draw(_trackSprite, (int)_absPosition.X, (int)_absPosition.Y + yOffset, (int)_absSize.X);
      _spriteBatch.Draw(_thumbSprite, (int)((_absPosition.X + kn * _absSize.X) - _thumbSprite.Width * 0.5f), (int)_absPosition.Y + yOffset);
      _spriteBatch.End();
    }

    public override bool MouseDown(Vector2 position, OpenTK.Input.MouseButton button)
    {
      if (button != MouseButton.Left)
        return false;
      if (!(Visible && Enabled))
        return false;
      if (!IsInside(position))
        return false;

      Focus();
      float value = (position.X - _absPosition.X) / _absSize.X;
      value = value * (_max - _min) + _min;
      Value = value < _min ? _min : value > _max ? _max : value;

      return true;
    }

    public override bool MouseMove(Vector2 position)
    {
      if (!(Visible && Enabled))
        return false;

      if (Parent != null && Parent.FocusChild == this)
      {
        float value = (position.X - _absPosition.X) / _absSize.X;
        value = value * (_max - _min) + _min;
        Value = value < _min ? _min : value > _max ? _max : value;
        return true;
      }

      return false;
    }

    public override bool MouseUp(Vector2 position, OpenTK.Input.MouseButton button)
    {
      if (button != MouseButton.Left)
        return false;

      if (Parent != null && Parent.FocusChild == this)
      {
        Unfocus();
        return true;
      }

      return false;
    }
  }
}
