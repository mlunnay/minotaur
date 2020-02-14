using System;
using System.Collections.ObjectModel;

namespace Minotaur.Pipeline.Graphics
{
  public class MipmapChainCollection : Collection<MipmapChain>
  {
    private bool _fixedSize;

    public bool FixedSize { get { return _fixedSize; } }

    public MipmapChainCollection(int initialSize, bool fixedSize = true)
    {
      for (int i = 0; i < initialSize; i++)
      {
        base.Add(new MipmapChain());
      }
      _fixedSize = fixedSize;
    }

    protected override void ClearItems()
    {
      if (_fixedSize)
        throw new NotSupportedException("MipmapChainCollection's size is fixed");
      base.ClearItems();
    }

    protected override void InsertItem(int index, MipmapChain item)
    {
      if (_fixedSize)
        throw new NotSupportedException("MipmapChainCollection's size is fixed");
      base.InsertItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
      if (_fixedSize)
        throw new NotSupportedException("MipmapChainCollection's size is fixed");
      base.RemoveItem(index);
    }
  }
}
