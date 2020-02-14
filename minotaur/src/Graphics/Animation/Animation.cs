using System;
using Minotaur.Core;

namespace Minotaur.Graphics.Animation
{
  public abstract class Animation : IAnimation, IUpdateable
  {
    public AnimationState State { get; private set; }

    public event EventHandler Completed;
    public event EventHandler Started;
    public event EventHandler Stopped;
    public event EventHandler Paused;
    public event EventHandler Resumed;

    public void Play()
    {
      State = AnimationState.Playing;
      OnStarted();
    }

    public void Stop()
    {
      State = AnimationState.Stopped;
      OnStopped();
    }

    public void Pause()
    {
      if (State == AnimationState.Playing)
      {
        State = AnimationState.Paused;
        OnPaused();
      }
    }

    public void Resume()
    {
      if (State == AnimationState.Paused)
      {
        State = AnimationState.Playing;
        OnResumed();
      }
    }

    public abstract void Update(Core.GameClock gameclock);

    protected virtual void OnStarted()
    {
      if (Started != null)
        Started(this, EventArgs.Empty);
    }

    protected virtual void OnStopped()
    {
      if (Stopped != null)
        Stopped(this, EventArgs.Empty);
    }

    protected virtual void OnPaused()
    {
      if (Paused != null)
        Paused(this, EventArgs.Empty);
    }

    protected virtual void OnResumed()
    {
      if (Resumed != null)
        Resumed(this, EventArgs.Empty);
    }

    protected virtual void OnCompleted()
    {
      if (Completed != null)
        Completed(this, EventArgs.Empty);
    }
  }
}
