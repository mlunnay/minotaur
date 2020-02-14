using System;
using Minotaur.Core;
using OpenTK;

namespace Minotaur.Content
{
  public class RayReader : ContentTypeReader<Ray>
  {
    public RayReader()
      : base(new Guid("46b14b3c-ba64-4b5f-ad6a-e3ba213bc7cf")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader(typeof(Vector3), new Vector3Reader());
    }

    public override object Read(ContentReader reader)
    {
      Ray result = new Ray();
      result.Position = reader.ReadObjectRaw<Vector3>();
      result.Direction = reader.ReadObjectRaw<Vector3>();
      return result;
    }
  }
}
