using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class StencilState: GraphicsResource, ICloneable
  {
    internal bool _dirty;
    private bool _stencilEnable;
    private StencilFunction _stencilFunction;
    private int _stencilReference;
    private StencilOp _stencilPass;
    private StencilOp _stencilFail;
    private StencilOp _stencilZFail;
    private int _stencilWriteMask;
    private int _stencilReadMask;

    public bool StencilEnable
    {
      get { return _stencilEnable; }
      set
      {
        if (_stencilEnable == value)
          return;
        _stencilEnable = value;
        _dirty = true;
      }
    }

    public StencilFunction StencilFunction
    {
      get { return _stencilFunction; }
      set
      {
        if (_stencilFunction == value)
          return;
        _stencilFunction = value;
        _dirty = true;
      }
    }

    public int StencilReference
    {
      get { return _stencilReference; }
      set
      {
        if (_stencilReference == value)
          return;
        _stencilReference = value;
        _dirty = true;
      }
    }

    public StencilOp StencilPass
    {
      get { return _stencilPass; }
      set
      {
        if (_stencilPass == value)
          return;
        _stencilPass = value;
        _dirty = true;
      }
    }

    public StencilOp StencilFail
    {
      get { return _stencilFail; }
      set
      {
        if (_stencilFail == value)
          return;
        _stencilFail = value;
        _dirty = true;
      }
    }

    public StencilOp StencilZFail
    {
      get { return _stencilZFail; }
      set
      {
        if (_stencilZFail == value)
          return;
        _stencilZFail = value;
        _dirty = true;
      }
    }

    public int StencilWriteMask
    {
      get { return _stencilWriteMask; }
      set
      {
        if (_stencilWriteMask == value)
          return;
        _stencilWriteMask = value;
        _dirty = true;
      }
    }

    public int StencilReadMask
    {
      get { return _stencilReadMask; }
      set
      {
        if (_stencilReadMask == value)
          return;
        _stencilReadMask = value;
        _dirty = true;
      }
    }

    public StencilState()
    {
      _stencilReference = 0;
      _stencilPass = StencilOp.Keep;
      _stencilFail = StencilOp.Keep;
      _stencilZFail = StencilOp.Keep;
      _stencilWriteMask = 0xff;
      _stencilReadMask = 0xff;
      _dirty = true;
    }

    public object Clone()
    {
      return new StencilState()
      {
        _dirty = _dirty,
        _stencilEnable = _stencilEnable,
        _stencilFail = _stencilFail,
        _stencilFunction = _stencilFunction,
        _stencilPass = _stencilPass,
        _stencilReadMask = _stencilReadMask,
        _stencilReference = _stencilReference,
        _stencilWriteMask = _stencilWriteMask,
        _stencilZFail = _stencilZFail
      };
    }

    public void Apply(GraphicsDevice device)
    {
      if (!_dirty)
        return;

      if (!_stencilEnable)
      {
        GL.Disable(EnableCap.StencilTest);
        Utilities.CheckGLError();
      }
      else
      {
        GL.Enable(EnableCap.StencilTest);
        Utilities.CheckGLError();

        GL.StencilFunc(StencilFunction, StencilReference, StencilReadMask);
        Utilities.CheckGLError();
        GL.StencilMask(StencilWriteMask);
        Utilities.CheckGLError();
        GL.StencilOp(StencilFail, StencilZFail, StencilPass);
        Utilities.CheckGLError();
      }
    }
  }
}
