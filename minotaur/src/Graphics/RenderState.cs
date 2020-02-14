using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class RenderState : GraphicsResource, ICloneable
  {
    private Stack<BlendState> _blendState;
    private Stack<RasterizerState> _rasterizerState;
    private Stack<DepthState> _depthState;
    private Stack<StencilState> _stencilState;

    public BlendState BlendState
    {
      get { return _blendState.Peek(); }
      set
      {
        _blendState.Peek().BlendColor = value.BlendColor;
        _blendState.Peek().ColorBlendFunction = value.ColorBlendFunction;
        _blendState.Peek().AlphaBlendFunction = value.AlphaBlendFunction;
        _blendState.Peek().ColorSourceBlend = value.ColorSourceBlend;
        _blendState.Peek().ColorDestinationBlend = value.ColorDestinationBlend;
        _blendState.Peek().AlphaSourceBlend = value.AlphaSourceBlend;
        _blendState.Peek().AlphaDestinationBlend = value.AlphaDestinationBlend;
        _blendState.Peek().ColorWriteChannels = value.ColorWriteChannels;
      }
    }

    public Color4 BlendColor
    {
      get { return _blendState.Peek().BlendColor; }
      set { _blendState.Peek().BlendColor = value; }
    }

    public BlendEquationMode ColorBlendFunction
    {
      get { return _blendState.Peek().ColorBlendFunction; }
      set { _blendState.Peek().ColorBlendFunction = value; }
    }

    public BlendEquationMode AlphaBlendFunction
    {
      get { return _blendState.Peek().AlphaBlendFunction; }
      set { _blendState.Peek().AlphaBlendFunction = value; }
    }

    public BlendingFactorSrc ColorSourceBlend
    {
      get { return _blendState.Peek().ColorSourceBlend; }
      set { _blendState.Peek().ColorSourceBlend = value; }
    }

    public BlendingFactorDest ColorDestinationBlend
    {
      get { return _blendState.Peek().ColorDestinationBlend; }
      set { _blendState.Peek().ColorDestinationBlend = value; }
    }

    public BlendingFactorSrc AlphaSourceBlend
    {
      get { return _blendState.Peek().AlphaSourceBlend; }
      set { _blendState.Peek().AlphaSourceBlend = value; }
    }

    public BlendingFactorDest AlphaDestinationBlend
    {
      get { return _blendState.Peek().AlphaDestinationBlend; }
      set { _blendState.Peek().AlphaDestinationBlend = value; }
    }

    public ColorWriteChannels ColorWriteChannels
    {
      get { return _blendState.Peek().ColorWriteChannels; }
      set { _blendState.Peek().ColorWriteChannels = value; }
    }

    public RasterizerState RasterizerState
    {
      get { return _rasterizerState.Peek(); }
      set
      {
        _rasterizerState.Peek().CullMode = value.CullMode;
        _rasterizerState.Peek().FillMode = value.FillMode;
        _rasterizerState.Peek().DepthBias = value.DepthBias;
        _rasterizerState.Peek().SlopeScaleDepthBias = value.SlopeScaleDepthBias;
        _rasterizerState.Peek().ScissorTestEnable = value.ScissorTestEnable;
      }
    }

    public CullMode CullMode
    {
      get { return _rasterizerState.Peek().CullMode; }
      set { _rasterizerState.Peek().CullMode = value; }
    }

    public PolygonMode FillMode
    {
      get { return _rasterizerState.Peek().FillMode; }
      set { _rasterizerState.Peek().FillMode = value; }
    }

    public Single DepthBias
    {
      get { return _rasterizerState.Peek().DepthBias; }
      set { _rasterizerState.Peek().DepthBias = value; }
    }

    public Single SlopeScaleDepthBias
    {
      get { return _rasterizerState.Peek().SlopeScaleDepthBias; }
      set { _rasterizerState.Peek().SlopeScaleDepthBias = value; }
    }

    public bool ScissorTestEnable
    {
      get { return _rasterizerState.Peek().ScissorTestEnable; }
      set { _rasterizerState.Peek().ScissorTestEnable = value; }
    }

    public DepthState DepthState
    {
      get { return _depthState.Peek(); }
      set
      {
        _depthState.Peek().DepthEnabled = value.DepthEnabled;
        _depthState.Peek().DepthWrite = value.DepthWrite;
        _depthState.Peek().DepthFunction = value.DepthFunction;
        _depthState.Peek().DepthOffsetFactor = value.DepthOffsetFactor;
        _depthState.Peek().DepthOffsetUnits = value.DepthOffsetUnits;
      }
    }

    public bool DepthEnabled
    {
      get { return _depthState.Peek().DepthEnabled; }
      set { _depthState.Peek().DepthEnabled = value; }
    }

    public bool DepthWrite
    {
      get { return _depthState.Peek().DepthWrite; }
      set { _depthState.Peek().DepthWrite = value; }
    }

    public DepthFunction DepthFunction
    {
      get { return _depthState.Peek().DepthFunction; }
      set { _depthState.Peek().DepthFunction = value; }
    }

    public Single DepthOffsetFactor
    {
      get { return _depthState.Peek().DepthOffsetFactor; }
      set { _depthState.Peek().DepthOffsetFactor = value; }
    }

    public Single DepthOffsetUnits
    {
      get { return _depthState.Peek().DepthOffsetUnits; }
      set { _depthState.Peek().DepthOffsetUnits = value; }
    }

    public StencilState StencilState
    {
      get { return _stencilState.Peek(); }
      set
      {
        _stencilState.Peek().StencilEnable = value.StencilEnable;
        _stencilState.Peek().StencilFunction = value.StencilFunction;
        _stencilState.Peek().StencilReference = value.StencilReference;
        _stencilState.Peek().StencilPass = value.StencilPass;
        _stencilState.Peek().StencilFail = value.StencilFail;
        _stencilState.Peek().StencilZFail = value.StencilZFail;
        _stencilState.Peek().StencilWriteMask = value.StencilWriteMask;
        _stencilState.Peek().StencilReadMask = value.StencilReadMask;
      }
    }

    public bool StencilEnable
    {
      get { return _stencilState.Peek().StencilEnable; }
      set { _stencilState.Peek().StencilEnable = value; }
    }

    public StencilFunction StencilFunction
    {
      get { return _stencilState.Peek().StencilFunction; }
      set { _stencilState.Peek().StencilFunction = value; }
    }

    public Int32 StencilReference
    {
      get { return _stencilState.Peek().StencilReference; }
      set { _stencilState.Peek().StencilReference = value; }
    }

    public StencilOp StencilPass
    {
      get { return _stencilState.Peek().StencilPass; }
      set { _stencilState.Peek().StencilPass = value; }
    }

    public StencilOp StencilFail
    {
      get { return _stencilState.Peek().StencilFail; }
      set { _stencilState.Peek().StencilFail = value; }
    }

    public StencilOp StencilZFail
    {
      get { return _stencilState.Peek().StencilZFail; }
      set { _stencilState.Peek().StencilZFail = value; }
    }

    public Int32 StencilWriteMask
    {
      get { return _stencilState.Peek().StencilWriteMask; }
      set { _stencilState.Peek().StencilWriteMask = value; }
    }

    public Int32 StencilReadMask
    {
      get { return _stencilState.Peek().StencilReadMask; }
      set { _stencilState.Peek().StencilReadMask = value; }
    }

    public static readonly RenderState Default = new RenderState();

    public RenderState()
    {
      _blendState = new Stack<BlendState>(new BlendState[] { new BlendState() });
      _depthState = new Stack<DepthState>(new DepthState[] { new DepthState() });
      _rasterizerState = new Stack<RasterizerState>(new RasterizerState[] { new RasterizerState() });
      _stencilState = new Stack<StencilState>(new StencilState[] { new StencilState() });
    }

    /// <summary>
    /// Resets the rendering system to the default states, in effect turns off culling, depth writing, blending and stencils.
    /// </summary>
    public static void Reset()
    {
      GL.Disable(EnableCap.CullFace);
      GL.CullFace(CullFaceMode.Back);

      GL.Disable(EnableCap.DepthTest);
      GL.DepthFunc(DepthFunction.Less);
      GL.PolygonOffset(0, 0);

      GL.Disable(EnableCap.Blend);
      GL.BlendFuncSeparate(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One, BlendingFactorDest.Zero);
      GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);

      GL.Disable(EnableCap.StencilTest);
    }

    /// <summary>
    /// Applies the rendering system states.
    /// </summary>
    /// <param name="reset">If true, sets the states back to their default value if the state group is not used, otherwise leaves them as they were.</param>
    public void Apply(GraphicsDevice device)
    {
      _blendState.Peek().Apply(device);
      _depthState.Peek().Apply(device);
      _rasterizerState.Peek().Apply(device);
      _stencilState.Peek().Apply(device);
    }

    public void Push()
    {
      _blendState.Push((BlendState)_blendState.Peek().Clone());
      _depthState.Push((DepthState)_depthState.Peek().Clone());
      _rasterizerState.Push((RasterizerState)_rasterizerState.Peek().Clone());
      _stencilState.Push((StencilState)_stencilState.Peek().Clone());
    }

    public void Pop() 
    {
      _blendState.Pop();
      _depthState.Pop();
      _rasterizerState.Pop();
      _stencilState.Pop();
    }

    public object Clone()
    {
      return new RenderState()
      {
        _blendState = new Stack<Graphics.BlendState>(_blendState.Select(s => (BlendState)s.Clone())),
        _depthState = new Stack<DepthState>(_depthState.Select(s => (DepthState)s.Clone())),
        _rasterizerState = new Stack<RasterizerState>(_rasterizerState.Select(s => (RasterizerState)s.Clone())),
        _stencilState = new Stack<StencilState>(_stencilState.Select(s => (StencilState)s.Clone()))
      };
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_blendState != null)
        {
          foreach (BlendState state in _blendState) state.Dispose();
          _blendState = null;
        }
        if (_depthState != null)
        {
          foreach (DepthState state in _depthState) state.Dispose();
          _depthState = null;
        }
        if (_rasterizerState != null)
        {
          foreach (RasterizerState state in _rasterizerState) state.Dispose();
          _rasterizerState = null;
        }
        if (_stencilState != null)
        {
          foreach (StencilState state in _stencilState) state.Dispose();
          _stencilState = null;
        }
      }
    }
  }
}
