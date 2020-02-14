using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Core
{
  public interface IUpdateable
  {
    void Update(GameClock clock);
  }
}
