using System;
using Minotaur.Graphics.Animation;
using OpenTK;

namespace Minotaur.Pipeline.Writers
{
  public class VectorKeyWriter : ContentTypeWriter<VectorKey>
  {
    public VectorKeyWriter()
      : base(new Guid("80AE34C8-9995-4020-BF1E-F6B9E861BE0C")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<Vector3>(new Vector3Writer());
    }

    public override void Write(ContentWriter writer, VectorKey value)
    {
      writer.Write(value.Time);
      writer.WriteRawObject(value.Vector);
    }
  }
}
