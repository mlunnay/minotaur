using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Pipeline
{
  public class ContentProcessorAttribute : Attribute
  {
    public virtual string DisplayName { get; set; }

    public ContentProcessorAttribute() { }
  }
}
