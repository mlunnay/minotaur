using System;
using Minotaur.Core;

namespace Minotaur.Content
{
  public class DecimalReader : ContentTypeReader<Decimal>
  {
    public DecimalReader()
      : base(new Guid("1cf31799-eec8-4dd6-ac14-0db3bd3d33db")) { }

    public override object Read(ContentReader reader)
    {
      int[] bits = new int[4];
      for (int i = 0; i < 4; i++)
      {
        bits[i] = reader.ReadInt32();
      }
      return new Decimal(bits);
    }
  }
}
