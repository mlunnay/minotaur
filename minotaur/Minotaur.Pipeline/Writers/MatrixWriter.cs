using System;
using OpenTK;

namespace Minotaur.Pipeline.Writers
{
  public class MatrixWriter : ContentTypeWriter<Matrix4>
  {
    public MatrixWriter()
      : base(new Guid("ea3c5404-8a3c-4d13-9754-0b3a26bd3629")) { }

    public override void Write(ContentWriter writer, Matrix4 value)
    {
      writer.Write(value.M11);
      writer.Write(value.M12);
      writer.Write(value.M13);
      writer.Write(value.M14);
      writer.Write(value.M21);
      writer.Write(value.M22);
      writer.Write(value.M23);
      writer.Write(value.M24);
      writer.Write(value.M31);
      writer.Write(value.M32);
      writer.Write(value.M33);
      writer.Write(value.M34);
      writer.Write(value.M41);
      writer.Write(value.M42);
      writer.Write(value.M43);
      writer.Write(value.M44);
    }
  }
}
