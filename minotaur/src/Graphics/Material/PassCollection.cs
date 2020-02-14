using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Minotaur.Graphics
{
  public class PassCollection : Collection<Pass>, ICloneable
  {
    public PassCollection() : base() { }

    public PassCollection(IList<Pass> passes) : base(passes) { }

    public Pass this[string name]
    {
      get
      {
        return this.FirstOrDefault(p => p.Name == name);
      }
    }

    public object Clone()
    {
      List<Pass> passes = this.Select(p => (Pass)p.Clone()).ToList();
      return new PassCollection(passes);
    }
  }
}
