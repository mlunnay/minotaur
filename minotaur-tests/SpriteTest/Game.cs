using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Core;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Minotaur.Graphics;
using MinotaurTests.Common;
using OpenTK.Graphics;
using Minotaur.Content;

namespace SpriteTest
{
  class Game : GameWindow
  {
    string vsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord;
void main() {
  fragTexCoord = TexCoord;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

      string fsSrc = @"#version 150
uniform sampler2D Texture;
      
noperspective in vec2 fragTexCoord;
      
out vec4 finalColor;
      
void main() {
    finalColor = texture(Texture, fragTexCoord);
}
";

      string wireframeVsSrc = @"#version 150
uniform mat4 MatrixTransform;
in vec3 Position;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord_;
void main() {
  fragTexCoord_ = TexCoord;
  gl_Position = MatrixTransform * vec4(Position, 1);
}
";

      string wireframeGeomSrc = @"#version 150
noperspective in vec2 fragTexCoord_[3];
uniform vec2 WIN_SCALE;
noperspective out vec3 dist;
noperspective out vec2 fragTexCoord;
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;
void main(void)
{
// taken from 'Single-Pass Wireframe Rendering'
vec2 p0 = WIN_SCALE * gl_PositionIn[0].xy/gl_PositionIn[0].w;
vec2 p1 = WIN_SCALE * gl_PositionIn[1].xy/gl_PositionIn[1].w;
vec2 p2 = WIN_SCALE * gl_PositionIn[2].xy/gl_PositionIn[2].w;
vec2 v0 = p2-p1;
vec2 v1 = p2-p0;
vec2 v2 = p1-p0;
float area = abs(v1.x*v2.y - v1.y * v2.x);

dist = vec3(area/length(v0),0,0);
fragTexCoord = fragTexCoord_[0];
gl_Position = gl_PositionIn[0];
EmitVertex();
dist = vec3(0,area/length(v1),0);
fragTexCoord = fragTexCoord_[1];
gl_Position = gl_PositionIn[1];
EmitVertex();
dist = vec3(0,0,area/length(v2));
fragTexCoord = fragTexCoord_[2];
gl_Position = gl_PositionIn[2];
EmitVertex();
EndPrimitive();
}
";

      string wireframeFsSrc = @"#version 150
//uniform sampler2D Texture;
uniform vec4 Diffuse = vec4(0.1,0.1,0.1,1);
noperspective in vec3 dist;
noperspective in vec2 fragTexCoord;
      
out vec4 finalColor;
      
void main() {
  // determine frag distance to closest edge
float nearD = min(min(dist[0],dist[1]),dist[2]);
float edgeIntensity = exp2(-1.0*nearD*nearD);
    finalColor = edgeIntensity * Diffuse;
}

";

    Matrix4 projectionMatrix, modelviewMatrix, world;

    Program shader, wireframeShader;

    GraphicsDevice _graphicsDevice;

    Texture2D texture;

    SpriteBatch spriteBatch;
    private BitmapFont _font;

    public Game()
      : base(800, 600, GraphicsMode.Default, "Model Test", 0,
      DisplayDevice.Default, 3, 3,
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

      shader = new Program(vsSrc, fsSrc);
      wireframeShader = new Program(new Shader[] {
        new Shader(wireframeFsSrc, ShaderType.FragmentShader),
        new Shader(wireframeGeomSrc, ShaderType.GeometryShader),
        new Shader(wireframeVsSrc, ShaderType.VertexShader)
      });
      System.Drawing.Bitmap bmp = new System.Drawing.Bitmap("whiteButton.png");
      texture = Texture2D.Create(bmp);

      ContentManager content = new ContentManager(new ServiceProvider(), @"c:\devel\Minotaur Tests\Content\");
      content.RegisterTypeReader<BitmapFont>(new SpriteFontReader(Shaders.FontShader));
      _font = content.Get<BitmapFont>(@"SourceCodePro16.meb").Content as BitmapFont;

      //projectionMatrix = Matrix4.CreateOrthographicOffCenter(0, _graphicsDevice.ViewPort.Width, _graphicsDevice.ViewPort.Height, 0, 0, 1);

      world = Matrix4.Scale(0.5f) * Matrix4.CreateTranslation(0, -1.5f, 0);

      _graphicsDevice.Camera = new GuiCamera(_graphicsDevice);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      SlicedSprite buttonSprite = new SlicedSprite(texture, 0, 0, 0, 0, 12, 11, 12, 11);
      //ushort[] indices;
      //VertexPositionColorTexture[] vertices = buttonSprite.GetVertices(200, 200, 300, 300, 0, Color4.White, out indices);
      //VertexBuffer vb = VertexBuffer.Create(vertices[0].VertexFormat, vertices);
      //IndexBuffer ib = IndexBuffer.Create(indices);
      //VertexArray vao = new VertexArray(vb, ib);

      //shader.Bind();
      //Sampler sampler = Sampler.Create(texture);
      //shader.BindUniforms(new Dictionary<string, IUniformValue>() { {"Texture", UniformValue.Create(sampler) }
      //});

      ////_graphicsDevice.RenderState.BlendState = BlendState.AlphaBlend;

      //_graphicsDevice.DrawVertexArray(vao, shader);

      //wireframeShader.Bind();
      //wireframeShader.BindUniforms(new Dictionary<string, IUniformValue>()
      //{
      //  {"MatrixTransform", UniformValue.Create(projectionMatrix)},
      //  {"WIN_SCALE", UniformValue.Create((float)_graphicsDevice.ViewPort.Width, (float)_graphicsDevice.ViewPort.Height)},
      //  {"Diffuse", UniformValue.Create(new Color4(1.0f, 0f, 0f, 1f))}
      //});
      //GL.Disable(EnableCap.DepthTest);
      //_graphicsDevice.DrawVertexArray(vao, wireframeShader);
      //GL.Enable(EnableCap.DepthTest);

      spriteBatch.Begin(SpriteSortMode.Texture, BlendState.NonPremultiplied);
      spriteBatch.Draw(buttonSprite, 100, 100, 100, 100, Color4.White);
      spriteBatch.Draw(buttonSprite, 100, 200, 200, 100, Color4.White);
      spriteBatch.Draw(buttonSprite, 300, 300, 100, 100, Color4.White);
      spriteBatch.End();

      spriteBatch.Begin(SpriteSortMode.Texture, new RenderState() { BlendState = BlendState.AlphaBlend }, null, _font.Shader);
      spriteBatch.DrawString(_font, "Test Text!!", 0, 0);
      spriteBatch.End();
      
      SwapBuffers();
      DisposalManager.Process();
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
