using System;
using Minotaur.Core;
using OpenTK;

namespace Minotaur.Content
{
  public class BoundingBoxReader : ContentTypeReader<BoundingBox>
  {
    public BoundingBoxReader()
      : base(new Guid("ed1fe179-a7c9-47ff-a281-52d387d83360")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader(typeof(Vector3), new Vector3Reader());
    }

    public override object Read(ContentReader reader)
    {
      BoundingBox result = new BoundingBox();
      result.Min = reader.ReadObjectRaw<Vector3>();
      result.Max = reader.ReadObjectRaw<Vector3>();
      return result;
    }
  }
}
