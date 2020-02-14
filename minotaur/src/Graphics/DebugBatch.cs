using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public enum CoordinateSpace
  {
    World,
    Screen
  }

  internal struct PrimitiveBatchItem
  {
    public IPrimitive Primitive;
    public Color4 Color;
    public Matrix4 World;
    public float LineWidth;
    public bool DepthEnabled;
    public float Duration;
    public CoordinateSpace Space;

    public void LowerDuration(float td)
    {
      Duration -= td;
    }
  }

  internal struct TextBatchItem
  {
    public string Text;
    public BitmapFont Font;
    public int Size;
    public Matrix4 World;
    public Color4 Color;
    public bool DepthEnabled;
    public float Duration;
    public CoordinateSpace Space;

    public void LowerDuration(float td)
    {
      Duration -= td;
    }
  }

  public class DebugBatch : GraphicsResource
  {
    private GraphicsDevice _graphicsDevice;

    private BitmapFont _defaultFont;
    private ICamera _worldCamera;
    private ICamera _screenCamera;

    private SpriteBatch _spriteBatch;

    private static Program _primitiveShader;
    private Program _currentShader;

    private List<PrimitiveBatchItem> _primitiveBatches = new List<PrimitiveBatchItem>();
    private List<TextBatchItem> _textBatches = new List<TextBatchItem>();

    private BeginMode _lastBeginMode;
    private VertexPositionColor[] _primitiveVertices;
    private VertexPositionColorTexture[] _textVertices;
    private ushort[] _indices;
    private int _verticesIndex = 0;
    private int _indicesIndex = 0;
    private VertexArray _primitiveVao;
    private VertexArray _textVao;
    private Texture2D _lastTexture;

    public readonly static int PrimitiveRestarstIndex = ushort.MaxValue;

    public BitmapFont DefaultFont
    {
      get { return _defaultFont; }
      set { _defaultFont = value; }
    }

    public ICamera WorldCamera
    {
      get { return _worldCamera; }
      set { _worldCamera = value; }
    }

    public ICamera ScreenCamera
    {
      get { return _screenCamera; }
      set { _screenCamera = value; }
    }

    static DebugBatch()
    {
      string vsSrc = @"#version 150
in vec3 Position;
in vec4 Color;
uniform mat4 WorldViewProj;
out vec4 fragColor;
void main()
{
  gl_Position = WorldViewProj * vec4(Position, 1);
  fragColor = Color;
}
";
      string fsSrc = @"#version 150
in vec4 fragColor;
out vec4 finalColor;
void main()
{
  finalColor = fragColor;
}
";
      _primitiveShader = new Program(vsSrc, fsSrc);
    }

    public DebugBatch(GraphicsDevice graphicsDevice)
    {
      _graphicsDevice = graphicsDevice;
      _spriteBatch = new SpriteBatch(graphicsDevice);

      _primitiveVertices = new VertexPositionColor[ushort.MaxValue];
      _indices = new ushort[ushort.MaxValue];
    }

    public void DrawPrimitive(IPrimitive primitive,
      CoordinateSpace coordinateSpace = CoordinateSpace.World,
      Matrix4? world = null,
      bool depthEnabled = false,
      float lineWidth = 1.0f,
      Color4? color = null,
      float duration = 0f)
    {
      _primitiveBatches.Add(new PrimitiveBatchItem()
      {
        Primitive = primitive,
        Space = coordinateSpace,
        World = world.HasValue ? world.Value : Matrix4.Identity,
        DepthEnabled = depthEnabled,
        LineWidth = lineWidth,
        Color = color.HasValue ? color.Value : Color4.White,
        Duration = duration
      });
    }

    public void DrawString(string text,
      CoordinateSpace space = CoordinateSpace.Screen,
      BitmapFont font = null,
      int size = 0,
      Matrix4? world = null,
      Color4? color = null,
      bool depthEnabled = false,
      float duration = 0f)
    {
      _textBatches.Add(new TextBatchItem()
      {
        Text = text,
        Font = font ?? _defaultFont,
        Size = size,
        World = world.HasValue ? world.Value : Matrix4.Identity,
        Color = color.HasValue ? color.Value : Color4.White,
        DepthEnabled = depthEnabled,
        Space = space,
        Duration = duration
      });
    }

    public void DrawBillboardString(string text,
      Vector3 position,
      BitmapFont font = null,
      int size = 0,
      Matrix4? world = null,
      Color4? color = null,
      bool depthEnabled = false,
      float duration = 0f)
    {
      // draw a string at a 3d position in world space.
      if (world.HasValue)
        position = Vector3.Transform(position, world.Value);

      //position = Vector3.Transform(position, _worldCamera.Matrix);
      Vector4 pv4 = Vector4.Transform(new Vector4(position, 1f), _graphicsDevice.ViewProjection);
      position = new Vector3(pv4 / pv4.W);  // convert from clip space to screen space
      //position = Vector3.Transform(position, _graphicsDevice.ViewProjection);
      position.X = _screenCamera.Viewport.Width * (position.X * 0.5f + 0.5f);
      position.Y = _screenCamera.Viewport.Height * (1 - (position.Y * 0.5f + 0.5f));  // make y = 0 at top
      position.Z = (1f - (position.Z / (_worldCamera.Far - _worldCamera.Near))) * 2.0f - 1;

      // text is bellow the point and to the right of the point, make sure it can fit on screen, move text relative to the point.
      Vector2 stringSize = (font ?? _defaultFont).MeasureString(text);

      if (position.X < _screenCamera.Viewport.X ||
        position.Y < _screenCamera.Viewport.Y ||
        position.X - stringSize.X > _screenCamera.Viewport.X + _screenCamera.Viewport.Width ||
        position.Y - stringSize.Y > _screenCamera.Viewport.Y + _screenCamera.Viewport.Height)
      {
        return;
      }

      if (position.X + stringSize.X > _screenCamera.Viewport.X + _screenCamera.Viewport.Width) // put it to the left of the point
        position.X -= stringSize.X;
      if (position.Y + stringSize.Y > _screenCamera.Viewport.Y + _screenCamera.Viewport.Height) // put it above the point
        position.Y -= stringSize.Y;


      _textBatches.Add(new TextBatchItem()
      {
        Text = text,
        Font = font ?? _defaultFont,
        Size = size,
        World = Matrix4.CreateTranslation(position),
        Color = color.HasValue ? color.Value : Color4.White,
        DepthEnabled = depthEnabled,
        Space = CoordinateSpace.Screen,
        Duration = duration
      });
    }

    public void DrawDebug(float td)
    {
      if (_worldCamera == null)
        throw new InvalidOperationException("WorldCamera must be set prior to calling DrawDebug.");
      if (_screenCamera == null)
        throw new InvalidOperationException("WorldCamera must be set prior to calling DrawDebug.");

      GL.PrimitiveRestartIndex(PrimitiveRestarstIndex);

      RenderState originalState = (RenderState)_graphicsDevice.RenderState.Clone();

      PrimitiveBatchItem lastPrimitiveItem = new PrimitiveBatchItem();
      bool first = true;
      
      _graphicsDevice.RenderState = new RenderState() { BlendState = BlendState.NonPremultiplied };
      _currentShader = _primitiveShader;
      _graphicsDevice.Camera = _worldCamera;
      foreach (PrimitiveBatchItem item in _primitiveBatches.Where(p => p.Space == CoordinateSpace.World)
        .OrderBy(p => p.Primitive.BeginMode)
        .ThenBy(p => p.DepthEnabled)
        .ThenBy(p => p.LineWidth))
      {
        if (first || item.DepthEnabled != lastPrimitiveItem.DepthEnabled)
          _graphicsDevice.RenderState.DepthState = item.DepthEnabled ? DepthState.DepthRead : DepthState.None;
        if (first || item.Primitive.BeginMode != lastPrimitiveItem.Primitive.BeginMode)
          FlushPrimitives(item.Primitive.BeginMode);
        if (first || item.LineWidth != lastPrimitiveItem.LineWidth)
          GL.LineWidth(item.LineWidth);
        first = false;

        ushort[] indices;
        VertexPositionColor[] vertices = item.Primitive.GetVertices(_graphicsDevice, item.World, item.Color, out indices);
        AddPrimitive(vertices, indices);

        lastPrimitiveItem = item;
        item.LowerDuration(td);
      }
      FlushPrimitives(_lastBeginMode, true);

      _graphicsDevice.Camera = _screenCamera;
      first = true;
      foreach (PrimitiveBatchItem item in _primitiveBatches.Where(p => p.Space == CoordinateSpace.Screen)
        .OrderBy(p => p.Primitive.BeginMode)
        .ThenBy(p => p.DepthEnabled)
        .ThenBy(p => p.LineWidth))
      {
        if (first || item.DepthEnabled != lastPrimitiveItem.DepthEnabled)
          _graphicsDevice.RenderState.DepthState = item.DepthEnabled ? DepthState.DepthRead : DepthState.None;
        if (first || item.Primitive.BeginMode != lastPrimitiveItem.Primitive.BeginMode)
          FlushPrimitives(item.Primitive.BeginMode);
        if (first || item.LineWidth != lastPrimitiveItem.LineWidth)
          GL.LineWidth(item.LineWidth);
        first = false;
        Utilities.CheckGLError();
        ushort[] indices;
        VertexPositionColor[] vertices = item.Primitive.GetVertices(_graphicsDevice ,item.World, item.Color, out indices);
        AddPrimitive(vertices, indices);

        lastPrimitiveItem = item;
        item.LowerDuration(td);
      }
      FlushPrimitives(_lastBeginMode, true);

      // remove all batches who's time has expired.
      _primitiveBatches.RemoveAll(p => p.Duration <= 0f);

      _graphicsDevice.Camera = _worldCamera;
      first = true;
      TextBatchItem lastTextItem = new TextBatchItem();
      BitmapFont currentFont = _defaultFont;
      foreach (TextBatchItem item in _textBatches.Where(p => p.Space == CoordinateSpace.World)
        .OrderBy(p => p.Font)
        .ThenBy(p => p.DepthEnabled))
      {
        if (first || item.DepthEnabled != lastTextItem.DepthEnabled)
          _graphicsDevice.RenderState.DepthState = item.DepthEnabled ? DepthState.DepthRead : DepthState.None;
        first = false;

        ushort[] indices;
        VertexPositionColorTexture[] vertices = item.Font.Build(item.Text, item.World, item.Size, item.Color, out indices);
        DrawTextInternal(item.Font, vertices, indices);
        item.LowerDuration(td);
      }

      _graphicsDevice.Camera = _screenCamera;
      foreach (TextBatchItem item in _textBatches.Where(p => p.Space == CoordinateSpace.Screen)
        .OrderBy(p => p.Font)
        .ThenBy(p => p.DepthEnabled))
      {
        if (first || item.DepthEnabled != lastTextItem.DepthEnabled)
          _graphicsDevice.RenderState.DepthState = item.DepthEnabled ? DepthState.DepthRead : DepthState.None;
        first = false;

        ushort[] indices;
        VertexPositionColorTexture[] vertices = item.Font.Build(item.Text, item.World, item.Size, item.Color, out indices);
        DrawTextInternal(item.Font, vertices, indices);
        item.LowerDuration(td);
      }

      _lastTexture = null;
      _textBatches.RemoveAll(t => t.Duration <= 0f);
      _graphicsDevice.RenderState = originalState;
    }

    /// <summary>
    /// Calculates the points for a constraned billboard
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="width"></param>
    /// <param name="aa"></param>
    /// <param name="ab"></param>
    /// <param name="ba"></param>
    /// <param name="bb"></param>
    public static void CreateBillboard(ICamera camera,
      Vector3 start,
      Vector3 end,
      float width,
      out Vector3 aa,
      out Vector3 ab,
      out Vector3 ba,
      out Vector3 bb)
    {
      // Compute billboard facing
      Vector3 camright = new Vector3(camera.Matrix.Row0);
      Vector3 up = new Vector3(camera.Matrix.Row1);

      Vector3 right = Vector3.Cross(end - start, Vector3.Cross(camright, up));

      right.Normalize();
      right *= width / 2;

      // Compute destination vertices
      aa = end - right;
      ab = end + right;
      ba = start - right;
      bb = start + right;
    }

    private void FlushPrimitives(BeginMode NewBeginMode, bool last = false)
    {
      if(_verticesIndex != 0)
      {
        _primitiveVao.VertexBuffer.SetData(_primitiveVertices, 0, _verticesIndex, true);
        _primitiveVao.IndexBuffer.SetData(_indices, 0, _indicesIndex, true);
        _graphicsDevice.DrawVertexArray(_primitiveVao, _currentShader);
      }

      if (!last || NewBeginMode != _lastBeginMode)
      {
        if (_primitiveVao != null)
          _primitiveVao.Dispose();
        VertexBuffer vbo = new VertexBuffer(VertexFormat.PositionColor, 0, BufferUsageHint.StreamDraw, NewBeginMode);
        IndexBuffer ibo = new IndexBuffer(BufferUsageHint.StreamDraw, DrawElementsType.UnsignedShort, 0);
        _primitiveVao = new VertexArray(vbo, ibo);
        _lastBeginMode = NewBeginMode;
      }
      
      _indicesIndex = _verticesIndex = 0;
    }

    private void AddPrimitive(VertexPositionColor[] vertices, ushort[] indices)
    {
      if (_verticesIndex + vertices.Length >= ushort.MaxValue)
        FlushPrimitives(_lastBeginMode);

      Array.Copy(vertices, 0, _primitiveVertices, _verticesIndex, vertices.Length);
      for (int i = 0; i < indices.Length; i++)
      {
        _indices[_indicesIndex++] = (ushort)(indices[i] + _verticesIndex);
      }
      _verticesIndex += vertices.Length;
    }

    private void DrawTextInternal(BitmapFont font, VertexPositionColorTexture[] vertices, ushort[] indices)
    {
      if (_textVao == null)
      {
        VertexBuffer vbo = new VertexBuffer(VertexFormat.PositionColorTexture, ushort.MaxValue, BufferUsageHint.StreamDraw, BeginMode.Triangles);
        IndexBuffer ibo = new IndexBuffer(BufferUsageHint.StreamDraw, DrawElementsType.UnsignedShort, 0);
        _textVao = new VertexArray(vbo, ibo);
      }

      Program shader = font.Shader;

      if (_lastTexture == null || font.Texture != _lastTexture)
      {
        Sampler sampler = (Sampler)Sampler.Create().Clone();
        sampler.Texture = font.Texture;
        shader.Bind();
        shader.BindUniforms(new Dictionary<string, IUniformValue>()
        {
          {"Texture", UniformValue.Create(sampler)}
        });
        
        _lastTexture = font.Texture;
      }

      _textVao.VertexBuffer.SetData(vertices, true);
      _textVao.IndexBuffer.SetData(indices, true);
      _graphicsDevice.World = Matrix4.Identity;
      _graphicsDevice.DrawVertexArray(_textVao, shader);
    }
  }
}