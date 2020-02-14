using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Minotaur.Graphics.Animation
{
  public class WeightedBoneAnimationController
  {
    internal Skeleton Skeleton;

    public IBoneAnimationController Controller { get; private set; }

    public float BlendWeight { get; set; }

    public WeightedBoneAnimationControllerBoneCollection BoneWeights { get; private set; }

    internal WeightedBoneAnimationController(Skeleton skeleton, IBoneAnimationController controller)
    {
      if (controller == null)
        throw new ArgumentNullException("controller");
      if (skeleton == null)
        throw new ArgumentNullException("skeleton");

      List<WeightedBoneAnimationControllerBone> bones = new List<WeightedBoneAnimationControllerBone>(skeleton.Bones.Count);
      for (int i = 0; i < skeleton.Bones.Count; i++)
        bones.Add(new WeightedBoneAnimationControllerBone());

      Skeleton = skeleton;
      Controller = controller;
      BlendWeight = 1;
      BoneWeights = new WeightedBoneAnimationControllerBoneCollection(this, bones);
    }

    public void EnableAll()
    {
      for (int i = 0; i < BoneWeights.Count; i++)
        BoneWeights[i].Enabled = true;
    }

    public void DisableAll()
    {
      for (int i = 0; i < BoneWeights.Count; i++)
        BoneWeights[i].Enabled = false;
    }

    public void Enable(int bone, bool enableChildBones)
    {
      SetEnabled(bone, true, enableChildBones);
    }

    public void Enable(string bone, bool enableChildBones)
    {
      SetEnabled(Skeleton.Bones[bone].Index, true, enableChildBones);
    }

    public void Disable(int bone, bool enableChildBones)
    {
      SetEnabled(bone, false, enableChildBones);
    }

    public void Disable(string bone, bool enableChildBones)
    {
      SetEnabled(Skeleton.Bones[bone].Index, false, enableChildBones);
    }

    private void SetEnabled(int bone, bool enabled, bool recursive)
    {
      BoneWeights[bone].Enabled = enabled;
      if (recursive)
      {
        foreach (int child in Skeleton.GetChildBones(bone))
          SetEnabled(child, enabled, true);
      }
    }
  }

  public class WeightedBoneAnimationControllerBone
  {
    public float BlendWeight;

    public bool Enabled;

    internal WeightedBoneAnimationControllerBone()
    {
      BlendWeight = 1;
      Enabled = true;
    }
  }

  public class WeightedBoneAnimationControllerBoneCollection : ReadOnlyCollection<WeightedBoneAnimationControllerBone>
  {
    private WeightedBoneAnimationController _animation;

    internal WeightedBoneAnimationControllerBoneCollection(WeightedBoneAnimationController animation, IList<WeightedBoneAnimationControllerBone> bones)
      : base(bones)
    {
      _animation = animation;
    }

    public WeightedBoneAnimationControllerBone this[string name]
    {
      get { return this[_animation.Skeleton.Bones[name].Index]; }
    }
  }

  public class BoneAnimationControllerCollection : ICollection<IBoneAnimationController>
  {
    private Skeleton _skeleton;
    private List<WeightedBoneAnimationController> _controllers = new List<WeightedBoneAnimationController>();

    public WeightedBoneAnimationController this[int index]
    {
      get { return _controllers[index]; }
    }

    public WeightedBoneAnimationController this[IBoneAnimationController item]
    {
      get { return _controllers.FirstOrDefault(c => c.Controller == item); }
    }

    public int Count
    {
      get { return _controllers.Count; }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    internal BoneAnimationControllerCollection(Skeleton skeleton)
    {
      _skeleton = skeleton;
    }

    public void Add(IBoneAnimationController item)
    {
      Add(item, 1.0f);
    }

    public void Add(IBoneAnimationController item, float blendWeight)
    {
      _controllers.Add(new WeightedBoneAnimationController(_skeleton, item) { BlendWeight = blendWeight });
    }

    public void Clear()
    {
      _controllers.Clear();
    }

    public bool Contains(IBoneAnimationController item)
    {
      return _controllers.Any(c => c.Controller == item);
    }

    public void CopyTo(IBoneAnimationController[] array, int arrayIndex)
    {
      foreach (WeightedBoneAnimationController controller in _controllers)
        array[arrayIndex++] = controller.Controller;
    }

    public bool Remove(IBoneAnimationController item)
    {
      for (int i = 0; i < _controllers.Count; i++)
      {
        if (_controllers[i].Controller == item)
        {
          _controllers.RemoveAt(i);
          return true;
        }
      }
      return false;
    }

    public IEnumerator<IBoneAnimationController> GetEnumerator()
    {
      return _controllers.Select(c => c.Controller).GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}
