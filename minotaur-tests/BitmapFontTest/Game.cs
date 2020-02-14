using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Minotaur.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Minotaur.Content;
using Minotaur.Core;

namespace BitmapFontTest
{
  class Game : GameWindow
  {
    string vertexShaderSource = @"
#version 150

uniform mat4 camera;
uniform mat4 model;

in vec3 vert;
in vec2 vertTexCoord;

out vec2 fragTexCoord;

void main() {
    // Pass the tex coord straight through to the fragment shader
    fragTexCoord = vertTexCoord;
    
    // Apply all matrix transformations to vert
    gl_Position = camera * model * vec4(vert, 1);
}";

    string fragmentShaderSource = @"#version 150
uniform sampler2D tex; //this is the texture
uniform vec4 baseColor; // color to draw text
in vec2 fragTexCoord; //this is the texture coord
out vec4 finalColor; //this is the output color of the pixel

void main(void) {
float distance = texture(tex, fragTexCoord).r;

if(distance >= 0.5) {
  finalColor = baseColor;
}
else if(distance >= 0.485) {
  vec4 col = baseColor;
  col.a = mix(0.49, 0.5, distance);
  finalColor = col;
}
else {
  finalColor = vec4(0.0, 0.0, 0.0, 0.0);
}
//finalColor = vec4(texture(tex, fragTexCoord).rrr, 1.0);
//finalColor = vec4(1.0, 0.0, 0.0, 1.0);
}
";

    string plainVertexShaderSource = @"
#version 150

uniform mat4 camera;
uniform mat4 model;

in vec3 vert;
in vec2 vertTexCoord;

out vec2 fragTexCoord;

void main() {
    // Pass the tex coord straight through to the fragment shader
    fragTexCoord = vertTexCoord;
    
    // Apply all matrix transformations to vert
    gl_Position = camera * model * vec4(vert, 1);
}";

    string plainFragmentShaderSource = @"#version 150
uniform sampler2D tex; //this is the texture
uniform vec4 baseColor; // color to draw text
in vec2 fragTexCoord; //this is the texture coord
out vec4 finalColor; //this is the output color of the pixel

void main(void) {

finalColor = texture(tex, fragTexCoord).rrrr * baseColor;
}
";

    Matrix4 projectionMatrix, modelviewMatrix;
    BitmapFont font;
    Program shader;

    public Game()
      : base(800, 600, GraphicsMode.Default, "BitmapFont Test", 0,
      DisplayDevice.Default, 3, 0,
      GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
      GL.Disable(EnableCap.DepthTest);
      GL.Enable(EnableCap.Blend);
      GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

      //Matrix4.CreateOrthographic(ClientSize.Width, ClientSize.Height, 1, -1, out projectionMatrix);
      Matrix4.CreateOrthographicOffCenter(0, ClientSize.Width, ClientSize.Height, 0, 10, -10, out projectionMatrix); 
      modelviewMatrix = Matrix4.CreateTranslation(0,0,0);
      //modelviewMatrix = Matrix4.LookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
      //shader = new Program(vertexShaderSource, fragmentShaderSource);
      shader = new Program(plainVertexShaderSource, plainFragmentShaderSource);
      shader.Bind();
      shader.UniformMatrix4("camera", false, ref projectionMatrix);
      shader.UniformMatrix4("model", false, ref modelviewMatrix);
      shader.Uniform4("baseColor", new Color4(255, 0, 0, 255));
      shader.Unbind();

      ContentManager content = new ContentManager(new ServiceProvider(), @"C:\devel\Minotaur Tests\Content\");
      content.RegisterTypeReader<BitmapFont>(new SpriteFontReader(shader));
      font = content.Get<BitmapFont>(@"SourceCodePro32.meb").Content as BitmapFont;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit);// | ClearBufferMask.DepthBufferBit);

      DrawString(10, 10, "BitmapFont Test", font, 18);
      DrawString(10, 50, "BitmapFont Test", font, 36);

      SwapBuffers();
    }

    protected void DrawString(int x, int y, string s, BitmapFont font, int size)
    {
      float curX = x;
      float curY = y;

      List<VertexPositionTexture> vertexData = new List<VertexPositionTexture>();
      List<int> indexData = new List<int>();

      float scale = size / (float)font.CharacterSet.RenderedSize;

      int i = 0;
      bool firstCharacter = true;
      foreach (char c in s)
      {
        BitmapCharacter bc = font.CharacterSet.GetCharacterByID((int)c);

        float xOffset = bc.XOffset * scale;
        float yOffset = bc.YOffset * scale;
        float xAdvance = bc.XAdvance * scale;
        float width = bc.Width * scale;
        float height = bc.Height * scale; 

        // create the quads triangles for this character
        // uper left
        //vertexData.Add(new VertexPositionTexture(curX + (bc.XOffset + font.CharacterSet.PaddingLeft )* scale, curY + (font.Ascent - bc.YOffset - font.CharacterSet.PaddingUp) * scale, 0, (float)bc.X / (float)font.CharacterSet.Width, (float)bc.Y / (float)font.CharacterSet.Height));
        vertexData.Add(new VertexPositionTexture(curX + xOffset, curY + yOffset, 0, (float)bc.X / (float)font.CharacterSet.Width, (float)bc.Y / (float)font.CharacterSet.Height));
        // upper right
        //vertexData.Add(new VertexPositionTexture(curX + width, curY + (font.Ascent - bc.YOffset - font.CharacterSet.PaddingUp) * scale, 0, (float)(bc.X + bc.Width) / (float)font.CharacterSet.Width, (float)bc.Y / (float)font.CharacterSet.Height));
        vertexData.Add(new VertexPositionTexture(curX + xOffset + width, curY + yOffset, 0, (float)(bc.X + bc.Width) / (float)font.CharacterSet.Width, (float)bc.Y / (float)font.CharacterSet.Height));
        // lower right
        //vertexData.Add(new VertexPositionTexture(curX + bc.Width * scale + bc.XOffset * scale, curY + (font.Ascent - bc.YOffset + bc.Height) * scale, 0, (float)(bc.X + bc.Width) / (float)font.CharacterSet.Width, (float)(bc.Y + bc.Height) / (float)font.CharacterSet.Height));
        vertexData.Add(new VertexPositionTexture(curX + xOffset + width, curY + yOffset + height, 0, (float)(bc.X + bc.Width) / (float)font.CharacterSet.Width, (float)(bc.Y + bc.Height) / (float)font.CharacterSet.Height));
        // lower left
        //vertexData.Add(new VertexPositionTexture(curX + bc.XOffset * scale, curY + (font.Ascent - bc.YOffset + bc.Height) * scale, 0, (float)bc.X / (float)font.CharacterSet.Width, (float)(bc.Y + bc.Height) / (float)font.CharacterSet.Height));
        vertexData.Add(new VertexPositionTexture(curX + xOffset, curY + yOffset + height, 0, (float)bc.X / (float)font.CharacterSet.Width, (float)(bc.Y + bc.Height) / (float)font.CharacterSet.Height));
        indexData.Add(i);
        indexData.Add(i + 1);
        indexData.Add(i + 2);
        indexData.Add(i + 2);
        indexData.Add(i + 3);
        indexData.Add(i);
        i += 4;

        

        curX += bc.XAdvance * scale;
      }

      VertexBuffer positionVbo = VertexBuffer.Create(VertexFormat.PositionTexture, vertexData.ToArray());
      IndexBuffer indexVbo = IndexBuffer.Create(indexData.ToArray());
      VertexArray vao = new VertexArray(positionVbo, indexVbo);
      vao.AddBinding(shader.VertexAttribute("vert").Slot, VertexUsage.Position);
      vao.AddBinding(shader.VertexAttribute("vertTexCoord").Slot, VertexUsage.TextureCoordinate);
      vao.Bind();
      
      font.Shader.Bind();
      font.Shader.Uniform1("tex", 0);
      GL.ActiveTexture(TextureUnit.Texture0);
      font.Texture.Bind();
      indexVbo.Bind();
      GL.DrawElements(BeginMode.Triangles, indexVbo.Length,
          DrawElementsType.UnsignedInt, IntPtr.Zero);
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
