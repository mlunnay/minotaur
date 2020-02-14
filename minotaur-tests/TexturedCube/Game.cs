using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Minotaur.Graphics;
using Minotaur.Core;
using System.Collections.Generic;
using System.Drawing;

namespace TexturedCube
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

    string fragmentShaderSource = @"
#version 150

uniform sampler2D tex;

in vec2 fragTexCoord;

out vec4 finalColor;

void main() {
    //note: the texture function was called texture2D in older versions of GLSL
    finalColor = texture(tex, fragTexCoord);
//    finalColor = vec4(fragTexCoord.s, fragTexCoord.t, 0, 1);
//    finalColor = vec4(1, 0, 0, 1);
}";

    VertexPositionTexture[] positionVboData = new VertexPositionTexture[]{
           new VertexPositionTexture(new Vector3(-1.0f, -1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( 1.0f, -1.0f,  1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( 1.0f,  1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f,  1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( 1.0f, -1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( 1.0f,  -1.0f,  -1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f,  1.0f,  -1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f, 1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f,  1.0f,  -1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f,  1.0f,  -1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, 1.0f,  -1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f, 1.0f,  -1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f,  -1.0f,  -1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f,  -1.0f,  -1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(-1.0f, -1.0f,  -1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( -1.0f, 1.0f,  -1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f, 1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(-1.0f,  -1.0f,  1.0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f, -1.0f,  1.0f), new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f, -1.0f,  1.0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3( -1.0f,  -1.0f,  -1.0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3(1.0f,  -1.0f,  -1.0f), new Vector2(0.0f, 1.0f)),
    };

    int[] indicesVboData = new int[]{
      // front face
      0, 1, 2, 2, 3, 0,
      // left face
      4, 5, 6, 6, 7, 4,
      // top face
      8, 9, 10, 10, 11, 8,
      // back face
      12, 13, 14, 14, 15, 12,
      // right face
      16, 17, 18, 18, 19, 16,
      // bottom face
      20, 21, 22, 22, 23, 20,
    };

    Matrix4 projectionMatrix, modelviewMatrix;

    Program shader;

    VertexBuffer positionVbo, normalVbo;
    IndexBuffer indexVbo;
    VertexArray vao;
    Texture2D texture;

    public Game()
      : base(800, 600, GraphicsMode.Default, "TexturedCube", 0,
      DisplayDevice.Default, 3, 0,
      GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      VSync = VSyncMode.On;
      
      GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
      GL.Enable(EnableCap.DepthTest);

      float aspectRatio = ClientSize.Width / (float)(ClientSize.Height);
      Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspectRatio, 1, 100, out projectionMatrix);
      modelviewMatrix = Matrix4.LookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));

      shader = new Program(vertexShaderSource, fragmentShaderSource);
      shader.Bind();
      shader.UniformMatrix4("camera", false, ref projectionMatrix);
      shader.UniformMatrix4("model", false, ref modelviewMatrix);
      shader.Unbind();

      positionVbo = VertexBuffer.Create(VertexFormat.PositionTexture, positionVboData);
      //normalVbo = VertexBuffer.Create(new VertexFormat(new List<VertexAttribute>() { new VertexAttribute(VertexUsage.TextureCoordinate, VertexAttribPointerType.Float, 0, 2) }), textureVboData);
      
      indexVbo = IndexBuffer.Create(indicesVboData);
      vao = new VertexArray(positionVbo, indexVbo);
      Utilities.CheckGLError();
      vao.AddBinding(shader.VertexAttribute("vert").Slot, VertexUsage.Position);
      vao.AddBinding(shader.VertexAttribute("vertTexCoord").Slot, VertexUsage.TextureCoordinate);
      Utilities.CheckGLError();
      Bitmap bmp = new Bitmap(@"..\..\..\Resources\wooden-crate.jpg");
      texture = Texture2D.Create(bmp, TextureMinFilter.Linear, TextureMagFilter.Linear);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
      Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
      shader.Bind();
      //shader.UniformMatrix4("model", false, ref modelviewMatrix);
      shader.BindUniforms(new Dictionary<string, IUniformValue>() {{"model", UniformValue.Create(modelviewMatrix)}});
      shader.Unbind();

      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
      
      vao.Bind();
      shader.Bind();
      shader.Uniform1("tex", 0);
      GL.ActiveTexture(TextureUnit.Texture0);
      texture.Bind();
      indexVbo.Bind();
      GL.DrawElements(BeginMode.Triangles, indexVbo.Length,
          DrawElementsType.UnsignedInt, IntPtr.Zero);

      SwapBuffers();
    }

    protected override void OnUnload(EventArgs e)
    {
      DisposalManager.Process();
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
