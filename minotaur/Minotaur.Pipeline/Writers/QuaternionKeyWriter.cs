using System;
using Minotaur.Graphics.Animation;
using OpenTK;

namespace Minotaur.Pipeline.Writers
{
  public class QuaternionKeyWriter : ContentTypeWriter<QuaternionKey>
  {
    public QuaternionKeyWriter()
      : base(new Guid("84FC85E2-6D29-44AC-8105-803035CD8790")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<Quaternion>(new QuaternionWriter());
    }

    public override void Write(ContentWriter writer, QuaternionKey value)
    {
      writer.Write(value.Time);
      writer.WriteRawObject(value.Rotation);
    }
  }
}
