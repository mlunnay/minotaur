using System;
using ComponentKit.Model;
using Minotaur.Core;

namespace Minotaur.Components
{
  public abstract class GameComponent : DependencyComponent, IGameComponent, IUpdateable
  {
    private bool _enabled = true;
    private int _updateOrder = 0;

    #region IUpdateable Members

    public int UpdateOrder
    {
      get { return _updateOrder; }
      set
      {
        if (_updateOrder != value)
        {
          _updateOrder = value;
          OnUpdateOrderChanged(EventArgs.Empty);
        }
      }
    }

    public bool Enabled
    {
      get { return _enabled; }
      set { _enabled = value; }
    }

    public event EventHandler UpdateOderChanged;

    public virtual void Update(GameClock gameclock) { }

    #endregion

    protected virtual void OnUpdateOrderChanged(EventArgs e)
    {
      if (UpdateOderChanged != null)
        UpdateOderChanged(this, e);
    }

    protected override void OnAdded(ComponentStateEventArgs registrationArgs)
    {
      base.OnAdded(registrationArgs);
      Initialize();
    }

    /// <summary>
    /// Override to do initialization when the object has been successfully added to a Entity.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called before the first update is called.
    /// This is where any global communication should take place.
    /// </summary>
    public virtual void Setup() { }
  }
}
