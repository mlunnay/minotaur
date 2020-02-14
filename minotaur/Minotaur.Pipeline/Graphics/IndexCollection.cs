using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Minotaur.Pipeline.Graphics
{
  public class IndexCollection : Collection<uint>
  {
    /// <summary>Appends the specified indices into the collection.</summary>
    /// <param name="indices">Index into the Positions member of the parent.</param>
    public void AddRange(IEnumerable<uint> indices)
    {
      if (indices == null)
      {
        throw new ArgumentNullException("indices");
      }
      foreach (uint current in indices)
      {
        base.Items.Add(current);
      }
    }
  }
}
