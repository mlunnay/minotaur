using System;
using Minotaur.Core;
using OpenTK.Graphics;

namespace Minotaur.Content
{
  public class ColorReader : ContentTypeReader<Color4>
  {
    public ColorReader()
      : base(new Guid("00bfc716-ac97-4012-9a35-60d037ea30b6")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {

    }

    public override object Read(ContentReader reader)
    {
      Color4 result = new Color4();
      result.R = reader.ReadSingle();
      result.G = reader.ReadSingle();
      result.B = reader.ReadSingle();
      result.A = reader.ReadSingle();
      return result;
    }
  }
}
