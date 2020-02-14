using System;

namespace Minotaur.Content
{
  public class TimeSpanReader : ContentTypeReader<TimeSpan>
  {
    public TimeSpanReader()
      : base(new Guid("489322ed-bd66-432f-ac71-21b20828e81a")) { }

    public override object Read(ContentReader reader)
    {
      return TimeSpan.FromTicks(reader.ReadInt64());
    }
  }
}
