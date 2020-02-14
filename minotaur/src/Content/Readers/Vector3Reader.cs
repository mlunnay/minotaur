using System;
using OpenTK;

namespace Minotaur.Content
{
  public class Vector3Reader : ContentTypeReader<Vector3>
  {
    public Vector3Reader()
      : base(new Guid("adc3a259-a7a5-4c71-9d7e-1a25899f4dec")) { }

    public override object Read(ContentReader reader)
    {
      Vector3 vec = new Vector3();
      vec.X = reader.ReadSingle();
      vec.Y = reader.ReadSingle();
      vec.Z = reader.ReadSingle();
      return vec;
    }
  }
}
