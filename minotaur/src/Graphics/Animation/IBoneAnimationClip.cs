using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public interface IBoneAnimationClip
  {
    float FramesPerSecond { get; }
    ushort TotalFrames { get; }
    KeyframeEnding PreferedEnding { get; }
    bool GetTransforms(int bone, float animationTime, out Vector3 translation, out Quaternion rotation, out Vector3 scale);
  }
}
