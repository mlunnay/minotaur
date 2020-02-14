using System;
using Minotaur.Core;

namespace Minotaur.Components
{
  public interface IUpdateable
  {
    int UpdateOrder { get; set; }
    bool Enabled { get; set; }

    event EventHandler UpdateOderChanged;

    void Update(GameClock gameclock);
  }
}
