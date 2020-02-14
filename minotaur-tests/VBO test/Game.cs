using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Minotaur.Graphics;
using Minotaur.Core;
using System.Collections.Generic;

namespace VBO_test
{
  class Game : GameWindow
  {
    string vertexShaderSource = @"
#version 130

precision highp float;

uniform mat4 projection_matrix;
uniform mat4 modelview_matrix;

in vec3 in_position;
in vec3 in_normal;

out vec3 normal;

void main(void)
{
  //works only for orthogonal modelview
  normal = (modelview_matrix * vec4(in_normal, 0)).xyz;
  
  gl_Position = projection_matrix * modelview_matrix * vec4(in_position, 1);
}";

    string fragmentShaderSource = @"
#version 130

precision highp float;

const vec3 ambient = vec3(0.1, 0.1, 0.1);
const vec3 lightVecNormalized = normalize(vec3(0.5, 0.5, 2.0));
const vec3 lightColor = vec3(0.9, 0.9, 0.7);

in vec3 normal;

out vec4 out_frag_color;

void main(void)
{
  float diffuse = clamp(dot(lightVecNormalized, normalize(normal)), 0.0, 1.0);
  out_frag_color = vec4(ambient + diffuse * lightColor, 1.0);
}";

    Vector3[] positionVboData = new Vector3[]{
            new Vector3(-1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f, -1.0f,  1.0f),
            new Vector3( 1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f,  1.0f,  1.0f),
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3( 1.0f, -1.0f, -1.0f), 
            new Vector3( 1.0f,  1.0f, -1.0f),
            new Vector3(-1.0f,  1.0f, -1.0f) };

    int[] indicesVboData = new int[]{
             // front face
                0, 1, 2, 2, 3, 0,
                // top face
                3, 2, 6, 6, 7, 3,
                // back face
                7, 6, 5, 5, 4, 7,
                // left face
                4, 0, 3, 3, 7, 4,
                // bottom face
                0, 1, 5, 5, 4, 0,
                // right face
                1, 5, 6, 6, 2, 1, };

    Matrix4 projectionMatrix, modelviewMatrix;

    Program shader;

    VertexBuffer positionVbo, normalVbo;
    IndexBuffer indexVbo;
    VertexArray vao;

    public Game()
      : base(800, 600, GraphicsMode.Default, "VBO test", 0,
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
      shader.UniformMatrix4("projection_matrix", false, ref projectionMatrix);
      shader.UniformMatrix4("modelview_matrix", false, ref modelviewMatrix);
      shader.Unbind();

      positionVbo = VertexBuffer.Create(new VertexFormat(new List<VertexAttribute>() { new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3)}), positionVboData);
      normalVbo = VertexBuffer.Create(new VertexFormat(new List<VertexAttribute>() { new VertexAttribute(VertexUsage.Normal, VertexAttribPointerType.Float, 0, 3) }), positionVboData);
      indexVbo = IndexBuffer.Create(indicesVboData);
      vao = new VertexArray(positionVbo, indexVbo);
      Utilities.CheckGLError();
      vao.AddBinding(shader.VertexAttribute("in_position").Slot, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      vao.AddBinding(shader.VertexAttribute("in_normal").Slot, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
      Utilities.CheckGLError();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
      Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
      shader.Bind();
      shader.UniformMatrix4("modelview_matrix", false, ref modelviewMatrix);
      shader.Unbind();

      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      shader.Bind();
      vao.Bind();
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
