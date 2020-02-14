using System;
using Minotaur.Core;
using OpenTK;

namespace Minotaur.Content
{
  public class PlaneReader : ContentTypeReader<Plane>
  {
    public PlaneReader()
      : base(new Guid("947e0c5b-489a-4a50-a68a-dadde0a1a104")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader(typeof(Vector3), new Vector3Reader());
    }

    public override object Read(ContentReader reader)
    {
      Plane result = new Plane();
      result.Normal = reader.ReadObjectRaw<Vector3>();
      result.D = reader.ReadSingle();
      return result;
    }
  }
}
