using System;
using System.Collections.Generic;
using System.Linq;
using Minotaur.Pipeline.Graphics;
using Assimp;
using System.ComponentModel;
using System.IO;

namespace Minotaur.Pipeline.Processors
{
  [ContentProcessor(DisplayName="Model Processor")]
  public class ModelProcessor : ContentProcessor<SceneContent, ModelContent>
  {
    private List<BoneContent> _bones = new List<BoneContent>();
    private Dictionary<string, BoneContent> _namesToBones = new Dictionary<string, BoneContent>();

    [Description("Add bone weights and indices for skinning.")]
    public bool Skin { get; set; }

    public bool Normals { get; set; }

    public virtual bool GenerateMipmaps { get; set; }

    [DefaultValueAttribute(true)]
    public virtual bool PremultiplyAlpha { get; set; }

    [DefaultValue(ImageType.RGB)]
    public ImageType OutputType { get; set; }

    [DefaultValue("BasicMaterial")]
    public string DefaultMaterialTypeFactory { get; set; }

    public Dictionary<int, string> MaterialTypeFactoryReplacements { get; set; }

    public List<string> ExportAnimations = new List<string>();

    public ModelProcessor()
    {
      MaterialTypeFactoryReplacements = new Dictionary<int, string>();
      DefaultMaterialTypeFactory = "BasicMaterial";
    }

    public override ModelContent Process(SceneContent input, ContentProcessorContext context)
    {
      List<ModelMeshContent> meshes = new List<ModelMeshContent>();
      VertexBufferContent vertexBuffer = new VertexBufferContent(new VertexAttribute[]{
        new PositionAttribute(),
        new NormalAttribute(),
        new TextureCoordinateAttribute(),
      });

      if (Skin)
      {
        vertexBuffer.AddAttribute(new BlendWeightsAttribute());
        vertexBuffer.AddAttribute(new BlendIndicesAttribute());
      }

      BoneContent rootBone = BuildSkeleton(input.Scene.RootNode, null);

      MaterialContent[] materials = new MaterialContent[input.Scene.MaterialCount];
      for (int i = 0; i < input.Scene.MaterialCount; i++)
      {
          Dictionary<string, object> parameters = new Dictionary<string, object>();
          GetMaterialParameters(input.Scene.Materials[i], parameters, context);
          string materialFactory;
          if (!MaterialTypeFactoryReplacements.TryGetValue(i, out materialFactory))
            materialFactory = DefaultMaterialTypeFactory;
          MaterialContent material = context.ContentManager.MaterialFactoryManager.GetMaterialFactory(materialFactory)(context, parameters);
          material.Name = input.Scene.Materials[i].HasName ? input.Scene.Materials[i].Name : string.Format("{0}_Material_{1}", input.Name, i);
          materials[i] = material;
      }

      uint numVertices = 0;
      uint numIndices = 0;
      for (int i = 0; i < input.Scene.MeshCount; i++)
      {
        Mesh mesh = input.Scene.Meshes[i];
        uint primitiveCount = (uint)mesh.FaceCount;
        uint baseVertex = numVertices;
        uint baseIndex = numIndices;
        uint partNumIndices = (uint)mesh.FaceCount * 3;
        uint partNumVertices = (uint)mesh.VertexCount;

        numVertices += (uint)mesh.VertexCount;
        numIndices += (uint)mesh.FaceCount * 3;

        Dictionary<uint, List<VertexWeight>> boneVertWeights = new Dictionary<uint, List<VertexWeight>>();
        if(Skin)
          ExtractBoneWeights(mesh, boneVertWeights);

        for (int j = 0; j < mesh.VertexCount; j++)
        {
          vertexBuffer[Minotaur.Graphics.VertexUsage.Position].AddValues(mesh, j);
          vertexBuffer[Minotaur.Graphics.VertexUsage.Normal].AddValues(mesh, j);
          vertexBuffer[Minotaur.Graphics.VertexUsage.TextureCoordinate].AddValues(mesh, j);
          if (Skin)
          {
            float[] weights = new float[4];
            byte[] indices = new byte[4];
            GetNormalizedBoneWeights(boneVertWeights[(uint)j], weights, indices);
            ((BlendWeightsAttribute)vertexBuffer[Minotaur.Graphics.VertexUsage.BlendWeight]).AddValues(weights);
            ((BlendIndicesAttribute)vertexBuffer[Minotaur.Graphics.VertexUsage.BlendIndices]).AddValues(indices);
          }
        }

        IndexCollection indecies = new IndexCollection();
        indecies.AddRange(mesh.GetIndices().Select(k => k + (uint)baseVertex));

        ModelMeshPartContent part = new ModelMeshPartContent(vertexBuffer, indecies, baseVertex, partNumVertices, baseIndex, partNumIndices, primitiveCount);
        Minotaur.Core.BoundingSphere boundingSphere = Minotaur.Core.BoundingSphere.CreateFromPoints(
          ((PositionAttribute)vertexBuffer[Minotaur.Graphics.VertexUsage.Position]).Values.Select(
          p => new OpenTK.Vector3(p.X, p.Y, p.Z)));
        ModelMeshContent modelmesh = new ModelMeshContent(mesh.Name, GetMeshParentBone(input.Scene.RootNode, i), new[] {part}, boundingSphere);
        meshes.Add(modelmesh);

        part.Material = materials[mesh.MaterialIndex];
      }

      List<ExternalReferenceContent<BoneAnimationsContent>> animations = new List<ExternalReferenceContent<BoneAnimationsContent>>();
      foreach (Animation anim in input.Scene.Animations)
      {
        if (ExportAnimations.Count == 0 || ExportAnimations.Contains(anim.Name))
        {
          animations.Add(context.ContentManager.BuildContent<BoneAnimationsContent>(input.Name, "ModelImporter",
          processorName: "BoneAnimProcessor",
          processorData: new Dictionary<string,object>() {
            {"AnimationName", anim.Name},
            {"NamePre", string.Format("{0}_", Path.GetFileNameWithoutExtension(input.Name))},
          },
          ignoreBuildItem: true
          ));
        }
      }

      return new ModelContent(rootBone, _bones, meshes, animations);
    }

    private BoneContent BuildSkeleton(Node node, BoneContent parent)
    {
      BoneContent bone = new BoneContent(node.Name, (uint)_bones.Count, ConvertMatrix(node.Transform), parent);
      _bones.Add(bone);
      _namesToBones[bone.Name] = bone;

      bone.InverseAbsoluteTransform = OpenTK.Matrix4.Invert(bone.Transform);
      BoneContent p = bone.Parent;
      while (p != null)
      {
        bone.InverseAbsoluteTransform *= p.Transform;
        p = p.Parent;
      }

      if (node.HasChildren)
      {
        foreach (Node n in node.Children)
        {
          BoneContent child = BuildSkeleton(n, bone);
          bone.Children.Add(child);
        }
      }

      return bone;
    }

    private void ExtractBoneWeights(Mesh mesh, Dictionary<uint, List<VertexWeight>> boneVertWeights)
    {
      foreach (Bone bone in mesh.Bones)
      {
        uint id = _namesToBones[bone.Name].Index;
        foreach (VertexWeight weight in bone.VertexWeights)
        {
          if (boneVertWeights.ContainsKey(weight.VertexID))
            boneVertWeights[weight.VertexID].Add(new VertexWeight(id, weight.Weight));
          else
            boneVertWeights[weight.VertexID] = new List<VertexWeight>() { new VertexWeight(id, weight.Weight) };
        }
      }
    }

    private void GetNormalizedBoneWeights(List<VertexWeight> vertWeights, float[] weights, byte[] indicies)
    {
      if (weights.Length != indicies.Length)
        throw new ArgumentException("Weights and indicies arrays must have the same length.");

      float total = 0f;
      int index = 0;
      for (int i = 0; i < vertWeights.Count && index < weights.Length; i++)
      {
        float weight = vertWeights[i].Weight;
        if (weight <= 0f)
          continue;

        total += weight;
        weights[index] = weight;
        indicies[index] = (byte)vertWeights[i].VertexID;
        index++;
      }

      float mult = 1.0f / total;
      for (int i = 0; i < index; i++)
      {
        weights[i] *= mult;
      }
      for (int i = index; i < weights.Length; i++)
      {
        weights[i] = 0f;
        indicies[i] = 0;
      }
    }

    private BoneContent GetMeshParentBone(Node node, int i)
    {
      if (node.HasMeshes && node.MeshIndices.Contains(i))
        return _namesToBones[node.Name];

      if (node.HasChildren)
      {
        foreach (Node child in node.Children)
        {
          BoneContent b = GetMeshParentBone(child, i);
          if (b != null)
            return b;
        }
      }

      return null;
    }

    private void GetMaterialParameters(Material material, Dictionary<string, object> parameters, ContentProcessorContext context)
    {
      foreach (Assimp.TextureType type in (Assimp.TextureType[])Enum.GetValues(typeof(Assimp.TextureType)))
      {
        if (type == Assimp.TextureType.None || type == Assimp.TextureType.Unknown || material.GetTextureCount(type) == 0)
          continue;

        TextureSlot tex = material.GetTexture(type, 0); // we dont handle multiple textures of a given type so just get the first one.
        string parameterName = "";
        if (tex.TextureType == Assimp.TextureType.Normals)
          parameterName = "NormalMap";
        else
          parameterName = string.Format("{0}Map", tex.TextureType.ToString());

        SamplerContent sampler = new SamplerContent();
        string path = File.Exists(tex.FilePath) ? tex.FilePath : Path.GetFileName(tex.FilePath);
        sampler.Texture = new ExternalReferenceContent<TextureContent>(context.ContentManager.ProcessorContext.GetFilenamePath(path), context.ContentManager)
        {
          ImporterData = new Dictionary<string, object>() { { "GenerateMipmaps", GenerateMipmaps }, { "PremultiplyAlpha", PremultiplyAlpha }, { "OutputType", OutputType } }
        };
        sampler.WrapS = ConvertWrapMode(tex.WrapModeU);
        sampler.WrapT = ConvertWrapMode(tex.WrapModeV);

        parameters[parameterName] = new UniformValueContent(Minotaur.Graphics.UniformValueType.Sampler, new[] { sampler });
      }
      
      if (material.HasBlendMode)
        parameters["BlendMode"] = material.BlendMode;
      if (material.HasBumpScaling)
        parameters["BumpScaling"] = material.BumpScaling;
      if (material.HasColorAmbient)
        parameters["AmbientColor"] = ConvertColor(material.ColorAmbient);
      if (material.HasColorDiffuse)
        parameters["DiffuseColor"] = ConvertColor(material.ColorDiffuse);
      if (material.HasColorEmissive)
        parameters["EmissiveColor"] = ConvertColor(material.ColorEmissive);
      if (material.HasColorReflective)
        parameters["ReflectiveColor"] = ConvertColor(material.ColorReflective);
      if (material.HasColorSpecular)
        parameters["SpecularColor"] = ConvertColor(material.ColorSpecular);
      if (material.HasColorTransparent)
        parameters["TransparentColor"] = ConvertColor(material.ColorTransparent);
      if (material.HasOpacity)
        parameters["Opacity"] = material.Opacity;
      if (material.HasReflectivity)
        parameters["Reflectivity"] = material.Reflectivity;
      if (material.HasShadingMode)
        parameters["ShadingMode"] = material.ShadingMode;
      if (material.HasShininess)
        parameters["Shininess"] = material.Shininess;
      if (material.HasShininessStrength)
        parameters["ShininessStrength"] = material.ShininessStrength;
      if (material.HasTwoSided)
        parameters["TwoSided"] = material.IsTwoSided;
      if (material.HasWireFrame)
        parameters["WireFrame"] = material.IsWireFrameEnabled;
    }

    private OpenTK.Graphics.OpenGL.TextureWrapMode ConvertWrapMode(TextureWrapMode mode)
    {
      switch (mode)
      {
        case TextureWrapMode.Clamp:
          return OpenTK.Graphics.OpenGL.TextureWrapMode.Clamp;
        case TextureWrapMode.Decal:
          return OpenTK.Graphics.OpenGL.TextureWrapMode.ClampToEdge;
        case TextureWrapMode.Mirror:
          return OpenTK.Graphics.OpenGL.TextureWrapMode.MirroredRepeat;
        case TextureWrapMode.Wrap:
          return OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat;
        default:
          return OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat;
      }
    }

    private OpenTK.Graphics.Color4 ConvertColor(Color4D c)
    {
      return new OpenTK.Graphics.Color4(c.R, c.G, c.B, c.A);
    }

    private OpenTK.Matrix4 ConvertMatrix(Matrix4x4 m)
    {
      return new OpenTK.Matrix4(m.A1, m.A2, m.A3, m.A4, m.B1, m.B2, m.B3, m.B4, m.C1, m.C2, m.C3, m.C4, m.D1, m.D2, m.D3, m.D4);
    }
  }
}
