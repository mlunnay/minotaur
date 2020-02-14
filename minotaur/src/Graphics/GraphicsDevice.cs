using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;
using OpenTK.Graphics;

namespace Minotaur.Graphics
{
  public class GraphicsDevice : IUniformSource
  {
    private GameWindow _gameWindow;

    private RenderState _renderState;
    private Viewport _viewport;

    private int _width;
    private int _height;

    private int _maxTextureSlots;
    private int _maxVertexAttributes;
    private int _maxTextureSize;
    private int _maxColorAttachments;
    private int _maxDrawBuffers;

    private ICamera _camera;
    private Matrix4 _viewProjection;

    private VertexAttributeMappings _attributeMappings;
    private UniformMappings _uniformMappings;
    private Color4 _clearColor;

    public RenderState RenderState
    {
      get { return _renderState; }
      set
      {
        _renderState.BlendState = value.BlendState;
        _renderState.DepthState = value.DepthState;
        _renderState.RasterizerState = value.RasterizerState;
        _renderState.StencilState = value.StencilState;
      }
    }

    public Viewport ViewPort
    {
      get { return _viewport; }
      set { _viewport = value; }
    }

    public TextureCollection Textures { get; private set; }

    public int MaxTextureSlots { get { return _maxTextureSlots; } }

    public int MaxVertexAttributes { get { return _maxVertexAttributes; } }

    public int MaxTextureSize { get { return _maxTextureSize; } }

    public int MaxColorAttachments { get { return _maxColorAttachments; } }

    public int MaxDrawBuffers { get { return _maxDrawBuffers; } }

    public int Width { get { return _width; } }

    public int Height { get { return _height; } }

    public ICamera Camera
    {
      get { return _camera; }
      set
      {
        _camera = value;
        _viewProjection = Matrix4.Mult(_camera.View, _camera.Projection);
      }
    }

    public Matrix4 ViewProjection
    {
      get { return _viewProjection; }
    }

    public Matrix4 World { get; set; }

    public VertexAttributeMappings AttributeMappings
    {
      get { return _attributeMappings; }
      set { _attributeMappings = value; }
    }

    public UniformMappings UniformMappings
    {
      get { return _uniformMappings; }
      set { _uniformMappings = value; }
    }

    public GraphicsStatistics Statistics { get; private set; }

    public FPS FPS { get; private set; }

    public DebugBatch DebugBatch { get; private set; }

    public BitmapFont DefaultFont { get; private set; }

    public Color4 ClearColor
    {
      get { return _clearColor; }
      set
      {
        _clearColor = value;
        GL.ClearColor(value);
      }
    }

    public GraphicsDevice(GameWindow window)
    {
      _gameWindow = window;
      _width = window.ClientSize.Width;
      _height = window.ClientSize.Height;

      GL.GetInteger(GetPName.MaxTextureImageUnits, out _maxTextureSlots);
      Utilities.CheckGLError();
      GL.GetInteger(GetPName.MaxVertexAttribs, out _maxVertexAttributes);
      Utilities.CheckGLError();
      GL.GetInteger(GetPName.MaxTextureSize, out _maxTextureSize);
      Utilities.CheckGLError();
      GL.GetInteger(GetPName.MaxColorAttachments, out _maxColorAttachments);
      Utilities.CheckGLError();
      GL.GetInteger(GetPName.MaxDrawBuffers, out _maxDrawBuffers);
      Utilities.CheckGLError();

      Textures = new TextureCollection(_maxTextureSlots);

      _viewProjection = Matrix4.Identity;
      World = Matrix4.Identity;

      _viewport = new Viewport(0, 0, window.ClientSize.Width, window.ClientSize.Height);

      _renderState = new RenderState();
      ClearColor = Color4.Black;

      Statistics = new GraphicsStatistics();
      FPS = new Graphics.FPS();

      DebugBatch = new Graphics.DebugBatch(this);

      ResetDefaultMappings();
    }

    internal void Initialize()
    {
      _renderState.BlendState = BlendState.Opaque;
      _renderState.RasterizerState = RasterizerState.CullCounterClockwise;

      Textures.Clear();
    }

    public void SetViewport(int x, int y, int width, int height)
    {
      _viewport.X = x;
      _viewport.Y = y;
      _viewport.Width = width;
      _viewport.Height = height;
    }

    public object[] GetRenderTargets()
    {
      throw new NotImplementedException();
    }

    public void Display(double td)
    {
      DebugBatch.DrawDebug((float)td);

      FPS.Update(td);
      Statistics.Reset();

      Minotaur.Core.DisposalManager.Process();

      _gameWindow.SwapBuffers();

      _renderState = new Graphics.RenderState();
      _renderState.Apply(this);

      GL.Viewport(0, 0, _width, _height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
    }

    #region Drawing Methods

    public void DrawVertexArray(VertexArray vao, Material material)
    {
      foreach (Pass pass in material.Passes)
      {
        Statistics.DrawCount++;
        Statistics.PrimitiveCount += vao.PrimitiveCount(0);
        Statistics.VertexCount += vao.VertexBuffer.Size;

        pass.Apply(this);

        if (pass.Program.UniformMappings == null)
          pass.Program.UniformMappings = _uniformMappings;
        pass.Program.Bind();
        pass.Program.BindUniformSource(this);
        _attributeMappings.BindAttributes(vao, pass.Program);

        _renderState.Apply(this);

        // TODO: look at doing occolusion based conditional rendering

        vao.Bind();
        if (vao.IndexBuffer != null)
          GL.DrawElements(vao.VertexBuffer.BeginMode, vao.IndexBuffer.Length, vao.IndexBuffer.ElementsType, IntPtr.Zero);
        else
          GL.DrawArrays(vao.VertexBuffer.BeginMode, 0, vao.VertexBuffer.Count);
        vao.Unbind();
      }
    }

    public void DrawVertexArray(VertexArray vao, int start, int end, Material material)
    {
      foreach (Pass pass in material.Passes)
      {
        Statistics.DrawCount++;
        Statistics.PrimitiveCount += vao.PrimitiveCount(start + (end == 0 ? 0 : vao.IndexBuffer.Length - end));
        Statistics.VertexCount += vao.VertexBuffer.Size;

        pass.Apply(this);

        if (pass.Program.UniformMappings == null)
          pass.Program.UniformMappings = _uniformMappings;
        pass.Program.Bind();
        pass.Program.BindUniformSource(this);
        _attributeMappings.BindAttributes(vao, pass.Program);

        pass.Program.BindUniforms(pass.Parameters);

        

        _renderState.Apply(this);

        vao.Bind();
        if (vao.IndexBuffer != null)
          GL.DrawRangeElements(vao.VertexBuffer.BeginMode, start, end == 0 ? vao.IndexBuffer.Length - 1 : end, (end == 0 ? vao.IndexBuffer.Length : end) - start, vao.IndexBuffer.ElementsType, IntPtr.Zero);
        else
          GL.DrawArrays(vao.VertexBuffer.BeginMode, start, end == 0 ? vao.VertexBuffer.Count : end - start);
        vao.Unbind();
      }
    }

    public void DrawVertexArray(VertexArray vao, Program program)
    {
      Statistics.DrawCount++;
      Statistics.PrimitiveCount += vao.PrimitiveCount(0);
      Statistics.VertexCount += vao.VertexBuffer.Size;

      if (program.UniformMappings == null)
        program.UniformMappings = _uniformMappings;
      program.Bind();
      program.BindUniformSource(this);
      _attributeMappings.BindAttributes(vao, program);
      Utilities.CheckGLError();
      _renderState.Apply(this);
      Utilities.CheckGLError();

      vao.Bind();

      if (vao.IndexBuffer != null)
        GL.DrawElements(vao.VertexBuffer.BeginMode, vao.IndexBuffer.Length, vao.IndexBuffer.ElementsType, IntPtr.Zero);
      else
        GL.DrawArrays(vao.VertexBuffer.BeginMode, 0, vao.VertexBuffer.Count);
      vao.Unbind();
      program.Unbind();
    }

    public void DrawVertexArray(VertexArray vao, int start, int end, Program program)
    {
      Statistics.DrawCount++;
      Statistics.PrimitiveCount += vao.PrimitiveCount(start + (end == 0 ? 0 : vao.IndexBuffer.Length - end));
      Statistics.VertexCount += vao.VertexBuffer.Size;

      if (program.UniformMappings == null)
        program.UniformMappings = _uniformMappings;
      program.Bind();
      program.BindUniformSource(this);
      _attributeMappings.BindAttributes(vao, program);

      _renderState.Apply(this);

      vao.Bind();
      if (vao.IndexBuffer != null)
        GL.DrawRangeElements(vao.VertexBuffer.BeginMode, start, end == 0 ? vao.IndexBuffer.Length - 1 : end, (end == 0 ? vao.IndexBuffer.Length : end) - start, vao.IndexBuffer.ElementsType, IntPtr.Zero);
      else
        GL.DrawArrays(vao.VertexBuffer.BeginMode, start, end == 0 ? vao.VertexBuffer.Count : end - start);
      vao.Unbind();
    }

    #endregion

    public void EnsureUniformMappingsForProgram(Program program)
    {
      if (program.UniformMappings == null)
        program.UniformMappings = _uniformMappings;
    }

    public void ResetDefaultMappings()
    {
      _attributeMappings = new VertexAttributeMappings();
      _attributeMappings.Add("Position", VertexUsage.Position);
      _attributeMappings.Add("Normal", VertexUsage.Normal);
      _attributeMappings.Add("Tangent", VertexUsage.Tangent);
      _attributeMappings.Add("TexCoord", VertexUsage.TextureCoordinate);
      _attributeMappings.Add("Color", VertexUsage.Color);

      _uniformMappings = new UniformMappings();
      _uniformMappings.Add("World", UniformUsage.World);
      _uniformMappings.Add("WorldView", UniformUsage.WorldView);
      _uniformMappings.Add("WorldViewProj", UniformUsage.WorldViewProj);
      _uniformMappings.Add("ViewPort", UniformUsage.ViewPort);
      _uniformMappings.Add("NearFar", UniformUsage.NearFar);
    }

    public bool GetUniformValue(UniformUsage usage, out IUniformValue value)
    {
      value = null;
      if (usage == UniformUsage.World)
        value = UniformValue.Create(World);
      else if (usage == UniformUsage.WorldView)
      {
        if (_camera != null)
          value = UniformValue.Create(Matrix4.Mult(World, _camera.View));
        else
          value = UniformValue.Create(World);
      }
      else if (usage == UniformUsage.WorldViewProj)
      {
        if (_camera != null)
          value = UniformValue.Create(Matrix4.Mult(World, _viewProjection));
        else
          value = UniformValue.Create(World);
      }
      else if (usage == UniformUsage.ViewPort)
      {
        if (_camera != null)
          value = UniformValue.Create(_camera.Viewport.GetAsVector4());
        else
          value = UniformValue.Create(new Vector4(0, 0, Width, Height));
      }
      else if (usage == UniformUsage.NearFar)
      {
        if (_camera != null)
          value = UniformValue.Create(new Vector2(_camera.Near, _camera.Far));
        else
          value = UniformValue.Create(new Vector2(1f, 100f));
      }

      return value != null;
    }
  }
}
