using System;
using OpenTK;

namespace Minotaur.Content
{
  public class Vector2Reader : ContentTypeReader<Vector2>
  {
    public Vector2Reader()
      : base(new Guid("26e17695-92e1-4470-b1dd-5f480a0e05ff")) { }

    public override object Read(ContentReader reader)
    {
      Vector2 vec = new Vector2();
      vec.X = reader.ReadSingle();
      vec.Y = reader.ReadSingle();
      return vec;
    }
  }
}
