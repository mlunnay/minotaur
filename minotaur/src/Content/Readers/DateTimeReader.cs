using System;
using Minotaur.Core;

namespace Minotaur.Content
{
  public class DateTimeReader : ContentTypeReader<DateTime>
  {
    public DateTimeReader()
      : base(new Guid("1b558e2b-f691-477f-9782-ef04463588b3")) { }

    public override object Read(ContentReader reader)
    {
      TimeSpan ts = TimeSpan.FromMilliseconds(reader.ReadInt64());
      return new DateTime(1970, 1, 1) + ts;
    }
  }
}
