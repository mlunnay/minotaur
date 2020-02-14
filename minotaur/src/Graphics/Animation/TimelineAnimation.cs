using System;
using Minotaur.Core;

namespace Minotaur.Graphics.Animation
{
  /// <summary>
  /// Defines whether the animation is playing forward or backward.
  /// </summary>
  public enum AnimationDirection
  {
    /// <summary>
    /// The animation is playing forward.
    /// </summary>
    Forward,

    /// <summary>
    /// The animation is playing backward.
    /// </summary>
    Backward,
  }

  public abstract class TimelineAnimation : Animation, ITimelineAnimation
  {
    private TimeSpan _totalDuration;
    private bool _durationNeedsUpdate;
    private TimeSpan? _beginTime;
    private TimeSpan? _endTime;
    private TimeSpan _duration;
    private float _speed = 1.0f;
    private float _repeat = 1.0f;
    private TimeSpan _position = TimeSpan.Zero;
    private float _phase;
    private TimeSpan _elapsedTime;

    /// <summary>
    /// Triggered when this animatino has reached the end and is starting to repeat.
    /// </summary>
    public event EventHandler Repeated;

    public TimeSpan TotalDuration
    {
      get { return _totalDuration; }
      set
      {
        if (value < TimeSpan.Zero)
          throw new ArgumentOutOfRangeException("TotalDuration cannot be negative.");
        _totalDuration = value;
        _durationNeedsUpdate = true;
      }
    }

    public TimeSpan Duration
    {
      get
      {
        if (_durationNeedsUpdate)
        {
          // clamp begin and end times
          if (_beginTime != null && _beginTime.Value < TimeSpan.Zero)
            _beginTime = TimeSpan.Zero;
          if (_endTime != null && _endTime.Value > _totalDuration)
            _endTime = _totalDuration;

          // recompute duration
          _duration = (_endTime.HasValue ? _endTime.Value : _totalDuration) -
            (_beginTime.HasValue ? _beginTime.Value : TimeSpan.Zero);
          _durationNeedsUpdate = false;
        }
        return _duration;
      }
    }

    public TimeSpan? BeginTime
    {
      get { return _beginTime; }
      set
      {
        _beginTime = value;
        _durationNeedsUpdate = true;
      }
    }

    public TimeSpan? EndTime
    {
      get { return _endTime; }
      set
      {
        _endTime = value;
        _durationNeedsUpdate = true;
      }
    }

    public float Speed
    {
      get { return _speed; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("Speed must be greater than zero.");
        _speed = value;
      }
    }

    /// <summary>
    /// Gets the elapsed time since the animation started playing.
    /// Accumulates on each update, and updated by seek to ensure the animation
    /// stops at the right time.
    /// </summary>
    public TimeSpan ElapsedTime
    {
      get { return _elapsedTime; }
      set
      {
        if (value == _elapsedTime)
          return;
        _elapsedTime = value;
        _phase = (float)(_elapsedTime.TotalSeconds % _totalDuration.TotalSeconds);
      }
    }

    /// <summary>
    /// Tells whether the animation should automatically play backwards after it reaches the end.
    /// </summary>
    public bool AutoReverse { get; set; }

    /// <summary>
    /// Tells wheter the animation is playing forward of backward on startup.
    /// </summary>
    public AnimationDirection StartupDirection { get; set; }

    /// <summary>
    /// Sets or gets the direction the animation is currently playing.
    /// </summary>
    public AnimationDirection Direction { get; set; }

    public float Phase
    {
      get { return _phase; }
      set
      {
        if (value == _phase)
          return;
        _phase = value;
        ElapsedTime = TimeSpan.FromSeconds(_duration.TotalSeconds / _phase);
      }
    }

    /// <summary>
    /// Gets or sets the number of times this animation will be played.
    /// When set to a fractional value, the animation will be stopped and completed part way.
    /// Float.MaxValue means play forever. The default is 1.
    /// </summary>
    public float Repeat
    {
      get { return _repeat; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("Repeat must be greater than zero.");
        _repeat = value;
      }
    }

    public TimeSpan Position
    {
      get { return _position; }
      set { _position = value; }
    }

    protected TimelineAnimation()
    {
      ElapsedTime = TimeSpan.Zero;
      StartupDirection = AnimationDirection.Forward;
    }

    public void Seek(TimeSpan position)
    {
      if (position < TimeSpan.Zero || position > _totalDuration)
        throw new ArgumentOutOfRangeException("position");
      if (_position == position)
        return;
      TimeSpan previousPosition = _position;
      _position = position;
      ElapsedTime += Direction == AnimationDirection.Forward ? _position - previousPosition : previousPosition - _position;
      OnSeek(_position, previousPosition);
    }

    public override void Update(GameClock gameclock)
    {
      if (State != AnimationState.Playing || gameclock.Paused)
        return;

      TimeSpan increment = TimeSpan.FromSeconds(gameclock.TotalSeconds * (double)Speed);

      // repeat sets an absolute upper bound on how long the animation may run.
      TimeSpan max_elapsed = TimeSpan.MaxValue;
      double maxSeconds = _repeat * _duration.TotalSeconds;
      if (max_elapsed.TotalSeconds > maxSeconds)
        max_elapsed = TimeSpan.FromSeconds(maxSeconds);

      if (_elapsedTime + increment > max_elapsed)
        increment = max_elapsed - _elapsedTime;

      TimeSpan beginPosition = GetBeginPosition();
      TimeSpan endPosition = GetEndPosition();

      // Loop to handle reverse or repeat without losing excess time.
      while (increment > TimeSpan.Zero && _elapsedTime < max_elapsed)
      {
        // update but only by enough to stay within the allowed range.
        TimeSpan previousPosition = _position;
        if (Direction == AnimationDirection.Forward)
          _position = _position + increment > endPosition ? endPosition : _position + increment;
        else
          _position = _position - increment > beginPosition ? beginPosition : _position - increment;

        TimeSpan partInc = (_position - previousPosition).Duration();
        ElapsedTime += partInc;
        increment -= partInc;

        // notify new position
        OnSeek(_position, previousPosition);

        // If time left and not complete, then reverse or repeat and notify of that too.
        if (increment > TimeSpan.Zero && _elapsedTime < max_elapsed)
        {
          if (AutoReverse)
            Direction = Direction == AnimationDirection.Forward ? AnimationDirection.Backward : AnimationDirection.Forward;
          else
          {
            previousPosition = _position;
            _position = Direction == AnimationDirection.Forward ? beginPosition : endPosition;
            OnSeek(_position, previousPosition);
          }
          OnRepeated();
        }
        if (!(_elapsedTime < max_elapsed))
        {
          Stop();
          OnCompleted();
        }
      }
    }

    protected abstract void OnSeek(TimeSpan position, TimeSpan previousPosition);

    protected virtual void OnRepeated()
    {
      if (Repeated != null)
        Repeated(this, EventArgs.Empty);
    }

    protected override void OnStarted()
    {
      Direction = StartupDirection;
      ElapsedTime = TimeSpan.Zero;
      _position = Direction == AnimationDirection.Forward ? GetBeginPosition() : GetEndPosition();
      base.OnStarted();
    }

    private TimeSpan GetBeginPosition()
    {
      return _beginTime.HasValue ? _beginTime.Value : TimeSpan.Zero;
    }

    private TimeSpan GetEndPosition()
    {
      return _endTime.HasValue ? _endTime.Value : _totalDuration;
    }
  }
}
