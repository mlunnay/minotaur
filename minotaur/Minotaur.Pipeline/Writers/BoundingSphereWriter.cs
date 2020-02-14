using System;
using Minotaur.Core;
using OpenTK;

namespace Minotaur.Pipeline.Writers
{
  public class BoundingSphereWriter : ContentTypeWriter<BoundingSphere>
  {
    public BoundingSphereWriter()
      : base(new Guid("94518210-7b67-4d12-b32b-f73c6f3c09a4")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<Vector3>(new Vector3Writer());
    }

    public override void Write(ContentWriter writer, BoundingSphere value)
    {
      writer.WriteRawObject<Vector3>(value.Center);
      writer.Write(value.Radius);
    }
  }
}
