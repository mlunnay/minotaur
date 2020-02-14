using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using Minotaur.Core;
using Minotaur.Helpers;

namespace Minotaur.Graphics.Animation
{
  public class BoneAnimation : Animation, IBoneAnimationController
  {
    private struct BoneAnimationItem
    {
      public float BlendWeight;
      public Matrix4 Transform;
    }

    private Skeleton _skeleton;
    private BoneAnimationControllerCollection _controllers;
    private ITimelineAnimation _keyController;
    private bool _isSychronized;
    private BoneAnimationItem[] _weightedBones = null;

    public Skeleton Skeletong { get { return _skeleton; } }

    public BoneAnimationControllerCollection Controllers { get { return _controllers; } }

    public ITimelineAnimation KeyController
    {
      get { return _keyController; }
      set
      {
        if (State != AnimationState.Stopped)
          throw new InvalidOperationException("Cannot modify the BoneAnimation when the animation is being played.");
        if (value != null && !_controllers.Any(c => c == value))
          throw new ArgumentException("The specified controller is not contained in this animation.");

        _keyController = value;
      }
    }

    public bool IsSychronized
    {
      get { return _isSychronized; }
      set
      {
        if (State != AnimationState.Stopped)
          throw new InvalidOperationException("Cannot modify the BoneAnimation when the animation is being played.");

        _isSychronized = value;
      }
    }

    public BoneAnimation(Skeleton skeleton)
    {
      if (skeleton == null)
        throw new ArgumentNullException("skeleton");
      _skeleton = skeleton;
      _controllers = new BoneAnimationControllerCollection(skeleton);
    }

    public BoneAnimation(Skeleton skeleton, BoneAnimationClip animationClip)
      : this(skeleton, new BoneAnimationController(animationClip)) { }

    public BoneAnimation(Skeleton skeleton, IBoneAnimationController animationController)
      : this(skeleton)
    {
      if (animationController == null)
        throw new ArgumentNullException("animationController");

      _controllers.Add(animationController);
    }

    protected override void OnStarted()
    {
      if (_controllers.Count > 0 && _weightedBones == null)
        _weightedBones = new BoneAnimationItem[_controllers.Count * _skeleton.BoneTransforms.Length];

      SychronizeSpeed();

      foreach (IBoneAnimationController controller in _controllers)
      {
        IAnimation animation = controller as IAnimation;
        if (animation != null)
          animation.Play();
      }

      base.OnStarted();
    }

    protected override void OnStopped()
    {
      foreach (IBoneAnimationController controller in _controllers)
      {
        IAnimation animation = controller as IAnimation;
        if (animation != null)
          animation.Stop();
      }

      base.OnStopped();
    }

    protected override void OnResumed()
    {
      foreach (IBoneAnimationController controller in _controllers)
      {
        IAnimation animation = controller as IAnimation;
        if (animation != null)
          animation.Resume();
      }
      
      base.OnResumed();
    }

    public override void Update(GameClock gameclock)
    {
      if (State == AnimationState.Playing && !gameclock.Paused)
      {
        UpdateControllers(gameclock);
        UpdateBoneTransforms(gameclock);
      }
    }

    public void TryGetBoneTransforms(int bone, out Vector3 translation, out Quaternion rotation, out Vector3 scale, out float blendWeight)
    {
      throw new NotImplementedException();
    }

    public void TryGetGoneTransforms(int bone, out Matrix4 transform, out float blendWeight)
    {
      Vector3 translation;
      Quaternion rotation;
      Vector3 scale;

      TryGetBoneTransforms(bone, out translation, out rotation, out scale, out blendWeight);

      transform = Matrix4.Translation(translation) * Matrix4.Rotate(rotation) * Matrix4.Scale(scale);
    }

    private void UpdateControllers(GameClock gameclock)
    {
      SychronizeSpeed();

      for (int i = 0; i < _controllers.Count; i++)
      {
        IUpdateable update = _controllers[i].Controller as IUpdateable;
        if (update != null)
          update.Update(gameclock);
      }

      bool allStopped = true;
      if (_keyController != null)
        allStopped = _keyController.State == AnimationState.Stopped;
      else
      {
        for (int i = 0; i < _controllers.Count; i++)
        {
          IAnimation animation = _controllers[i].Controller as IAnimation;
          if (animation == null || animation.State != AnimationState.Stopped)
            allStopped = false;
        }
      }

      if (allStopped)
      {
        Stop();
        OnCompleted();
      }
    }

    private void SychronizeSpeed()
    {
      if (_isSychronized && _keyController != null)
      {
        TimeSpan duration = _keyController.Duration;
        for (int i = 0; i < _controllers.Count; i++)
        {
          IBoneAnimationController controller = _controllers[i].Controller;
          if (controller == _keyController)
            continue;

          ITimelineAnimation animation = controller as ITimelineAnimation;
          if (animation == null)
            continue;

          animation.Speed = (float)((double)animation.Duration.Ticks * _keyController.Speed / duration.Ticks);
        }
      }
    }

    private void UpdateBoneTransforms(GameClock gameclock)
    {
      // update controller transforms
      Array.Clear(_weightedBones, 0, _weightedBones.Length);

      int numBones = _skeleton.BoneTransforms.Length;
      for (int i = 0; i < _controllers.Count; i++)
      {
        for (int b = 0; b < numBones; b++)
        {
          int index = i * numBones + b;
          if (_controllers[i].Controller.TryGetGoneTransforms(b, out _weightedBones[index].Transform, out _weightedBones[index].BlendWeight))
            _weightedBones[index].BlendWeight *= _controllers[i].BlendWeight * (_controllers[i].BoneWeights[b].Enabled ? _controllers[i].BoneWeights[b].BlendWeight : 0);
          else
            _weightedBones[index].BlendWeight = 0;
        }
      }

      // Normalize weights
      for (int bone = 0; bone < numBones; bone++)
      {
        int firstNonZeroChannel = -1;
        float totalWeight = 0;

        for (int i = 0; i < _controllers.Count; i++)
        {
          int index = i * numBones + bone;
          if (firstNonZeroChannel < 0 && _weightedBones[index].BlendWeight > 0)
            firstNonZeroChannel = i;

          totalWeight += _weightedBones[i].BlendWeight;
        }

        if (totalWeight <= 0)
          continue;

        // ToDo: Modify to use seperate translation, rotation and scale.
        Matrix4 transform = _weightedBones[firstNonZeroChannel * numBones + bone].Transform;

        for (int i = 0; i < _controllers.Count; i++)
        {
          int index = i * numBones + bone;
          if (_weightedBones[index].BlendWeight <= float.Epsilon)
            continue;

          // this is not matmatically correct, but produces an acceptable result.
          transform = LerpHelper.Slerp(transform, _weightedBones[index].Transform,
            _weightedBones[index].BlendWeight / totalWeight);
        }

        _skeleton.BoneTransforms[bone] = transform;
      }
    }
  }
}
