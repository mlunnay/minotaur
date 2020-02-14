using OpenTK;
using System.Collections.Generic;

namespace Minotaur.Graphics.Animation
{
  public class BoneAnimationClip : GraphicsResource, IBoneAnimationClip
  {
    private List<List<IVectorKey>> _translationKeys;
    private List<List<IQuaternionKey>> _rotationKeys;
    private List<List<IVectorKey>> _scaleKeys;
    protected float _framesPerSecond;
    protected ushort _totalFrames;
    protected KeyframeEnding _preferedEnding;

    public float FramesPerSecond
    {
      get { return _framesPerSecond; }
    }

    public ushort TotalFrames
    {
      get { return _totalFrames; }
    }

    public KeyframeEnding PreferedEnding
    {
      get { return _preferedEnding; }
    }

    public string Name { get; set; }

    public BoneAnimationClip(float framesPerSecond, ushort totalFrames, KeyframeEnding preferedEnding = KeyframeEnding.Clamp,
      List<List<IVectorKey>> translationKeys = null, List<List<IQuaternionKey>> rotationKeys = null, List<List<IVectorKey>> scaleKeys = null)
    {
      _framesPerSecond = framesPerSecond;
      _totalFrames = totalFrames;
      _preferedEnding = preferedEnding;

      if (translationKeys == null)
        _translationKeys = new List<List<IVectorKey>>();
      else
        _translationKeys = translationKeys;

      if (rotationKeys == null)
        _rotationKeys = new List<List<IQuaternionKey>>();
      else
        _rotationKeys = rotationKeys;

      if (scaleKeys == null)
        _scaleKeys = new List<List<IVectorKey>>();
      else
        _scaleKeys = scaleKeys;
    }

    public bool GetTransforms(int bone, float animationTime, out Vector3 translation, out Quaternion rotation, out Vector3 scale)
    {
      int index;
      float delta;
      float factor;

      bool hasTranslation = true;
      bool hasRotation = true;
      bool hasScale = true;

      if (bone >= _translationKeys.Count || _translationKeys[bone] == null)
      {
        translation = Vector3.Zero;
        hasTranslation = false;
      }
      else if (_translationKeys[bone].Count == 1)
        translation = _translationKeys[bone][0].Vector;
      else
      {
        List<IVectorKey> translationKeys = _translationKeys[bone];
        index = GetTranslationKey(translationKeys, animationTime);
        delta = translationKeys[index + 1].Time - translationKeys[index].Time;
        factor = (animationTime - translationKeys[index].Time) / delta;
        translation = Vector3.Lerp(translationKeys[index].Vector, translationKeys[index + 1].Vector, factor);
      }

      if (bone >= _rotationKeys.Count || _rotationKeys[bone] == null)
      {
        rotation = Quaternion.Identity;
        hasRotation = false;
      }
      else if (_rotationKeys[bone].Count == 1)
        rotation = _rotationKeys[bone][0].Rotation;
      else
      {
        List<IQuaternionKey> rotationKeys = _rotationKeys[bone];
        index = GetRotationKey(rotationKeys, animationTime);
        delta = rotationKeys[index + 1].Time - rotationKeys[index].Time;
        factor = (animationTime - rotationKeys[index].Time) / delta;
        rotation = Quaternion.Slerp(rotationKeys[index].Rotation, rotationKeys[index + 1].Rotation, factor);
      }

      if (bone >= _scaleKeys.Count || _scaleKeys[bone] == null)
      {
        scale = Vector3.One;
        hasScale = false;
      }
      else if (_scaleKeys[bone].Count == 1)
        scale = _scaleKeys[bone][0].Vector;
      else
      {
        List<IVectorKey> scaleKeys = _scaleKeys[bone];
        index = GetScaleKey(scaleKeys, animationTime);
        delta = scaleKeys[index + 1].Time - scaleKeys[index].Time;
        factor = (animationTime - scaleKeys[index].Time) / delta;
        scale = Vector3.Lerp(scaleKeys[index].Vector, scaleKeys[index + 1].Vector, factor);
      }

      return hasTranslation && hasRotation && hasScale;
    }

    private int GetTranslationKey(List<IVectorKey> translationKeys, float time)
    {
      for (int i = 0; i < translationKeys.Count - 1; i++)
      {
        if (time < translationKeys[i + 1].Time)
          return i;
      }
      return -1;
    }

    private int GetRotationKey(List<IQuaternionKey> rotationKeys, float time)
    {
      for (int i = 0; i < rotationKeys.Count - 1; i++)
      {
        if (time < rotationKeys[i + 1].Time)
          return i;
      }
      return -1;
    }

    private int GetScaleKey(List<IVectorKey> scaleKeys, float time)
    {
      for (int i = 0; i < scaleKeys.Count - 1; i++)
      {
        if (time < scaleKeys[i + 1].Time)
          return i;
      }
      return -1;
    }
  }
}
