using System;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class BlendState : GraphicsResource, ICloneable
  {
    internal bool _dirty;
    private Color4 _blendColor;
    private BlendEquationMode _colorBlendFunction;
    private BlendEquationMode _alphaBlendFunction;
    private BlendingFactorSrc _colorSourceBlend;
    private BlendingFactorDest _colorDestinationBlend;
    private BlendingFactorSrc _alphaSourceBlend;
    private BlendingFactorDest _alphaDestinationBlend;
    private ColorWriteChannels _colorWriteChannels;
    
    public Color4 BlendColor
    {
      get { return _blendColor; }
      set
      {
        if (_blendColor == value)
          return;
        _blendColor = value;
        _dirty = true;
      }
    }

    public BlendEquationMode ColorBlendFunction
    {
      get { return _colorBlendFunction; }
      set
      {
        if (_colorBlendFunction == value)
          return;
        _colorBlendFunction = value;
        _dirty = true;
      }
    }

    public BlendEquationMode AlphaBlendFunction
    {
      get { return _alphaBlendFunction; }
      set
      {
        if (_alphaBlendFunction == value)
          return;
        _alphaBlendFunction = value;
        _dirty = true;
      }
    }

    public BlendingFactorSrc ColorSourceBlend
    {
      get { return _colorSourceBlend; }
      set
      {
        if (_colorSourceBlend == value)
          return;
        _colorSourceBlend = value;
        _dirty = true;
      }
    }

    public BlendingFactorDest ColorDestinationBlend
    {
      get { return _colorDestinationBlend; }
      set
      {
        if (_colorDestinationBlend == value)
          return;
        _colorDestinationBlend = value;
        _dirty = true;
      }
    }

    public BlendingFactorSrc AlphaSourceBlend
    {
      get { return _alphaSourceBlend; }
      set
      {
        if (_alphaSourceBlend == value)
          return;
        _alphaSourceBlend = value;
        _dirty = true;
      }
    }

    public BlendingFactorDest AlphaDestinationBlend
    {
      get { return _alphaDestinationBlend; }
      set
      {
        if (_alphaDestinationBlend == value)
          return;
        _alphaDestinationBlend = value;
        _dirty = true;
      }
    }

    public ColorWriteChannels ColorWriteChannels
    {
      get { return _colorWriteChannels; }
      set
      {
        if (_colorWriteChannels == value)
          return;
        _colorWriteChannels = value;
        _dirty = true;
      }
    }

    public static readonly BlendState Additive;
    public static readonly BlendState AlphaBlend;
    public static readonly BlendState NonPremultiplied;
    public static readonly BlendState Opaque;

    public BlendState()
    {
      _alphaBlendFunction = BlendEquationMode.FuncAdd;
      _alphaDestinationBlend = BlendingFactorDest.Zero;
      _alphaSourceBlend = BlendingFactorSrc.One;
      _colorBlendFunction = BlendEquationMode.FuncAdd;
      _colorDestinationBlend = BlendingFactorDest.Zero;
      _colorSourceBlend = BlendingFactorSrc.One;
      _colorWriteChannels = ColorWriteChannels.All;
      _blendColor = new Color4(0,0,0,0);
      _dirty = true;
    }

    static BlendState()
    {
      Additive = new BlendState()
      {
        ColorSourceBlend = BlendingFactorSrc.SrcAlpha,
        AlphaSourceBlend = BlendingFactorSrc.SrcAlpha,
        ColorDestinationBlend = BlendingFactorDest.One,
        AlphaDestinationBlend = BlendingFactorDest.One
      };

      AlphaBlend = new BlendState()
      {
        ColorSourceBlend = BlendingFactorSrc.One,
        AlphaSourceBlend = BlendingFactorSrc.One,
        ColorDestinationBlend = BlendingFactorDest.OneMinusSrcAlpha,
        AlphaDestinationBlend = BlendingFactorDest.OneMinusSrcAlpha
      };

      NonPremultiplied = new BlendState()
      {
        ColorSourceBlend = BlendingFactorSrc.SrcAlpha,
        AlphaSourceBlend = BlendingFactorSrc.SrcAlpha,
        ColorDestinationBlend = BlendingFactorDest.OneMinusSrcAlpha,
        AlphaDestinationBlend = BlendingFactorDest.OneMinusSrcAlpha
      };

      Opaque = new BlendState()
      {
        ColorSourceBlend = BlendingFactorSrc.One,
        AlphaSourceBlend = BlendingFactorSrc.One,
        ColorDestinationBlend = BlendingFactorDest.Zero,
        AlphaDestinationBlend = BlendingFactorDest.Zero
      };
    }

    internal void Apply(GraphicsDevice device)
    {
      if (!_dirty)
        return;

      bool blendEnabled = !(_colorSourceBlend == BlendingFactorSrc.One &&
                           _colorDestinationBlend == BlendingFactorDest.Zero &&
                           _alphaSourceBlend == BlendingFactorSrc.One &&
                           _alphaDestinationBlend == BlendingFactorDest.Zero);
      if (!blendEnabled)
      {
        GL.Disable(EnableCap.Blend);
        Utilities.CheckGLError();
      }
      else
      {
        GL.Enable(EnableCap.Blend);
        Utilities.CheckGLError();

        GL.BlendColor(_blendColor);
        Utilities.CheckGLError();

        GL.BlendEquationSeparate(
            _colorBlendFunction,
            _alphaBlendFunction);
        Utilities.CheckGLError();

        GL.BlendFuncSeparate(
            _colorSourceBlend,
            _colorDestinationBlend,
            _alphaSourceBlend,
            _alphaDestinationBlend);
        Utilities.CheckGLError();

        GL.ColorMask(
            (_colorWriteChannels & ColorWriteChannels.Red) != 0,
            (_colorWriteChannels & ColorWriteChannels.Green) != 0,
            (_colorWriteChannels & ColorWriteChannels.Blue) != 0,
            (_colorWriteChannels & ColorWriteChannels.Alpha) != 0);
        Utilities.CheckGLError();
      }

      _dirty = false;
    }

    public object Clone()
    {
      return new BlendState()
      {
        _alphaBlendFunction = _alphaBlendFunction,
        _alphaDestinationBlend = _alphaDestinationBlend,
        _alphaSourceBlend = _alphaSourceBlend,
        _blendColor = _blendColor,
        _colorBlendFunction = _colorBlendFunction,
        _colorDestinationBlend = _colorDestinationBlend,
        _colorSourceBlend = _colorSourceBlend,
        _colorWriteChannels = _colorWriteChannels,
        _dirty = _dirty
      };
    }
  }
}
