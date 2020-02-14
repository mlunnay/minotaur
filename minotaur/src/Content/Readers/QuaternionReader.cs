using System;
using OpenTK;

namespace Minotaur.Content
{
  public class QuaternionReader : ContentTypeReader<Quaternion>
  {
    public QuaternionReader()
      : base(new Guid("c48876b4-4c12-4b2d-bf52-18f3b022e4c3")) { }

    public override object Read(ContentReader reader)
    {
      Quaternion result = new Quaternion();
      result.X = reader.ReadSingle();
      result.Y = reader.ReadSingle();
      result.Z = reader.ReadSingle();
      result.W = reader.ReadSingle();
      return result;
    }
  }
}
