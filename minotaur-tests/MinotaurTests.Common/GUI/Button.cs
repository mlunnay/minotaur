using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Graphics;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;

namespace MinotaurTests.Common.GUI
{
  public class Button : GuiComponent
  {
    private BitmapFont _font;
    private Sprite _normalSprite;
    private Sprite _hoverSprite;
    private Sprite _downSprite;
    private Sprite _currentSprite;

    private Sprite _imageSprite;
    private int[] _borders;

    private bool _buttonDown;

    private SpriteBatch _spriteBatch;

    private const int spacing = 6;

    public event EventHandler Click;

    public string Content { get; set; }

    public Button(string text,
      Sprite normalSprite,
      Sprite hoverSprite,
      Sprite downSprite,
      Sprite imageSprite,
      BitmapFont font)
    {
      Content = text;
      _font = font;
      _normalSprite = normalSprite;
      _hoverSprite = hoverSprite;
      _downSprite = downSprite;
      _imageSprite = imageSprite;

      _currentSprite = _normalSprite;

      Vector2 labelSize = font.MeasureString(text);
      Vector2 size = new Vector2(0);
      if (_imageSprite != null)
        size += new Vector2(_imageSprite.Width, _imageSprite.Height);
      if (!string.IsNullOrEmpty(text))
      {
        size.X += (size.X == 0 ? 0 : spacing) + labelSize.X;
        size.Y = Math.Max(size.Y, labelSize.Y);
      }
      if (normalSprite is SlicedSprite)
        _borders = ((SlicedSprite)normalSprite).Borders;
      size.X += _borders[0] + _borders[2];
      size.Y += _borders[1] + _borders[3];
      _size = size;
    }

    public override void ComputeAbsoluteSizePosition()
    {
      Vector2 labelSize = _font.MeasureString(Content);
      Vector2 size = new Vector2(0);
      if (_imageSprite != null)
        size += new Vector2(_imageSprite.Width, _imageSprite.Height);
      if (!string.IsNullOrEmpty(Content))
      {
        size.X += (size.X == 0 ? 0 : spacing) + labelSize.X;
        size.Y = Math.Max(size.Y, labelSize.Y);
      }
      size.X += _borders[0] + _borders[2];
      size.Y += _borders[1] + _borders[3];
      _size = size;
      base.ComputeAbsoluteSizePosition();
    }

    public override void Render(GraphicsDevice graphicsDevice)
    {
      if (_spriteBatch == null)
        _spriteBatch = new SpriteBatch(graphicsDevice);

      _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
      _spriteBatch.Draw(_currentSprite, (int)_absPosition.X, (int)_absPosition.Y, (int)_absSize.X, (int)_absSize.Y);
      if (_imageSprite != null)
        _spriteBatch.Draw(_imageSprite, (int)_absPosition.X + _borders[0], (int)(_absPosition.Y + (_absSize.Y - _imageSprite.Height) / 2));
      _spriteBatch.End();

      if (!string.IsNullOrEmpty(Content))
      {
        Vector2 labelSize = _font.MeasureString(Content);
        int xPos = (int)(_absPosition.X + _borders[1] + (_imageSprite != null ? _imageSprite.Width + spacing : 0));
        _spriteBatch.Begin(SpriteSortMode.Deffered, new RenderState() { BlendState = BlendState.AlphaBlend }, null, _font.Shader);
        _spriteBatch.DrawString(_font, Content, xPos, (int)(_absPosition.Y + _borders[1] + ((_absSize.Y - labelSize.Y) / 2)), Color4.Gray);
        _spriteBatch.End();
      }
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
      _currentSprite = _downSprite;
      _buttonDown = true;
      return true;
    }

    public override bool MouseUp(Vector2 position, OpenTK.Input.MouseButton button)
    {
      if (button != MouseButton.Left)
        return false;
      if (!(Visible && Enabled))
        return false;
      if (!IsInside(position))
        return false;

      if (Parent != null && Parent.FocusChild == this)
      {
        EventHandler handler = Click;
        if (handler != null)
          handler(this, EventArgs.Empty);
        _currentSprite = _normalSprite;
        _buttonDown = false;
        Unfocus();
        return true;
      }

      return false;
    }

    public override bool MouseMove(Vector2 position)
    {
      if (!(Visible && Enabled))
        return false;
      if (!IsInside(position))
      {
        if (_buttonDown)
        {
          _buttonDown = false;
          _currentSprite = _normalSprite;
        }
        return false;
      }

      return false;
    }
  }
}
