using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Minotaur.Graphics;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using MinotaurTests.Common;
using Minotaur.Content;
using Minotaur.Core;
using Minotaur.Graphics.Primitives;

namespace PrimitivesTest
{
  class Game : GameWindow
  {
    GraphicsDevice _graphicsDevice;

    BitmapFont _font;

    ICamera _3DCamera, _screenCamera;

    Model _dwarf;

    public Game()
      : base(800, 600, GraphicsMode.Default, "Primitives Test", 0,
      DisplayDevice.Default, 3, 3,
      GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      GL.Enable(EnableCap.DepthTest);
      _graphicsDevice = new GraphicsDevice(this);
      VSync = VSyncMode.On;

      _graphicsDevice.ClearColor = new Color4(0.1f, 0.2f, 0.5f, 1.0f);

      _3DCamera = new LookAtCamera(_graphicsDevice, new Vector3(0, 5, 10), Vector3.UnitY, Vector3.UnitZ);
      _screenCamera = new GuiCamera(_graphicsDevice);

      _graphicsDevice.Camera = _3DCamera;
      _graphicsDevice.DebugBatch.WorldCamera = _3DCamera;
      _graphicsDevice.DebugBatch.ScreenCamera = _screenCamera;

      ContentManager content = new ContentManager(new ServiceProvider(), @"..\..\..\Content\");
      content.RegisterTypeReader<BitmapFont>(new SpriteFontReader(Shaders.FontShader));
      content.RegisterTypeReader<Model>(new ModelReader());
      _font = content.Get<BitmapFont>(@"SourceCodePro16.meb").Content as BitmapFont;
      _graphicsDevice.DebugBatch.DefaultFont = _font;
      _dwarf = content.Get<Model>("dwarf1.meb").Content as Model;
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      _graphicsDevice.Camera = _3DCamera;
      _graphicsDevice.DebugBatch.DrawPrimitive(new Grid(1, 30, 30), color: new Color4(0.25f, 0.25f, 0.25f, 1f));
      _graphicsDevice.DebugBatch.DrawPrimitive(new Arrow(new Vector3(-10, 0, -10), new Vector3(-10, 1, -10), 0, new Color4(1f, 0f, 0f, 1f)));
      _graphicsDevice.DebugBatch.DrawBillboardString("Arrow (-10, 0, -10)", new Vector3(-10, 0, -10));
      _graphicsDevice.DebugBatch.DrawPrimitive(new Sphere(1), color: new Color4(0f, 0f, 1f, 1f), world: Matrix4.CreateTranslation(-7, 0, -10));
      _graphicsDevice.DebugBatch.DrawPrimitive(new AABox(new Vector3(-4, 1, -10), new Vector3(-3, 0, -11)), color: new Color4(0f, 1f, 0f, 1f));
      _graphicsDevice.DebugBatch.DrawPrimitive(new Axis());
      _graphicsDevice.DebugBatch.DrawPrimitive(new Axis(), world: Matrix4.CreateRotationY((float)Math.PI / 4) * Matrix4.CreateTranslation(2, 0, -10));

      _graphicsDevice.DebugBatch.DrawPrimitive(new AABox(new Vector3(-4, 1, -5), new Vector3(-3, 0, -4), true), color: new Color4(0f, 1f, 0f, 1f));
      _graphicsDevice.DebugBatch.DrawPrimitive(new Sphere(1, solid: true), color: new Color4(0f, 0f, 1f, 1f), world: Matrix4.CreateTranslation(-7, 0, -5));
      _graphicsDevice.DebugBatch.DrawPrimitive(new Minotaur.Graphics.Primitives.Skeleton(_dwarf.Skeleton, ignoreBones: new List<string>() { "$dummy_root", "base", "cam" }), world: Matrix4.Scale(0.25f) * Matrix4.CreateRotationY((float)Math.PI) * Matrix4.CreateTranslation(1, 0, -5));

      

      Vector2 size = _font.MeasureString("Debug String.");
      //size.X /= _graphicsDevice.Width;
      //size.Y /= _graphicsDevice.Height;
      _graphicsDevice.DebugBatch.DrawString("Debug String.", CoordinateSpace.World,
        world: Matrix4.CreateTranslation(-size.X / 2, -size.Y / 2, 0) * Matrix4.CreateRotationX((float)Math.PI) * Matrix4.Scale(1f / 32) *  Matrix4.CreateTranslation(size.X / 64, size.Y / 64, 0) * Matrix4.CreateTranslation(-2, 0, -10));

      _graphicsDevice.DrawFPS();
      
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
