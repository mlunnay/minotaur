using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Core
{
  public class GameClock
  {
    public bool Paused { get; set; }
    public float TimeScale { get; set; }
    public double ElapsedMilliseconds { get; set; }
    public double TotalMilliseconds { get; set; }
    public double ElapsedSeconds { get; private set; }
    public double TotalSeconds { get; private set; }

    public GameClock()
    {
      Paused = false;
      TimeScale = 1.0f;
      ElapsedMilliseconds = 0f;
      TotalMilliseconds = 0f;
    }

    public GameClock Clone()
    {
      GameClock clock = new GameClock();
      clock.Paused = Paused;
      clock.TimeScale = TimeScale;
      clock.ElapsedMilliseconds = ElapsedMilliseconds;
      clock.TotalMilliseconds = TotalMilliseconds;
      return clock;
    }

    public void Reset()
    {
      ElapsedMilliseconds = 0f;
      TotalMilliseconds = 0f;
    }

    public void Update(double elapsed)
    {
      ElapsedMilliseconds = Paused ? 0.0f : elapsed * TimeScale;
      TotalMilliseconds += ElapsedMilliseconds;
      ElapsedSeconds = ElapsedMilliseconds / 1000;
      TotalSeconds = TotalMilliseconds / 1000;
    }
  }
}
