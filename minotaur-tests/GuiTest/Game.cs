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

namespace GuiTest
{
  class Game : GameWindow
  {
    Matrix4 projectionMatrix, modelviewMatrix, world;

    Program shader, wireframeShader;

    GraphicsDevice _graphicsDevice;

    Texture2D _checkBoxOn;
    Texture2D _checkBoxOff;
    Texture2D _trackSprite;
    Texture2D _thumbSprite;

    SpriteBatch spriteBatch;
    private BitmapFont _font;

    CheckBox _checkBox;
    IGuiComponent _guiRoot;

    public bool Check;
    public float FloatValue;

    FrameBuffer _fbo;
    Texture2D _depthTexture, _screenTexture;

    List<IGuiComponent> debugGuis = new List<IGuiComponent>();

    public Game()
      : base(800, 600, GraphicsMode.Default, "Model Test", 0,
      DisplayDevice.Default, 3, 0,
      GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      _graphicsDevice = new GraphicsDevice(this);
      VSync = VSyncMode.On;
      
      spriteBatch = new SpriteBatch(_graphicsDevice);

      GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
      GL.Enable(EnableCap.DepthTest);

      ContentManager content = new ContentManager(new ServiceProvider(), @"..\..\..\Content\");
      content.RegisterTypeReader<BitmapFont>(new SpriteFontReader(Shaders.FontShader));
      _font = content.Get<BitmapFont>(@"SourceCodePro16.meb").Content as BitmapFont;

      _graphicsDevice.DebugBatch.DefaultFont = _font;

      System.Drawing.Bitmap bmp = new System.Drawing.Bitmap("checkbox16on.png");
      _checkBoxOn = Texture2D.Create(bmp);
      bmp = new System.Drawing.Bitmap("checkbox16off.png");
      _checkBoxOff = Texture2D.Create(bmp);
      bmp = new System.Drawing.Bitmap("slider16.png");
      _trackSprite = Texture2D.Create(bmp);
      bmp = new System.Drawing.Bitmap("sliderknob16.png");
      _thumbSprite = Texture2D.Create(bmp);
      bmp = new System.Drawing.Bitmap("buttons.png");
      Texture2D buttons = Texture2D.Create(bmp);
      bmp = new System.Drawing.Bitmap("disk.png");
      Texture2D disk = Texture2D.Create(bmp);

      //projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, _graphicsDevice.ViewPort.Width, _graphicsDevice.ViewPort.Height, 0, 0, 1);

      world = Matrix4.Scale(0.5f) * Matrix4.CreateTranslation(0, -1.5f, 0);

      _graphicsDevice.Camera = new GuiCamera(_graphicsDevice);
      _graphicsDevice.DebugBatch.ScreenCamera = _graphicsDevice.Camera;
      _graphicsDevice.DebugBatch.WorldCamera = _graphicsDevice.Camera;

      _guiRoot = new GuiComponent(_graphicsDevice.Width, _graphicsDevice.Height);// { Position = new Vector2(100, 100) };
      _checkBox = new CheckBox("Test Check Box", new Binding(this, "Check"), new Sprite(_checkBoxOn, 0, 0, 0, 0), new Sprite(_checkBoxOff, 0, 0, 0, 0), _font);
      Label label = new Label("test label", _font); // { Position = new Vector2(0, 50)};
      Sprite trackSprite = new Sprite(_trackSprite, 0, 0, 0, 0);
      Sprite thumbSprite = new Sprite(_thumbSprite,0,0,0,0);
      FloatValue = 0f;
      Slider slider = new Slider("Test slider: {0:0.00}", new Binding(this, "FloatValue"), 0f, 1f, trackSprite, thumbSprite, _font);

      SlicedSprite buttonNormal = new SlicedSprite(buttons, 0, 0, 30, 30, 2);
      SlicedSprite buttonHover = new SlicedSprite(buttons, 0, 32, 30, 30, 2);
      SlicedSprite buttonPressed = new SlicedSprite(buttons, 0, 64, 30, 30, 2);
      Button button = new Button("Button.", buttonNormal, buttonHover, buttonPressed, new Sprite(disk, 0, 0, 0, 0), _font);
      button.Click += (o, ev) => System.Diagnostics.Debug.WriteLine("Button Pressed!!!!");
      StackPanel hpanel = new StackPanel(Dimension.Vertical);
      Panel panel = new Panel(Color4.Gray, 1f) { Color = new Color4(128,128,128,63), Position = new Vector2(100, 100), Margin = 5 };
      hpanel.AddChild(_checkBox);
      hpanel.AddChild(label);
      hpanel.AddChild(slider);
      hpanel.AddChild(button);
      panel.AddChild(hpanel);
      _guiRoot.AddChild(panel);

      //debugGuis.Add(button);

      Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(OnButtonUp);
      Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(OnButtonDown);
      Mouse.Move += new EventHandler<MouseMoveEventArgs>(OnMouseMove);

      _depthTexture = new Texture2D(800, 600, PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, PixelType.Float);
      _screenTexture = new Texture2D(800, 600);
      //_fbo = new FrameBuffer(_graphicsDevice, new Texture2D[] { _screenTexture }, _depthTexture);

      string vsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
in vec3 Color;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord;
void main() {
  fragTexCoord = TexCoord;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

      string fsSrc = @"#version 150
uniform sampler2D Texture;
uniform vec4 Diffuse = vec4(1);
      
noperspective in vec2 fragTexCoord;
      
out vec4 finalColor;
      
void main() {
    float n = 1.0; // camera z near
  float f = 100.0; // camera z far
  float z = texture(Texture, fragTexCoord).x;
  float d = (2.0 * n) / (f + n - z * (f - n));
    finalColor = vec4(d,d,d,1);
}
";
      wireframeShader = new Program(vsSrc, fsSrc);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    private void OnButtonUp(object sender, MouseButtonEventArgs e)
    {
      _guiRoot.MouseUp(new Vector2(e.X, e.Y), e.Button);
    }

    private void OnButtonDown(object sender, MouseButtonEventArgs e)
    {
      _guiRoot.MouseDown(new Vector2(e.X, e.Y), e.Button);
    }

    private void OnMouseMove(object sender, MouseMoveEventArgs e)
    {
      _guiRoot.MouseMove(new Vector2(e.X, e.Y));
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      //_graphicsDevice.Camera = _graphicsDevice.DebugBatch.ScreenCamera;
      _guiRoot.Render(_graphicsDevice);

      foreach (IGuiComponent i in debugGuis)
      {
        _graphicsDevice.DebugBatch.DrawPrimitive(new Minotaur.Graphics.Primitives.Box(new Vector3(i.AbsolutePosition.X, i.AbsolutePosition.Y, 0), new Vector3(i.AbsolutePosition.X + i.AbsoluteSize.X, i.AbsolutePosition.Y + i.AbsoluteSize.Y, 0)), color: Color4.Red);
      }
      //_graphicsDevice.DebugBatch.DrawPrimitive(new Minotaur.Graphics.Primitives.Box(new Vector3(_checkBox.AbsolutePosition), new Vector3(_checkBox.AbsolutePosition + _checkBox.AbsoluteSize)), CoordinateSpace.Screen, color: Color4.Red);
      //_graphicsDevice.DebugBatch.DrawString("Test", world: Matrix4.CreateTranslation(500, 500, 0));

      //Sprite sprite = new Sprite(_checkBoxOn, 0, 0, 0, 0);
      ////spriteBatch.Begin(SpriteSortMode.Deffered, BlendState.AlphaBlend);
      //spriteBatch.Begin(SpriteSortMode.Immediate, new RenderState() { BlendState = BlendState.AlphaBlend }, null, Shaders.FontShader);
      //spriteBatch.Draw(new Sprite(_font.Texture, 0, 0, 0, 0), 200, 200);
      //spriteBatch.End();

      //_graphicsDevice.DebugBatch.DrawPrimitive(new Minotaur.Graphics.Primitives.Box(
      //  new Vector3(200, 200, 0),
      //  new Vector3(328, 328, 0)),
      //  CoordinateSpace.Screen, color: Color4.Red);

      //_graphicsDevice.DebugBatch.DrawPrimitive(new Minotaur.Graphics.Primitives.Box(
      //  new Vector3(200 + _font.Texture.Width * 0.5625f, 200 + _font.Texture.Height * 0.429275f, 0),
      //  new Vector3(200 + _font.Texture.Width * 0.625f, 200 + _font.Texture.Height * 0.484375f, 0)),
      //  CoordinateSpace.Screen, color: Color4.Red);

      //shader = Shaders.SpriteShader;

      //SlicedSprite buttonSprite = new SlicedSprite(_checkBoxOn, 0, 0, 0, 0, 12, 11, 12, 11);
      //ushort[] indices;
      //VertexPositionColorTexture[] vertices = buttonSprite.GetVertices(100, 100, 200, 200, 0, Color4.White, out indices);
      //VertexBuffer vb = VertexBuffer.Create(vertices[0].VertexFormat, vertices);
      //IndexBuffer ib = IndexBuffer.Create(indices);
      //VertexArray vao = new VertexArray(vb, ib);

      //shader.Bind();
      //Sampler sampler = Sampler.Create(_checkBoxOn);
      //shader.BindUniforms(new Dictionary<string, IUniformValue>() { {"Texture", UniformValue.Create(sampler) }
      //});
      //shader.Unbind();

      ////_graphicsDevice.RenderState.BlendState = BlendState.AlphaBlend;

      //_graphicsDevice.DrawVertexArray(vao, shader);

      //vertices = buttonSprite.GetVertices(300, 300, 200, 200, 0, Color4.White, out indices);
      //vb = VertexBuffer.Create(vertices[0].VertexFormat, vertices);
      //ib = IndexBuffer.Create(indices);
      //vao = new VertexArray(vb, ib);

      //shader.Bind();
      //sampler = Sampler.Create(_checkBoxOff);
      //shader.BindUniforms(new Dictionary<string, IUniformValue>() { {"Texture", UniformValue.Create(sampler) }
      //});
      //shader.Unbind();

      //_graphicsDevice.DrawVertexArray(vao, shader);

      //_depthTexture.Bind();
      //GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 0, 0, 800, 600, 0);

      //wireframeShader.Bind();
      //wireframeShader.BindUniforms(new System.Collections.Generic.Dictionary<string, IUniformValue>()
      //{
      //  {"Texture", UniformValue.Create(Sampler.Create(_depthTexture))}
      //});

      //_graphicsDevice.RenderState = new RenderState() { BlendState = BlendState.Opaque, DepthState = DepthState.None };

      //VertexBuffer vbo = VertexBuffer.Create(VertexFormat.PositionColorTexture, new VertexPositionColorTexture[] {
      //  new VertexPositionColorTexture(new Vector3(0, 0, 0), new Vector4(1), new Vector2(0)),
      //  new VertexPositionColorTexture(new Vector3(0, 600, 0), new Vector4(1), new Vector2(0, 1)),
      //  new VertexPositionColorTexture(new Vector3(800, 0, 0), new Vector4(1), new Vector2(1, 0)),
      //  new VertexPositionColorTexture(new Vector3(800, 600, 0), new Vector4(1), new Vector2(1))
      //}, beginMode: BeginMode.TriangleStrip);

      //VertexArray vao = new VertexArray(vbo, null);

      //_graphicsDevice.DrawVertexArray(vao, wireframeShader);


      _graphicsDevice.DrawFPS();
      _graphicsDevice.DrawStatitics(0, 550);

      _graphicsDevice.Display((float)e.Time);
    }

    protected override void OnUnload(EventArgs e)
    {
      DisposalManager.Process();
    }

    private Vector3 GetTranslation(Matrix4 mat)
    {
      return new Vector3(mat.M14, mat.M24, mat.M34);
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
