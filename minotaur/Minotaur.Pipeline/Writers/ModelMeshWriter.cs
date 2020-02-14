using System;
using System.Collections.Generic;
using System.Linq;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class ModelMeshWriter : ContentTypeWriter<ModelMeshContent>
  {
    public ModelMeshWriter()
      : base(new Guid("D9E7AC24-C12F-4408-B0C6-63316AC9A6C6")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<Minotaur.Core.BoundingSphere>(new BoundingSphereWriter());
      //manager.RegisterTypeWriter<ModelMeshPartContent>(new ModelMeshPartWriter());
    }

    public override void Write(ContentWriter writer, ModelMeshContent value)
    {
      writer.Write(value.Name);
      writer.Write((uint)(value.ParentBone == null ? 0 : value.ParentBone.Index + 1));
      writer.WriteRawObject(value.BoundingSphere);
      writer.WriteSharedResource(value.Tag);
      writer.Write((uint)value.MeshParts.Count);
      ModelMeshPartWriter partWriter = new ModelMeshPartWriter();
      foreach (ModelMeshPartContent item in value.MeshParts)
      {
        partWriter.Write(writer, item);
      }
    }
  }
}
