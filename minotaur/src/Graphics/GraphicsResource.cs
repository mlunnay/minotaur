using System;

namespace Minotaur.Graphics
{
  // base class for Graphic Resources
  public abstract class GraphicsResource : IDisposable
  {
    private bool _disposed = false;

    public bool IsDisposed
    {
      get { return _disposed; }
      protected set { _disposed = value; }
    }

    ~GraphicsResource()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (_disposed)
        return;

      _disposed = true;
    }
  }
}
