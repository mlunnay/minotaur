using System;
using OpenTK;

namespace Minotaur.Content
{
  public class MatrixReader : ContentTypeReader<Matrix4>
  {
    public MatrixReader()
      : base(new Guid("ea3c5404-8a3c-4d13-9754-0b3a26bd3629")) { }

    public override object Read(ContentReader reader)
    {
      Matrix4 result = new Matrix4();
      result.M11 = reader.ReadSingle();
      result.M12 = reader.ReadSingle();
      result.M13 = reader.ReadSingle();
      result.M14 = reader.ReadSingle();
      result.M21 = reader.ReadSingle();
      result.M22 = reader.ReadSingle();
      result.M23 = reader.ReadSingle();
      result.M24 = reader.ReadSingle();
      result.M31 = reader.ReadSingle();
      result.M32 = reader.ReadSingle();
      result.M33 = reader.ReadSingle();
      result.M34 = reader.ReadSingle();
      result.M41 = reader.ReadSingle();
      result.M42 = reader.ReadSingle();
      result.M43 = reader.ReadSingle();
      result.M44 = reader.ReadSingle();
      return result;
    }
  }
}
