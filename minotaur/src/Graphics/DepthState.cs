using System;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class DepthState : GraphicsResource, ICloneable
  {
    internal bool _dirty;
    private bool _depthEnabled;
    private bool _depthWrite;
    private DepthFunction _depthFunction;
    private float _depthOffsetFactor;
    private float _depthOffsetUnits;

    public bool DepthEnabled
    {
      get { return _depthEnabled; }
      set { _depthEnabled = value; }
    }

    public bool DepthWrite
    {
      get { return _depthWrite; }
      set
      {
        if (_depthWrite == value)
          return;
        _depthWrite = value;
        _dirty = true;
      }
    }

    public DepthFunction DepthFunction
    {
      get { return _depthFunction; }
      set
      {
        if (_depthFunction == value)
          return;
        _depthFunction = value;
        _dirty = true;
      }
    }

    public float DepthOffsetFactor
    {
      get { return _depthOffsetFactor; }
      set
      {
        if (_depthOffsetFactor == value)
          return;
        _depthOffsetFactor = value;
        _dirty = true;
      }
    }

    public float DepthOffsetUnits
    {
      get { return _depthOffsetUnits; }
      set
      {
        if (_depthOffsetUnits == value)
          return;
        _depthOffsetUnits = value;
        _dirty = true;
      }
    }

    public static readonly DepthState Default;
    public static readonly DepthState DepthRead;
    public static readonly DepthState None;

    public DepthState()
    {
      _depthFunction = OpenTK.Graphics.OpenGL.DepthFunction.Lequal;
      _depthOffsetFactor = 0;
      _depthOffsetUnits = 0;
      _depthWrite = true;
      _depthEnabled = true;
      _dirty = true;
    }

    static DepthState()
    {
      Default = new DepthState()
      {
        _depthEnabled = true,
        _depthWrite = true
      };

      DepthRead = new DepthState()
      {
        _depthEnabled = true,
        _depthWrite = false
      };

      None = new DepthState()
      {
        _depthEnabled = false,
        _depthWrite = false
      };
    }

    public object Clone()
    {
      return new DepthState()
      {
        _depthEnabled = _depthEnabled,
        _depthFunction = _depthFunction,
        _depthOffsetFactor = _depthOffsetFactor,
        _depthOffsetUnits = _depthOffsetUnits,
        _depthWrite = _depthWrite,
        _dirty = _dirty
      };
    }

    public void Apply(GraphicsDevice device)
    {
      if (!_dirty)
        return;
      if (!_depthEnabled)
      {
        GL.Disable(EnableCap.DepthTest);
        Utilities.CheckGLError();
        GL.DepthMask(_depthWrite);
        Utilities.CheckGLError();
      }
      else
      {
        GL.Enable(EnableCap.DepthTest);
        Utilities.CheckGLError();

        GL.DepthMask(_depthWrite);
        Utilities.CheckGLError();
        GL.DepthFunc(DepthFunction);
        Utilities.CheckGLError();
        GL.PolygonOffset(DepthOffsetFactor, DepthOffsetUnits);
        Utilities.CheckGLError();

        GL.DepthRange(0, 1);
      }
      _dirty = false;
    }
  }
}
