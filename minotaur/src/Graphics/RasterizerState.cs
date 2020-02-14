using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class RasterizerState : GraphicsResource, ICloneable
  {
    internal bool _dirty;
    private CullMode _cullMode;
    private PolygonMode _fillMode;
    private float _depthBias;
    private float _slopeScaleDepthBias;
    private bool _scissorTestEnable;

    public static readonly RasterizerState CullClockwise;
    public static readonly RasterizerState CullCounterClockwise;
    public static readonly RasterizerState CullNone;

    public CullMode CullMode
    {
      get { return _cullMode; }
      set
      {
        if (_cullMode == value)
          return;
        _cullMode = value;
        _dirty = true;
      }
    }

    public PolygonMode FillMode
    {
      get { return _fillMode; }
      set
      {
        if (_fillMode == value)
          return;
        _fillMode = value;
        _dirty = true;
      }
    }

    public float DepthBias
    {
      get { return _depthBias; }
      set
      {
        if (_depthBias == value)
          return;
        _depthBias = value;
        _dirty = true;
      }
    }

    public float SlopeScaleDepthBias
    {
      get { return _slopeScaleDepthBias; }
      set
      {
        if (_slopeScaleDepthBias == value)
          return;
        _slopeScaleDepthBias = value;
        _dirty = true;
      }
    }

    public bool ScissorTestEnable
    {
      get { return _scissorTestEnable; }
      set
      {
        if (_scissorTestEnable == value)
          return;
        _scissorTestEnable = value;
        _dirty = true;
      }
    }

    public RasterizerState()
    {
      _cullMode = CullMode.CullClockwiseFace;
      _fillMode = PolygonMode.Fill;
      _depthBias = 0;
      _slopeScaleDepthBias = 0;
      _scissorTestEnable = false;
      _dirty = true;
    }

    static RasterizerState()
    {
      CullClockwise = new RasterizerState()
      {
        CullMode = CullMode.CullClockwiseFace
      };
      CullCounterClockwise = new RasterizerState()
      {
        CullMode = CullMode.CullCounterClockwiseFace
      };
      CullNone = new RasterizerState()
      {
        CullMode = CullMode.None
      };
    }

    internal void Apply(GraphicsDevice device)
    {
      if(!_dirty)
        return;

      // when rendering offscreen the faces change order.
      //bool offscreen = device.GetRenderTargets().Length > 0;
      bool offscreen = false;

      if (_cullMode == CullMode.None)
      {
        GL.Disable(EnableCap.CullFace);
        Utilities.CheckGLError();
      }
      else
      {
        GL.Enable(EnableCap.CullFace);
        Utilities.CheckGLError();
        GL.CullFace(CullFaceMode.Back);
        Utilities.CheckGLError();

        if (_cullMode == Graphics.CullMode.CullClockwiseFace)
        {
          if (offscreen)
            GL.FrontFace(FrontFaceDirection.Cw);
          else
            GL.FrontFace(FrontFaceDirection.Ccw);
          Utilities.CheckGLError();
        }
        else
        {
          if (offscreen)
            GL.FrontFace(FrontFaceDirection.Ccw);
          else
            GL.FrontFace(FrontFaceDirection.Cw);
          Utilities.CheckGLError();
        }
      }

      GL.PolygonMode(MaterialFace.FrontAndBack, _fillMode);

      if (_scissorTestEnable)
        GL.Enable(EnableCap.ScissorTest);
      else
        GL.Disable(EnableCap.ScissorTest);
      Utilities.CheckGLError();

      if (_depthBias != 0 || _slopeScaleDepthBias != 0)
      {
        GL.Enable(EnableCap.PolygonOffsetFill);
        GL.PolygonOffset(_slopeScaleDepthBias, _depthBias);
      }
      else
        GL.Disable(EnableCap.PolygonOffsetFill);
      Utilities.CheckGLError();

      _dirty = false;
    }

    public object Clone()
    {
      return new RasterizerState()
      {
        _cullMode = _cullMode,
        _depthBias = _depthBias,
        _dirty = _dirty,
        _fillMode = _fillMode,
        _scissorTestEnable = _scissorTestEnable,
        _slopeScaleDepthBias = _slopeScaleDepthBias
      };
    }
  }
}
