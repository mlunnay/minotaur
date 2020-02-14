using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Minotaur.Graphics;
using Minotaur.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using Minotaur.Content;
using MinotaurTests.Common;

namespace ModelTest
{
  class Game : GameWindow
  {
    string vertexShaderSource = @"
#version 150

//uniform mat4 camera;
//uniform mat4 model;
uniform mat4 WorldViewProj;

//in vec3 vert;
//in vec2 vertTexCoord;
in vec3 Position;
in vec2 TexCoord;

out vec2 fragTexCoord;

void main() {
    // Pass the tex coord straight through to the fragment shader
    //fragTexCoord = vertTexCoord;
    fragTexCoord = TexCoord;
    
    // Apply all matrix transformations to vert
    //gl_Position = camera * model * vec4(vert, 1);
    gl_Position = WorldViewProj * vec4(Position, 1);
}";

    string fragmentShaderSource = @"
#version 150

uniform sampler2D Texture;
uniform vec4 Diffuse;

in vec2 fragTexCoord;

out vec4 finalColor;

void main() {
    //note: the texture function was called texture2D in older versions of GLSL
    finalColor = texture(Texture, fragTexCoord);
//    finalColor = vec4(fragTexCoord.s, fragTexCoord.t, 0, 1);
   // finalColor = vec4(1, 0, 0, 1);
}";

    string skeletonVertexShaderSource = @"
#version 150

uniform mat4 camera;
uniform mat4 model;

in vec3 vert;
in vec4 color;

out vec4 fragColor;

void main() {
    fragColor = color;
    
    // Apply all matrix transformations to vert
    gl_Position = camera * model * vec4(vert, 1);
}";

    string skeletonFragmentShaderSource = @"
#version 150

in vec4 fragColor;

out vec4 finalColor;

void main() {
    finalColor = fragColor;
}";

    Model dwarf;

    Matrix4 projectionMatrix, modelviewMatrix, world;

    Program shader;
    Program skeletonShader;

    GraphicsDevice _graphicsDevice;

    Texture2D texture1, texture2;

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

      GL.ClearColor(0.1f, 0.2f, 0.5f, 0.0f);
      GL.Enable(EnableCap.DepthTest);

      float aspectRatio = ClientSize.Width / (float)(ClientSize.Height);
      Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 4, aspectRatio, 1, 100, out projectionMatrix);
      modelviewMatrix = Matrix4.LookAt(new Vector3(0, 3, 5), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
      Matrix4 scale = Matrix4.Scale(0.5f, 0.5f, 0.5f);
      Matrix4.Mult(ref scale, ref modelviewMatrix, out modelviewMatrix);
      Matrix4 translate = Matrix4.CreateTranslation(0f, -2.5f, 0f);
      Matrix4.Mult(ref translate, ref modelviewMatrix, out modelviewMatrix);
      world = Matrix4.Identity;

      shader = new Program(vertexShaderSource, fragmentShaderSource);
      //shader.Bind();
      //shader.UniformMatrix4("camera", false, ref projectionMatrix);
      //shader.UniformMatrix4("model", false, ref modelviewMatrix);
      //shader.Unbind();

      skeletonShader = new Program(skeletonVertexShaderSource, skeletonFragmentShaderSource);
      skeletonShader.Bind();
      skeletonShader.UniformMatrix4("camera", false, ref projectionMatrix);
      skeletonShader.UniformMatrix4("model", false, ref modelviewMatrix);
      skeletonShader.Unbind();

      ContentManager content = new ContentManager(new ServiceProvider(), "content");
      content.RegisterTypeReader<Texture2D>(new Texture2DReader());
      content.RegisterTypeReader<Model>(new ModelReader());
      texture1 = content.Get<Texture2D>("\\dwarf.meb").Content as Texture2D;
      texture2 = content.Get<Texture2D>("\\axe.meb").Content as Texture2D;
      dwarf = content.Get<Model>("\\dwarf1.meb").Content as Model;

      dwarf.Meshes[1].MeshParts[0].Material.Passes[0].Parameters["Diffuse"] = UniformValue.Create(new Color4(1f, 0.2f, 0.2f, 1));

      ICamera camera = new LookAtCamera(_graphicsDevice, new Vector3(0, 0, -5), Vector3.UnitY, Vector3.UnitZ);
      _graphicsDevice.Camera = camera;
      world = Matrix4.Scale(0.5f) * Matrix4.CreateTranslation(0, -1.5f, 0);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
      //Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
      Matrix4.Mult(ref rotation, ref world, out world);
      //shader.Bind();
      //shader.UniformMatrix4("model", false, ref modelviewMatrix);
      //shader.Unbind();

      skeletonShader.Bind();
      skeletonShader.UniformMatrix4("model", false, ref modelviewMatrix);
      skeletonShader.Unbind();

      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    VertexPositionColor[] points = new VertexPositionColor[] {
        new VertexPositionColor(new Vector3(-2.096993f, 2.968065f, 0.582794f), new Vector4(0f, 1f, 1f, 1f)),
        new VertexPositionColor(new Vector3(-1.973066f, 2.815842f, 0.572127f), new Vector4(0f, 1f, 1f, 1f)),
        new VertexPositionColor(new Vector3(-2.096993f, 2.968065f, 0.582794f), new Vector4(0f, 1f, 1f, 1f)),
        new VertexPositionColor(new Vector3(-2.179054f, 3.086323f, 0.438969f), new Vector4(0f, 1f, 1f, 1f)),
        new VertexPositionColor(new Vector3(-1.986928f, 2.77321f, 0.365077f), new Vector4(0f, 1f, 1f, 1f)),
        new VertexPositionColor(new Vector3(-1.973066f, 2.815842f, 0.572127f), new Vector4(0f, 1f, 1f, 1f))
      };
    VertexBuffer buffer;

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      int count = 0;
      foreach (ModelMesh mesh in dwarf.Meshes)
      {
        foreach (ModelMeshPart part in mesh.MeshParts)
        {
          //part.VertexArray.AddBinding(shader.VertexAttribute("vert").Slot, VertexUsage.Position);
          //part.VertexArray.AddBinding(shader.VertexAttribute("vertTexCoord").Slot, VertexUsage.TextureCoordinate);

          //shader.Bind();
          //shader.Uniform1("tex", 0);
          //GL.ActiveTexture(TextureUnit.Texture0);
          //if (count == 0)
          //  texture2.Bind();
          //else
          //  texture1.Bind();
          //part.VertexArray.Bind();
          //GL.DrawElements(part.VertexBuffer.BeginMode, part.IndexBuffer.Length, part.IndexBuffer.ElementsType, new IntPtr(0));
          ////GL.DrawElementsBaseVertex(part.VertexBuffer.BeginMode, (int)part.NumIndicies, part.IndexBuffer.ElementsType, new IntPtr(part.StartIndex), (int)part.StartVertex);
          //count++;
          //Utilities.CheckGLError();
          //part.VertexArray.Unbind();
          //Utilities.CheckGLError();
          //shader.Unbind();
          //Utilities.CheckGLError();
          //texture2.Unbind();
          //shader.Bind();
          //if (count == 0)
          //  shader.BindUniforms(new Dictionary<string, IUniformValue>() { { "tex", UniformValue.Create(Sampler.Create(texture2)) } });
          //else
          //  shader.BindUniforms(new Dictionary<string, IUniformValue>() { { "tex", UniformValue.Create(Sampler.Create(texture1)) } });
          //count++;
          // this will eventually be handled by a camera class attached to the GraphicsDevice
          //shader.BindUniforms(new Dictionary<string, IUniformValue>() { { "WorldView", UniformValue.Create(Matrix4.Mult(modelviewMatrix, projectionMatrix)) } });
          //part.Material.Passes[0].Apply(_graphicsDevice);
          //part.Material.Passes[0].Program.Bind();
          //part.Material.Passes[0].Program.BindUniforms(part.Material.Passes[0].Parameters);
          //shader.BindUniforms(part.Material.Passes[0].Parameters);
          _graphicsDevice.World = world;
          _graphicsDevice.DrawVertexArray(part.VertexArray, part.Material);
        }
      }

      // draw testing points for arm

      //if (buffer == null)
      //{
      //  buffer = VertexBuffer.Create(VertexFormat.PositionColor, points);
      //  buffer.Bind();
      //  int slot1 = skeletonShader.VertexAttributeSlot("vert");
      //  GL.EnableVertexAttribArray(slot1);
      //  GL.VertexAttribPointer(slot1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
      //  int slot2 = skeletonShader.VertexAttributeSlot("color");
      //  GL.EnableVertexAttribArray(slot2);
      //  GL.VertexAttribPointer(slot2, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
      //}
      //buffer.Bind();
      //Utilities.CheckGLError();
      //skeletonShader.Bind();
      //Utilities.CheckGLError();
      
      //Utilities.CheckGLError();
      //GL.DrawArrays(BeginMode.Points, 0, buffer.Count);
      //Utilities.CheckGLError();
      //skeletonShader.Unbind();
      //buffer.Unbind();
      //Utilities.CheckGLError();

      //RenderSkeleton(dwarf.Skeleton);
      SwapBuffers();
      DisposalManager.Process();
    }

    protected override void OnUnload(EventArgs e)
    {
      DisposalManager.Process();
    }

    private void AddBoneLines(Minotaur.Graphics.Bone bone, List<VertexPositionColor> lineData, Matrix4 parentTransform)
    {
      Matrix4 start = parentTransform;
      Matrix4 end = bone.Transform * parentTransform;

      if (Vector3.Subtract(GetTranslation(end), GetTranslation(start)).LengthSquared > 0)
      {
        if (lineData.Count == 0) System.Diagnostics.Debug.WriteLine(bone.Name);
        if (bone.Name == "weapon" || bone.Parent.Name == "weapon")
        {
          lineData.Add(new VertexPositionColor(GetTranslation(start), new Vector4(1f, 0, 0, 1f)));
          lineData.Add(new VertexPositionColor(GetTranslation(end), new Vector4(1f, 0, 0, 1f)));
        }
        else
        {
          lineData.Add(new VertexPositionColor(GetTranslation(start), Vector4.One));
          lineData.Add(new VertexPositionColor(GetTranslation(end), Vector4.One));
        }
      }

      foreach (Minotaur.Graphics.Bone child in bone.Children)
      {
        AddBoneLines(child, lineData, end);
      }
    }

    private void RenderSkeleton(Skeleton skeleton)
    {
      List<VertexPositionColor> lineData = new List<VertexPositionColor>();
      AddBoneLines(skeleton.Bones[0], lineData, Matrix4.Identity);
      Utilities.CheckGLError();
      using (VertexBuffer vbo = new VertexBuffer(VertexFormat.PositionColor, lineData.Count))
      {
        Utilities.CheckGLError();
        GL.Disable(EnableCap.DepthTest);
        Utilities.CheckGLError();

        vbo.SetData(lineData.ToArray());
        Utilities.CheckGLError();
        skeletonShader.Bind();
        Utilities.CheckGLError();
        vbo.Bind();
        Utilities.CheckGLError();
        int slot1 = skeletonShader.VertexAttributeSlot("vert");
        GL.EnableVertexAttribArray(slot1);
        GL.VertexAttribPointer(slot1, 3, VertexAttribPointerType.Float, false, 7 * sizeof(float), 0);
        GL.EnableVertexAttribArray(slot1);
        int slot2 = skeletonShader.VertexAttributeSlot("color");
        GL.EnableVertexAttribArray(slot2);
        GL.VertexAttribPointer(slot2, 4, VertexAttribPointerType.Float, false, 7 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(slot2);
        Utilities.CheckGLError();
        GL.DrawArrays(BeginMode.Lines, 0, vbo.Count);
        Utilities.CheckGLError();
        skeletonShader.Unbind();
        Utilities.CheckGLError();
        GL.DisableVertexAttribArray(slot1);
        GL.DisableVertexAttribArray(slot2);
        vbo.Unbind();
        Utilities.CheckGLError();

        GL.Enable(EnableCap.DepthTest);
        Utilities.CheckGLError();
      }
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
