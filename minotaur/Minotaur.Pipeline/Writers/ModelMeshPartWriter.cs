using System;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class ModelMeshPartWriter : ContentTypeWriter<ModelMeshPartContent>
  {
    public ModelMeshPartWriter()
      : base(new Guid("92FA4E47-1147-4AE6-9957-B695458E4D92")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<VertexBufferContent>(new VertexBufferWriter());
      manager.RegisterTypeWriter<IndexCollection>(new IndexBufferWriter());
      // TODO: implement MaterialWriter
      //manager.RegisterTypeWriter<MaterialContent>(new MaterialWriter());
    }

    public override void Write(ContentWriter writer, ModelMeshPartContent value)
    {
      writer.Write(value.StartVertex);
      writer.Write(value.NumVertices);
      writer.Write(value.NumIndicies);
      writer.Write(value.StartIndex);
      writer.Write(value.PrimitiveCount);
      writer.WriteSharedResource(value.Tag);
      writer.WriteSharedResource(value.VertexBuffer);
      writer.WriteSharedResource(value.IndexBuffer);
      writer.WriteSharedResource(value.Material);
    }
  }
}
