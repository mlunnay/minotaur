using System;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public class BoneAnimationController : TimelineAnimation, IBoneAnimationController
  {
    private KeyframeEnding _ending;
    private TimeSpan _beginTime;
    private TimeSpan _endTime;
    private double _elapsedSeconds;

    public BoneAnimationClip AnimationClip { get; private set; }

    public KeyframeEnding Ending
    {
      get { return _ending; }
      set
      {
        _ending = value;
        UpdateDuration();
      }
    }

    public ushort TotalFrames
    {
      get
      {
        return AnimationClip.TotalFrames;
      }
    }

    public float FramesPerSecond { get; set; }

    public BoneAnimationController(BoneAnimationClip animationClip)
    {
      AnimationClip = animationClip;
      FramesPerSecond = animationClip.FramesPerSecond;
      _ending = animationClip.PreferedEnding;
      UpdateDuration();
    }

    public BoneAnimationController(BoneAnimationClip animationClip, TimeSpan beginTime, float length)
      : this(animationClip)
    {
      BeginTime = beginTime;
      EndTime = beginTime.Add(TimeSpan.FromSeconds(length));
    }

    public BoneAnimationController(BoneAnimationClip animationClip, TimeSpan beginTime, TimeSpan endTime)
      : this(animationClip)
    {
      if (endTime < beginTime)
        throw new ArgumentException("beginTime must be less than endTime.");
      BeginTime = beginTime;
      EndTime = endTime;
    }

    public override void Update(Core.GameClock gameclock)
    {
      _elapsedSeconds = gameclock.ElapsedSeconds;
      base.Update(gameclock);
    }

    protected override void OnSeek(TimeSpan position, TimeSpan previousPosition)
    {
    }

    public bool TryGetBoneTransforms(int bone, out Vector3 translation, out Quaternion rotation, out Vector3 scale, out float blendWeight)
    {
      blendWeight = 1f;
      return AnimationClip.GetTransforms(bone, (float)_elapsedSeconds, out translation, out rotation, out scale);
    }

    public bool TryGetGoneTransforms(int bone, out Matrix4 transform, out float blendWeight)
    {
      Vector3 translation;
      Quaternion rotation;
      Vector3 scale;

      bool result = TryGetBoneTransforms(bone, out translation, out rotation, out scale, out blendWeight);

      transform = Matrix4.CreateTranslation(translation) * Matrix4.Rotate(rotation) * Matrix4.Scale(scale);

      return result;
    }

    private void UpdateDuration()
    {
      int realFrames = Math.Max(0, _ending == KeyframeEnding.Discard ? TotalFrames - 1 : TotalFrames);
      TotalDuration = TimeSpan.FromSeconds(realFrames / FramesPerSecond);
    }
  }
}
