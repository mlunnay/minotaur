using System;
using System.Collections.Generic;

namespace Minotaur.Pipeline
{
  public class ContentItem
  {
    private Dictionary<string, object> _opaqueData = new Dictionary<string, object>();

    public String Name { get; set; }

    public Dictionary<string, object> OpaqueData { get { return _opaqueData; } }

    public ContentItem() { }
  }
}
