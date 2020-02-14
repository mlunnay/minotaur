using System;

namespace Minotaur.Graphics.Animation
{
  /// <summary>
  /// Defines the behavior of the last ending keyframe.
  /// </summary>
  /// <remarks>
  /// The difference between these behaviors won't be noticeable
  /// unless the KeyframeAnimation is really slow.
  /// </remarks>
  public enum KeyframeEnding
  {
    /// <summary>
    /// The animation will wait for the last frame to finish
    /// but won't blend the last frame with the first frame.
    /// Specify this when your animation isn't looped.
    /// </summary>
    Clamp,

    /// <summary>
    /// The animation will blend between the last keyframe
    /// and the first keyframe. 
    /// Specify this when the animation is looped and the first
    /// frame doesn't equal to the last frame.
    /// </summary>
    Wrap,

    /// <summary>
    /// The animation will stop immediately when it reaches
    /// the last frame, so the ending frame has no duration.
    /// Specify this when the animation is looped and the first
    /// frame is identical to the last frame.
    /// </summary>
    Discard,
  }

  public class KeyframeEventArgs : EventArgs
  {
    /// <summary>
    /// gets the index of the frame.
    /// </summary>
    public int Frame { get; internal set; }
  }

  /// <summary>
  /// Base class for all keyframed animatins.
  /// </summary>
  public abstract class KeyframeAnimation : TimelineAnimation
  {
    private int _currentFrame;
    private float _framesPerSecond = 24;
    private int _totalFrames;
    private KeyframeEnding _ending = KeyframeEnding.Clamp;
    // used to prevent from allways calling exit frame before enter frame on the first frame.
    private bool _hasPlayed;

    /// <summary>
    /// Triggers when this animation has just entered the current frame.
    /// </summary>
    public event EventHandler<KeyframeEventArgs> EnterFrame;
    /// <summary>
    /// Triggers when this animation is about to exit the current frame.
    /// </summary>
    public event EventHandler<KeyframeEventArgs> ExitFrame;

    public KeyframeEnding Ending
    {
      get { return _ending; }
      set
      {
        _ending = value;
        UpdateDuration();
      }
    }

    public int TotalFrames
    {
      get { return _totalFrames; }
      set
      {
        _totalFrames = value;
        UpdateDuration();
      }
    }

    public float FramesPerSecond
    {
      get { return _framesPerSecond; }
      set
      {
        _framesPerSecond = value;
        UpdateDuration();
      }
    }

    public int CurrentFrame { get { return _currentFrame; } }

    /// <summary>
    /// Gets or sets the frame at which this <see cref="KeyframeAnimation"/> should begin.
    /// </summary>
    public int? BeginFrame
    {
      get
      {
        if (BeginTime.HasValue)
          return (int)(BeginTime.Value.TotalSeconds * _totalFrames / TotalDuration.TotalSeconds);
        return null;
      }
      set
      {
        if (value.HasValue)
          BeginTime = TimeSpan.FromSeconds(value.Value * TotalDuration.TotalSeconds / _totalFrames);
        else
          BeginTime = null;
      }
    }

    /// <summary>
    /// Gets or sets the frame at which this <see cref="KeyframeAnimation"/> should end.
    /// </summary>
    public int? EndFrame
    {
      get
      {
        if (EndTime.HasValue)
          return (int)(EndTime.Value.TotalSeconds * _totalFrames / TotalDuration.TotalSeconds);
        return null;
      }
      set
      {
        if (value.HasValue)
          EndTime = TimeSpan.FromSeconds(value.Value * TotalDuration.TotalSeconds / _totalFrames);
        else
          EndTime = null;
      }
    }

    protected KeyframeAnimation()
    {
      _currentFrame = 0;
    }

    public void Seek(int frame)
    {
      if (frame < 0 || frame >= _totalFrames)
        throw new ArgumentOutOfRangeException("frame");

      Seek(TimeSpan.FromSeconds(1.0 * frame / _framesPerSecond));
    }

    protected override void OnStopped()
    {
      _hasPlayed = false;
      base.OnStopped();
    }

    protected override void OnStarted()
    {
      _hasPlayed = false;
      base.OnStarted();
    }

    protected override void OnCompleted()
    {
      _hasPlayed = false;
      base.OnCompleted();
    }

    protected override void OnSeek(TimeSpan position, TimeSpan previousPosition)
    {
      float percentage;
      int current, previous;
      GetFrame(previousPosition, out previous, out percentage);
      GetFrame(position, out current, out percentage);

      if (_ending == KeyframeEnding.Wrap)
        OnSeek(current, (current + 1) % _totalFrames, percentage);
      else
        OnSeek(current, Math.Min(current + 1, _totalFrames - 1), percentage);

      if (current != previous || !_hasPlayed)
      {
        if (_hasPlayed)
          OnExitFrame(previous);

        _hasPlayed = true;

        _currentFrame = current;
        OnEnterFrame(current);
      }
    }

    /// <summary>
    /// Moves the animation at the position between start frame and end frame specified by percentage.
    /// </summary>
    /// <param name="startFrame"></param>
    /// <param name="endFrame"></param>
    /// <param name="percentage"></param>
    protected virtual void OnSeek(int startFrame, int endFrame, float percentage) { }

    protected virtual void OnEnterFrame(int frame)
    {
      if (EnterFrame != null)
        EnterFrame(this, new KeyframeEventArgs() { Frame = frame });
    }

    protected virtual void OnExitFrame(int frame)
    {
      if (ExitFrame != null)
        ExitFrame(this, new KeyframeEventArgs() { Frame = frame });
    }

    private void GetFrame(TimeSpan position, out int frame, out float percentage)
    {
      if (position < TimeSpan.Zero || position > TotalDuration)
        throw new ArgumentOutOfRangeException("position");

      frame = (int)(position.TotalSeconds * _framesPerSecond);
      percentage = (float)(position.TotalSeconds * _framesPerSecond - frame);

      if (frame >= _totalFrames)
      {
        frame = 0;
        percentage = 0f;
      }
    }

    private void UpdateDuration()
    {
      int realFrames = Math.Max(0, _ending == KeyframeEnding.Discard ? _totalFrames - 1 : _totalFrames);
      TotalDuration = TimeSpan.FromSeconds(realFrames / _framesPerSecond);
    }
  }
}
