using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Minotaur.Graphics;
using Minotaur.Core;
using System.Collections.Generic;
using Assimp;
using System.IO;
using System.Linq;
using Assimp.Configs;
using System.Drawing;

namespace ModelTest
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

    #region Assimp Import Flags
    /// <summary>
    /// Target post-processing steps for the standard flat tangent mesh.
    /// </summary>
    private const PostProcessSteps DefaultFlags = PostProcessSteps.CalculateTangentSpace // calculate tangents and bitangents if possible
        | PostProcessSteps.JoinIdenticalVertices // join identical vertices/ optimize indexing
        | PostProcessSteps.ValidateDataStructure // perform a full validation of the loader's output
        | PostProcessSteps.ImproveCacheLocality // improve the cache locality of the output vertices
        | PostProcessSteps.RemoveRedundantMaterials // remove redundant materials
        | PostProcessSteps.FindDegenerates // remove degenerated polygons from the import
        | PostProcessSteps.FindInvalidData // detect invalid model data, such as invalid normal vectors
        | PostProcessSteps.GenerateUVCoords // convert spherical, cylindrical, box and planar mapping to proper UVs
        | PostProcessSteps.TransformUVCoords // preprocess UV transformations (scaling, translation ...)
        | PostProcessSteps.FindInstances // search for instanced meshes and remove them by references to one master
        | PostProcessSteps.LimitBoneWeights // limit bone weights to 4 per vertex
        | PostProcessSteps.OptimizeMeshes // join small meshes, if possible;
      // | PostProcessSteps.FixInfacingNormals
        | PostProcessSteps.GenerateSmoothNormals // generate smooth normal vectors if not existing
        | PostProcessSteps.SplitLargeMeshes // split large, unrenderable meshes into submeshes
        | PostProcessSteps.Triangulate // triangulate polygons with more than 3 edges
        | PostProcessSteps.SortByPrimitiveType // make 'clean' meshes which consist of a single typ of primitives
        | PostProcessSteps.PreTransformVertices // bake transforms, fixes most errors for Xna
       // | PostProcessSteps.FlipUVs  // common DirectX issue (Xna also)

        | PostProcessSteps.MakeLeftHanded  // Makes the model import the right way round (not flipped left to right).
        | PostProcessSteps.FlipWindingOrder
        ;
    #endregion

    Model dwarf;

    Matrix4 projectionMatrix, modelviewMatrix;

    Program shader;
    Program skeletonShader;

    Texture2D texture1, texture2;

    public Game()
      : base(800, 600, GraphicsMode.Default, "Model Test", 0,
      DisplayDevice.Default, 3, 0,
      GraphicsContextFlags.ForwardCompatible | GraphicsContextFlags.Debug)
    {
    }

    protected override void OnLoad(EventArgs e)
    {
      GraphicsDevice graphicsDevice = new GraphicsDevice(this);
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

      shader = new Program(vertexShaderSource, fragmentShaderSource);
      shader.Bind();
      shader.UniformMatrix4("camera", false, ref projectionMatrix);
      shader.UniformMatrix4("model", false, ref modelviewMatrix);
      shader.Unbind();

      skeletonShader = new Program(skeletonVertexShaderSource, skeletonFragmentShaderSource);
      skeletonShader.Bind();
      skeletonShader.UniformMatrix4("camera", false, ref projectionMatrix);
      skeletonShader.UniformMatrix4("model", false, ref modelviewMatrix);
      skeletonShader.Unbind();

      // load model with ASSIMP.NET
      AssimpImporter importer = new AssimpImporter();
      Scene scene = importer.ImportFile(@"..\..\..\Resources\dwarf2.x", PostProcessPreset.TargetRealTimeMaximumQuality);
      
      List<ModelMeshPart> meshParts = new List<ModelMeshPart>();
      foreach (Mesh mesh in scene.Meshes)
      {
        ModelMeshPart part = new ModelMeshPart();
        List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
        List<int> indices = new List<int>();
        for(int i = 0; i < mesh.VertexCount; i++)
        {
          vertices.Add(new VertexPositionNormalTexture(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z, mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z, mesh.GetTextureCoords(0)[i].X, 1 - mesh.GetTextureCoords(0)[i].Y));
        }
        foreach (Face face in mesh.Faces)
        {
          foreach (uint i in face.Indices)
          {
            indices.Add((int)i);
          }
        }
        part.VertexArray = new VertexArray(VertexBuffer.Create(VertexFormat.PositionNormalTexture, vertices.ToArray()), IndexBuffer.Create(indices.ToArray()));
        part.NumVertices = (uint)mesh.VertexCount;
        part.PrimitiveCount = (uint)mesh.FaceCount;
        meshParts.Add(part);
      }

      // create skeleton
      List<Minotaur.Graphics.Bone> bones = new List<Minotaur.Graphics.Bone>();
      CreateSkeleton(bones, scene.RootNode);
      
      ModelMesh modelMesh = new ModelMesh(meshParts);
      dwarf = new Model(new Skeleton(bones), new List<ModelMesh>() { modelMesh});

      Bitmap bmp = new Bitmap(@"..\..\..\Resources\dwarf.jpg");
      texture1 = Texture2D.Create(bmp, TextureMinFilter.Linear, TextureMagFilter.Linear);
      bmp = new Bitmap(@"..\..\..\Resources\axe.jpg");
      texture2 = Texture2D.Create(bmp, TextureMinFilter.Linear, TextureMagFilter.Linear);
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
      Matrix4 rotation = Matrix4.CreateRotationY((float)e.Time);
      Matrix4.Mult(ref rotation, ref modelviewMatrix, out modelviewMatrix);
      shader.Bind();
      shader.UniformMatrix4("model", false, ref modelviewMatrix);
      shader.Unbind();

      skeletonShader.Bind();
      skeletonShader.UniformMatrix4("model", false, ref modelviewMatrix);
      skeletonShader.Unbind();

      if (Keyboard[OpenTK.Input.Key.Escape])
        Exit();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      GL.Viewport(0, 0, Width, Height);
      GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

      int count = 0;
      foreach (ModelMesh mesh in dwarf.Meshes)
      {
        foreach (ModelMeshPart part in mesh.MeshParts)
        {
          part.VertexArray.AddBinding(shader.VertexAttribute("vert").Slot, VertexUsage.Position);
          part.VertexArray.AddBinding(shader.VertexAttribute("vertTexCoord").Slot, VertexUsage.TextureCoordinate);

          shader.Bind();
          shader.Uniform1("tex", 0);
          GL.ActiveTexture(TextureUnit.Texture0);
          if (count == 0)
            texture2.Bind();
          else
            texture1.Bind();
          part.VertexArray.Bind();
          GL.DrawElements(part.VertexBuffer.BeginMode, part.IndexBuffer.Length, part.IndexBuffer.ElementsType, 0);
          count++;
          //Utilities.CheckGLError();
          //part.VertexArray.Unbind();
          //Utilities.CheckGLError();
          shader.Unbind();
          Utilities.CheckGLError();
        }
      }

      RenderSkeleton(dwarf.Skeleton);
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
        if (bone.Name == "lelbo" || bone.Parent.Name == "weapon")
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

    private void CreateSkeleton(List<Minotaur.Graphics.Bone> bones, Node node)
    {
      int index = 0;
      CreateSkeleton(bones, node, null, ref index);
    }

    private Minotaur.Graphics.Bone CreateSkeleton(List<Minotaur.Graphics.Bone> bones, Node node, Minotaur.Graphics.Bone parent, ref int index)
    {
      Minotaur.Graphics.Bone bone = new Minotaur.Graphics.Bone(index++, node.Name, ToMatrix4(node.Transform));
      bones.Add(bone);
      if (node.HasChildren)
      {
        Minotaur.Graphics.Bone[] children = new Minotaur.Graphics.Bone[node.ChildCount];
        for (int i = 0; i < node.ChildCount; i++)
        {
          children[i] = CreateSkeleton(bones, node.Children[i], bone, ref index);
        }
        bone.SetParentAndChildren(parent, children);
      }
      else if (parent != null)
        bone.Parent = parent;
      return bone;

    }

    private Matrix4 ToMatrix4(Matrix4x4 mat)
    {
      return new Matrix4(mat.A1, mat.A2, mat.A3, mat.A4, mat.B1, mat.B2, mat.B3, mat.B4, mat.C1, mat.C2, mat.C3, mat.C4, mat.D1, mat.D2, mat.D3, mat.D4);
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
