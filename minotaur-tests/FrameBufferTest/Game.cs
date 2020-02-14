using System;
using Minotaur.Content;
using Minotaur.Core;
using Minotaur.Graphics;
using MinotaurTests.Common;
using MinotaurTests.Common.GUI;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Collections.Generic;
using Minotaur.Graphics.Primitives;

namespace FrameBufferTest
{
  class Game : GameWindow
  {
    GraphicsDevice _graphicsDevice;
    FrameBuffer _fbo;

    Texture2D _depthTexture;
    Texture2D _diffuseTexture;
    Texture2D _normalTexture;
    Texture2D _positionTexture;

    Program _defferedShader;

    Model _dwarf;

    VertexArray _fullScreenQuad;

    BitmapFont _font;

    ICamera _3DCamera, _screenCamera;

    SpriteBatch _spriteBatch;

    Matrix4 _dwarfWorld;

    public Game()
      : base(800, 600, GraphicsMode.Default, "FrameBuffer Test", 0,
      DisplayDevice.Default, 3, 3,
      GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      //WindowState = OpenTK.WindowState.Fullscreen;
      //WindowBorder = OpenTK.WindowBorder.Hidden;
      GL.Enable(EnableCap.DepthTest);
      _graphicsDevice = new GraphicsDevice(this);
      VSync = VSyncMode.On;

      _graphicsDevice.ClearColor = new Color4(0.1f, 0.2f, 0.5f, 1.0f);

      _3DCamera = new LookAtCamera(_graphicsDevice, new Vector3(0, 0, -5), Vector3.UnitY, Vector3.UnitZ);
      _screenCamera = new GuiCamera(_graphicsDevice);

      _graphicsDevice.Camera = _3DCamera;
      _graphicsDevice.DebugBatch.WorldCamera = _3DCamera;
      _graphicsDevice.DebugBatch.ScreenCamera = _screenCamera;

      ContentManager content = new ContentManager(new ServiceProvider(), @"..\..\..\Content\");
      content.RegisterTypeReader<BitmapFont>(new SpriteFontReader(Shaders.FontShader));
      content.RegisterTypeReader<Texture2D>(new Texture2DReader());
      content.RegisterTypeReader<Model>(new ModelReader());
      _font = content.Get<BitmapFont>(@"SourceCodePro16.meb").Content as BitmapFont;
      _graphicsDevice.DebugBatch.DefaultFont = _font;

      string vsSrc = @"#version 330
in vec3 Position;
in vec3 Normal;
in vec2 TexCoord;
uniform mat4 WorldViewProj;
out vec2 fragTexCoord;
out vec3 fragNormal;
out vec3 position;
void main() {
  fragTexCoord = TexCoord;
  fragNormal = Normal;
  position = (WorldViewProj * vec4(Position, 1)).xyz;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

      string fsSrc = @"#version 330
#extension GL_ARB_explicit_attrib_location : require
uniform sampler2D Texture;
in vec2 fragTexCoord;
in vec3 fragNormal;
in vec3 position;
layout(location = 0) out vec4 diffuseOutput;
layout(location = 1) out vec4 posOutput;
layout(location = 2) out vec4 normOutput;

void main() {
  diffuseOutput = texture(Texture, fragTexCoord);
  posOutput.xyz = position;
  normOutput = vec4(fragNormal * 0.5 + 0.5, 1);
}
";
      _defferedShader = new Program(vsSrc, fsSrc);

      _depthTexture = Texture2D.CreateEmpty(_graphicsDevice.Width, _graphicsDevice.Height, PixelInternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.UnsignedInt);
      _diffuseTexture = Texture2D.CreateEmpty(_graphicsDevice.Width, _graphicsDevice.Height);
      _normalTexture = Texture2D.CreateEmpty(_graphicsDevice.Width, _graphicsDevice.Height);
      _positionTexture = Texture2D.CreateEmpty(_graphicsDevice.Width, _graphicsDevice.Height);

      _fbo = new FrameBuffer(_graphicsDevice);
      _fbo.Attach(_diffuseTexture, FramebufferAttachment.ColorAttachment0);
      _fbo.Attach(_positionTexture, FramebufferAttachment.ColorAttachment1);
      _fbo.Attach(_normalTexture, FramebufferAttachment.ColorAttachment2);
      _fbo.Attach(_depthTexture, FramebufferAttachment.DepthAttachment);
      _fbo.Unbind();

      Utilities.CheckGLError();

      _spriteBatch = new SpriteBatch(_graphicsDevice);

      _dwarf = content.Get<Model>("\\dwarf1.meb").Content as Model;
      _dwarfWorld = Matrix4.Scale(0.5f) * Matrix4.CreateTranslation(0, -1.5f, 0);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      _dwarfWorld = Matrix4.Mult(Matrix4.CreateRotationY((float)e.Time), _dwarfWorld);

      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      _fbo.BindDraw();
      //Utilities.CheckGLError();
      
      GL.Viewport(0, 0, _fbo.Width, _fbo.Height);
      
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      _graphicsDevice.World = _dwarfWorld;
      _graphicsDevice.Camera = _3DCamera;
      
      foreach (ModelMesh mesh in _dwarf.Meshes)
      {
        foreach (ModelMeshPart part in mesh.MeshParts)
        {
          _defferedShader.Bind();
          _defferedShader.BindUniforms(part.Material.Passes[0].Parameters);
          _graphicsDevice.DrawVertexArray(part.VertexArray, _defferedShader);
        }
      }
      
      _fbo.Unbind();

      _graphicsDevice.Camera = _screenCamera;
      _spriteBatch.Begin();
      _spriteBatch.Draw(_diffuseTexture, 0, 0, _graphicsDevice.Width / 2, _graphicsDevice.Height / 2, effect: SpriteEffect.FlipVertically);
      _spriteBatch.Draw(_positionTexture, _graphicsDevice.Width / 2, 0, _graphicsDevice.Width / 2, _graphicsDevice.Height / 2, effect: SpriteEffect.FlipVertically);
      _spriteBatch.Draw(_normalTexture, 0, _graphicsDevice.Height / 2, _graphicsDevice.Width / 2, _graphicsDevice.Height / 2, effect: SpriteEffect.FlipVertically);
      _spriteBatch.Draw(_depthTexture, _graphicsDevice.Width / 2, _graphicsDevice.Height / 2, _graphicsDevice.Width / 2, _graphicsDevice.Height / 2, effect: SpriteEffect.FlipVertically);
      _spriteBatch.End();

      

      _graphicsDevice.Camera = _3DCamera;
      _graphicsDevice.World = Matrix4.Identity;

      _graphicsDevice.DrawFPS();
      _graphicsDevice.DrawStatitics(600, 0);

      _graphicsDevice.Display(e.Time);
    }

    [STAThread]
    static void Main(string[] args)
    {
      using (Game game = new Game())
      {
        game.Run(30.0);
      }
    }
  }
}
