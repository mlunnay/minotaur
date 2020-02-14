using System;
using OpenTK;

namespace Minotaur.Content
{
  public class Vector4Reader : ContentTypeReader<Vector4>
  {
    public Vector4Reader()
      : base(new Guid("53e58fab-cf87-4b34-9062-4d18760db2ca")) { }

    public override object Read(ContentReader reader)
    {
      Vector4 vec = new Vector4();
      vec.X = reader.ReadSingle();
      vec.Y = reader.ReadSingle();
      vec.Z = reader.ReadSingle();
      vec.W = reader.ReadSingle();
      return vec;
    }
  }
}
