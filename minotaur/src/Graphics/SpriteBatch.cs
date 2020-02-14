using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class SpriteBatch : GraphicsResource
  {
    private SpriteBatcher _batcher;
    private Program _shader;
    private SpriteSortMode _sortMode;
    private RenderState _renderState;
    private Sampler _sampler;
    private Sampler _defaultSampler;
    private bool _beginCalled;
    private static Program _spriteShader;
    private GraphicsDevice _graphicsDevice;
    private Matrix4 _matrix;
    private Dictionary<string, IUniformValue> _shaderUniforms = new Dictionary<string, IUniformValue>();

    static SpriteBatch()
    {
      string vsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
in vec4 Color;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord;
out vec4 fragColor;
void main() {
  fragTexCoord = TexCoord;
  fragColor = Color;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";
      string fsSrc = @"#version 150
uniform sampler2D Texture;
      
noperspective in vec2 fragTexCoord;
in vec4 fragColor;
      
out vec4 finalColor;
      
void main() {
    finalColor = texture(Texture, fragTexCoord) * fragColor;
}
";
      _spriteShader = new Program(vsSrc, fsSrc);
    }

    public SpriteBatch(GraphicsDevice graphicsDevice)
    {
      if(graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      _graphicsDevice = graphicsDevice;

      _defaultSampler = Sampler.Create(wrapS: TextureWrapMode.Clamp, wrapT: TextureWrapMode.Clamp);
      _batcher = new SpriteBatcher(graphicsDevice);
      _beginCalled = false;
    }

    public void Begin(SpriteSortMode sortMode, RenderState renderState, Sampler sampler = null, Program shader = null, Matrix4? transform = null)
    {
      if (_beginCalled)
        throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

      _sortMode = sortMode;
      _renderState = renderState;
      if (shader != null)
        _shader = shader;
      else
        _shader = _spriteShader;
      if(transform.HasValue)
        _matrix = transform.Value;
      else
        _matrix = Matrix4.Identity;
      _sampler = sampler ?? _defaultSampler;

      if (sortMode == SpriteSortMode.Immediate)
        Setup();

      _beginCalled = true;
    }

    public void Begin()
    {
      Begin(SpriteSortMode.Deffered,
        new RenderState() { BlendState = BlendState.AlphaBlend, DepthState = DepthState.None, RasterizerState = RasterizerState.CullClockwise },
        _defaultSampler);
    }

    public void Begin(SpriteSortMode sortMode, BlendState blendState)
    {
      Begin(sortMode,
        new RenderState() { BlendState = blendState, DepthState = DepthState.None, RasterizerState = RasterizerState.CullClockwise },
        _defaultSampler);
    }

    public void End()
    {
      _beginCalled = false;
      if (_sortMode != SpriteSortMode.Immediate)
        Setup();
      _batcher.DrawBatch(_sortMode, _shader, _sampler);
    }

    private void Setup()
    {
      _graphicsDevice.RenderState = _renderState;
      Viewport vp = _graphicsDevice.ViewPort;

      Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, 1);
      projection = Matrix4.Mult(_matrix, projection);

      _shader.Bind();
      _shaderUniforms["MatrixTransform"] = UniformValue.Create(projection);
      _shader.BindUniforms(_shaderUniforms);
    }

    public void Draw(ISprite sprite, int x, int y, int width, int height, Color4 color, float depth = 0f, SpriteEffect effect = SpriteEffect.None)
    {
      SpriteBatchItem item = _batcher.CreateBatchItem();
      item.Sprite = sprite;
      item.Color = color;
      item.X = x;
      item.Y = y;
      item.Width = width;
      item.Height = height;
      item.Depth = depth;
      item.SpriteEffect = effect;

      if (_sortMode == SpriteSortMode.Immediate)
        _batcher.DrawBatch(_sortMode, _shader, _sampler);
    }

    public void Draw(ISprite sprite, int x, int y, Color4 color, float depth = 0f, SpriteEffect effect = SpriteEffect.None)
    {
      Draw(sprite, x, y, sprite.Width, sprite.Height, color, depth, effect);
    }

    public void Draw(ISprite sprite, int x, int y, int width = 0, int height = 0, float depth = 0f, SpriteEffect effect = SpriteEffect.None)
    {
      Draw(sprite, x, y, width == 0 ? sprite.Width : width, height == 0 ? sprite.Height : height, new Color4(1f, 1f, 1f, 1f), depth, effect);
    }

    public void Draw(Texture2D texture, int x, int y, int width, int height, Color4 color, float depth = 0f, SpriteEffect effect = SpriteEffect.None)
    {
      Draw(new Sprite(texture, 0, 0, 0, 0), x, y, width, height, color, depth, effect);
    }

    public void Draw(Texture2D texture, int x, int y, int width = 0, int height = 0, float depth = 0f, SpriteEffect effect = SpriteEffect.None)
    {
      Draw(new Sprite(texture, 0, 0, 0, 0), x, y, width == 0 ? texture.Width : width, height == 0 ? texture.Height : height, new Color4(1f, 1f, 1f, 1f), depth, effect);
    }

    public void DrawString(BitmapFont font, string text, int x, int y, Color4 color, float depth = 0f)
    {
      font.DrawInto(this, text, x, y, 0, color, depth);
    }

    public void DrawString(BitmapFont font, string text, int x, int y, int size,  float depth = 0f)
    {
      font.DrawInto(this, text, x, y, size, Color4.White, depth);
    }

    public void DrawString(BitmapFont font, string text, int x, int y, float depth = 0f)
    {
      font.DrawInto(this, text, x, y, 0, Color4.White, depth);
    }

    private void CheckValue(Texture2D texture)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (!_beginCalled)
        throw new InvalidOperationException("Draw was called, but Begin has not yet been called.");
    }

    private void CheckValue(BitmapFont font, string text)
    {
      if (font == null)
        throw new ArgumentNullException("font");
      if (text == null)
        throw new ArgumentNullException("text");
      if (!_beginCalled)
        throw new InvalidOperationException("Draw was called, but Begin has not yet been called.");
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          if (_shader != null)
          {
            _shader.Dispose();
            _shader = null;
          }
        }
      }
      base.Dispose(disposing);
    }
  }
}
