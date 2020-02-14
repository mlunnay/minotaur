using System;
using System.Collections.Generic;
using System.Linq;
using Minotaur.Core;
using Minotaur.Graphics;
using OpenTK;
using Minotaur.Graphics.Animation;

namespace Minotaur.Content
{
  public class ModelReader : ContentTypeReader<Model>
  {
    private VertexBuffer vertexBuffer;

    public ModelReader()
      : base(new Guid("09b12e6d-acf3-4cf5-a150-923056d88d9b")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<BoundingSphere>(new BoundingSphereReader());
      manager.RegisterTypeReader<Matrix4>(new MatrixReader());
      manager.RegisterTypeReader<VertexBuffer>(new VertexBufferReader());
      manager.RegisterTypeReader<IndexBuffer>(new IndexBufferReader());
      manager.RegisterTypeReader<Material>(new MaterialReader());
      manager.RegisterTypeReader<BoneAnimationClip>(new BoneAnimationClipReader());
    }

    public override object Read(ContentReader reader)
    {
      // bones information
      int boneCount = (int)reader.ReadUInt32();
      List<Bone> bones = new List<Bone>(boneCount);
      List<Matrix4> absoluteTransforms = new List<Matrix4>(boneCount);
      for (int i = 0; i < boneCount; i++)
      {
        string name = reader.ReadString();
        Matrix4 transform = reader.ReadObjectRaw<Matrix4>();
        Matrix4 absoluteTransform = reader.ReadObjectRaw<Matrix4>();
        bones.Add(new Bone(i, name, transform));
        absoluteTransforms.Add(absoluteTransform);
      }
      for (int i = 0; i < boneCount; i++)
      {
        Bone bone = bones[i];
        int parent = (int)reader.ReadUInt32();
        //bone.Parent = parent == 0 ? null : bones[parent - 1];
        int childCount = (int)reader.ReadUInt32();
        Bone[] children = new Bone[childCount];
        for (int j = 0; j < childCount; j++)
        {
          int child = (int)reader.ReadUInt32();
          if (child == 0)
            throw new ContentLoadException(string.Format("Null child for bone: {0}", bone.Name));
          children[j] = bones[child - 1];
        }
        bone.SetParentAndChildren(parent == 0 ? null : bones[parent - 1], children);
      }
      
      // mesh information
      int meshCount = (int)reader.ReadUInt32();
      List<ModelMesh> meshes = new List<ModelMesh>();
      for (int i = 0; i < meshCount; i++)
      {
        string meshName = reader.ReadString();
        int parentBone = (int)reader.ReadUInt32();
        BoundingSphere boundingSphere = reader.ReadObjectRaw<BoundingSphere>();
        reader.ReadSharedResource<object>(o => meshes[i].Tag = o);
        int meshPartCount = (int)reader.ReadUInt32();
        List<ModelMeshPart> parts = new List<ModelMeshPart>();
        for (int j = 0; j < meshPartCount; j++)
        {
          ModelMeshPart part = new ModelMeshPart();
          part.StartVertex = reader.ReadUInt32();
          part.NumVertices = reader.ReadUInt32();
          part.NumIndicies = reader.ReadUInt32();
          part.StartIndex = reader.ReadUInt32();
          part.PrimitiveCount = reader.ReadUInt32();
          reader.ReadSharedResource<object>(o => part.Tag = o);
          reader.ReadSharedResource<VertexBuffer>(x =>
          {
            vertexBuffer = x;
          });
          reader.ReadSharedResource<IndexBuffer>(x =>
          {
            part.VertexArray = new VertexArray(vertexBuffer, x);
          });
          reader.ReadSharedResource<Material>(x =>
          {
            part.Material = x;
          });
          parts.Add(part);
        }
        ModelMesh mesh = new ModelMesh(parts);
        mesh.Name = meshName;
        mesh.ParentBone = parentBone == 0 ? null : bones[parentBone - 1];
        mesh.BoundingSphere = boundingSphere;
        meshes.Add(mesh);
      }
      uint rootBoneIndex = reader.ReadUInt32();
      Skeleton skeleton = rootBoneIndex == 0 ? null : new Skeleton(bones, (int)rootBoneIndex - 1, absoluteTransforms.ToList());
      Model model = new Model(skeleton, meshes);
      reader.ReadSharedResource<object>(o => model.Tag = o);
      int animationCount = (int)reader.ReadUInt32();
      model.Animations = new List<BoneAnimationClip>();
      for (int i = 0; i < animationCount; i++)
        model.Animations.Add(reader.ReadExternalReference<BoneAnimationClip>());
      
      return model;
    }
  }
}
