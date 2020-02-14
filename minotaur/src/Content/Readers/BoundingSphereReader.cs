using System;
using Minotaur.Core;
using OpenTK;

namespace Minotaur.Content
{
  public class BoundingSphereReader : ContentTypeReader<BoundingSphere>
  {
    public BoundingSphereReader()
      : base(new Guid("94518210-7b67-4d12-b32b-f73c6f3c09a4")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader(typeof(Vector3), new Vector3Reader());
    }

    public override object Read(ContentReader reader)
    {
      BoundingSphere result = new BoundingSphere();
      result.Center = reader.ReadObjectRaw<Vector3>();
      result.Radius = reader.ReadSingle();
      return result;
    }
  }
}
