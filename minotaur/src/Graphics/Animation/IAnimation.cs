using System;

namespace Minotaur.Graphics.Animation
{
  /// <summary>
  /// Current state (playing, paused, or stopped) of an animation.
  /// </summary>
  public enum AnimationState
  {
    /// <summary>
    /// The animation is stopped.
    /// </summary>
    Stopped,

    /// <summary>
    /// The animation is playing.
    /// </summary>
    Playing,

    /// <summary>
    /// The animation is paused.
    /// </summary>
    Paused,
  }

  public interface IAnimation
  {
    /// <summary>
    /// The current state of the animation.
    /// </summary>
    AnimationState State { get; }

    /// <summary>
    /// Plays the animation from the start position.
    /// </summary>
    void Play();

    /// <summary>
    /// Stops the animation.
    /// </summary>
    void Stop();

    /// <summary>
    /// Pauses the animation.
    /// </summary>
    void Pause();

    /// <summary>
    /// Resumes playing the animation from the current position.
    /// </summary>
    void Resume();

    /// <summary>
    /// Event triggered when this animation has finished playing.
    /// </summary>
    event EventHandler Completed;
  }

  /// <summary>
  /// Interface for timeline based animations.
  /// </summary>
  public interface ITimelineAnimation : IAnimation
  {
    /// <summary>
    /// The running time for a single pass of a timeline animation.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets the current position of the animatino playback since the start position.
    /// </summary>
    TimeSpan Position { get; }

    /// <summary>
    /// The current position normalised to the duration of this clip in the range [0, 1].
    /// </summary>
    float Phase { get; }

    /// <summary>
    /// Retrieves or modifies the playing speed of the timeline animation.
    /// Multiplies the number of clock ticks on each update.
    /// </summary>
    float Speed { get; set; }
  }
}
