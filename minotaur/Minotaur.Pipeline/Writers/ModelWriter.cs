using System;
using Minotaur.Pipeline.Graphics;
using OpenTK;
using Minotaur.Core;
using Minotaur.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class ModelWriter : ContentTypeWriter<ModelContent>
  {
    public ModelWriter()
      : base(new Guid("09b12e6d-acf3-4cf5-a150-923056d88d9b")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<Matrix4>(new MatrixWriter());
      manager.RegisterTypeWriter<VertexBufferContent>(new VertexBufferWriter());
      manager.RegisterTypeWriter<IndexCollection>(new IndexBufferWriter());
      manager.RegisterTypeWriter<Minotaur.Core.BoundingSphere>(new BoundingSphereWriter());
      manager.RegisterTypeWriter<MaterialContent>(new MaterialWriter());
      manager.RegisterTypeWriter<BoneAnimationsContent>(new BoneAnimationWriter());
    }

    public override void Write(ContentWriter writer, ModelContent value)
    {
      writer.Write((uint)value.Bones.Count);
      foreach (BoneContent bone in value.Bones)
      {
        writer.Write(bone.Name);
        writer.WriteRawObject(bone.Transform);
        writer.WriteRawObject(bone.InverseAbsoluteTransform);
      }
      foreach (BoneContent bone in value.Bones)
      {
        writer.Write(bone.Parent != null ? bone.Parent.Index + 1 : 0);
        writer.WriteRawObject((uint)bone.Children.Count);
        foreach (BoneContent child in bone.Children)
        {
          writer.Write(child != null ? child.Index + 1 : 0);
        }
      }
      writer.Write((uint)value.Meshes.Count);
      ModelMeshWriter meshWriter = new ModelMeshWriter();
      foreach (ModelMeshContent mesh in value.Meshes)
      {
        meshWriter.Write(writer, mesh);
      }
      writer.Write(value.RootBone != null ? value.RootBone.Index + 1 : 0);
      writer.WriteSharedResource(value.Tag);
      writer.Write((uint)value.Animations.Count);  // animation count
      foreach (var item in value.Animations)
        writer.WriteRawObject(item);
    }
  }
}
