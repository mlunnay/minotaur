using System;
using Minotaur.Graphics.Animation;
using OpenTK;

namespace Minotaur.Content
{
  public class VectorKeyReader : ContentTypeReader<VectorKey>
  {
    public VectorKeyReader()
      : base(new Guid("80AE34C8-9995-4020-BF1E-F6B9E861BE0C")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<Vector3>(new Vector3Reader());
    }

    public override object Read(ContentReader reader)
    {
      float time = reader.ReadSingle();
      Vector3 v = reader.ReadObjectRaw<Vector3>();
      return new VectorKey(time, v);
    }
  }
}
