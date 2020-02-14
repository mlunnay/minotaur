using System;
using Minotaur.Graphics.Animation;
using OpenTK;

namespace Minotaur.Content
{
  public class QuaternionKeyReader : ContentTypeReader<QuaternionKey>
  {
    public QuaternionKeyReader()
      : base(new Guid("84FC85E2-6D29-44AC-8105-803035CD8790")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<Quaternion>(new QuaternionReader());
    }

    public override object Read(ContentReader reader)
    {
      float time = reader.ReadSingle();
      Quaternion q = reader.ReadObjectRaw<Quaternion>();
      return new QuaternionKey(time, q);
    }
  }
}
