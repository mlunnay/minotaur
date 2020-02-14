using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK;

namespace Minotaur.Graphics
{
  public class Bone
  {
    private List<Bone> _children = new List<Bone>();

    public int Index { get; set; }
    public string Name { get; set; }
    public Bone Parent { get; set; }
    public BoneCollection Children { get; private set; }
    public Matrix4 Transform { get; set; }

    public Bone(int index, string name, Matrix4 transform)
    {
      Index = index;
      Name = name;
      Transform = transform;
      Children = new BoneCollection(new List<Bone>());
    }

    public void SetParentAndChildren(Bone parent, Bone[] children)
    {
      Parent = parent;
      Children = new BoneCollection(children);
    }

    public void AddChild(Bone child)
    {
      List<Bone> children = Children.ToList();
      children.Add(child);
      Children = new BoneCollection(children);
    }
  }

  public class BoneCollection : ReadOnlyCollection<Bone>
  {
    public BoneCollection(IList<Bone> list)
      : base(list) { }

    public Bone this[string name]
    {
      get
      {
        Bone bone;
        if (TryGetValue(name, out bone))
          return bone;
        throw new KeyNotFoundException();
      }
    }

    public bool TryGetValue(string name, out Bone value)
    {
      foreach (Bone bone in Items)
      {
        if (bone.Name == name)
        {
          value = bone;
          return true;
        }
      }
      value = null;
      return false;
    }
  }

  /// <summary>
  /// A bone heirachy that can be animated.
  /// </summary>
  public class Skeleton
  {
    protected BoneCollection _bones;

    private Matrix4[] _boneTransforms;

    public BoneCollection Bones { get { return _bones; } }

    public int SkeletonRoot { get; private set; }

    public Matrix4[] BoneTransforms { get { return _boneTransforms; } }

    public ReadOnlyCollection<Matrix4> InverseAbsoluteBindPose { get; private set; }

    public bool HasAnimated;

    public Skeleton(IList<Bone> bones, int skeletonRoot = -1, IList<Matrix4> inverseAbsoluteBindPose = null)
    {
      if (skeletonRoot >= 0 && inverseAbsoluteBindPose == null)
        throw new ArgumentException("inverseAsoluteBindPose must be non null is skeletonRoot is >= 0.");
      _bones = new BoneCollection(bones);
      SkeletonRoot = skeletonRoot;
      if (inverseAbsoluteBindPose != null)
        InverseAbsoluteBindPose = new ReadOnlyCollection<Matrix4>(inverseAbsoluteBindPose);
      _boneTransforms = _bones.Select(b => b.Transform).ToArray();
    }

    public Skeleton Clone()
    {
      Skeleton skeleton = new Skeleton(_bones.ToList(), SkeletonRoot, InverseAbsoluteBindPose.ToList());
      skeleton._boneTransforms = new Matrix4[_boneTransforms.Length];
      _boneTransforms.CopyTo(skeleton._boneTransforms, 0);
      return skeleton;
    }

    public IEnumerable<int> GetChildBones(int bone)
    {
      int count = _bones[bone].Children.Count;
      for (int i = 0; i < count; i++)
      {
        yield return _bones[bone].Children[i].Index;
      }
    }

    public Matrix4 GetAbsoluteBoneTransform(int boneIndex)
    {
      Bone bone = Bones[boneIndex];
      Matrix4 absoluteTransform = bone.Transform;
      while ((bone = bone.Parent) != null)
      {
        absoluteTransform = absoluteTransform * bone.Transform;
      }

      return absoluteTransform;
    }

    public void CopyAbsoluteBoneTransformsTo(Matrix4[] destinationBoneTransforms)
    {
      if (destinationBoneTransforms == null)
        throw new ArgumentNullException("destinationBoneTransforms");
      if(destinationBoneTransforms.Length < _bones.Count)
        throw new ArgumentOutOfRangeException("destinationBoneTransforms");

      for (int i = 0; i < _bones.Count; i++)
      {
        if (_bones[i].Parent == null)
          destinationBoneTransforms[i] = _boneTransforms[i];
        else
          destinationBoneTransforms[i] = _boneTransforms[i] * destinationBoneTransforms[_bones[i].Parent.Index];
      }
    }

    public void CopyBoneTransformsTo(Matrix4[] destinationBoneTransforms)
    {
      if (destinationBoneTransforms == null)
        throw new ArgumentNullException("destinationBoneTransforms");
      if (destinationBoneTransforms.Length < _bones.Count)
        throw new ArgumentOutOfRangeException("destinationBoneTransforms");

      for (int i = 0; i < _bones.Count; i++)
      {
        destinationBoneTransforms[i] = _boneTransforms[i];
      }
    }

    public Matrix4 GetBoneTransform(int bone)
    {
      return _boneTransforms[bone];
    }

    public Matrix4 GetBoneTransform(string bone)
    {
      return _boneTransforms[_bones[bone].Index];
    }

    public Matrix4[] GetSkinTransforms()
    {
      if (InverseAbsoluteBindPose == null)
        throw new NotSupportedException("Skeleton does not support skinning.");

      Matrix4[] skin = new Matrix4[InverseAbsoluteBindPose.Count];
      Matrix4[] bones = new Matrix4[_bones.Count];
      CopyAbsoluteBoneTransformsTo(bones);
      for (int i = 0; i < InverseAbsoluteBindPose.Count; i++)
      {
        skin[i] = InverseAbsoluteBindPose[i] * bones[SkeletonRoot + i];
      }

      return skin;
    }

    public Matrix4[] GetSkinTransforms(Matrix4 world)
    {
      if (InverseAbsoluteBindPose == null)
        throw new NotSupportedException("Skeleton does not support skinning.");

      Matrix4[] skin = new Matrix4[InverseAbsoluteBindPose.Count];
      Matrix4[] bones = new Matrix4[_bones.Count];
      CopyAbsoluteBoneTransformsTo(bones);
      for (int i = 0; i < InverseAbsoluteBindPose.Count; i++)
      {
        skin[i] = InverseAbsoluteBindPose[i] * bones[SkeletonRoot + i] * world;
      }

      return skin;
    }

    public void GetSkinTransforms(Matrix4[] skinTransform)
    {
      if (InverseAbsoluteBindPose == null)
        throw new NotSupportedException("Skeleton does not support skinning.");

      Matrix4[] bones = new Matrix4[_bones.Count];
      CopyAbsoluteBoneTransformsTo(bones);
      for (int i = 0; i < InverseAbsoluteBindPose.Count; i++)
      {
        skinTransform[i] = InverseAbsoluteBindPose[i] * bones[SkeletonRoot + i];
      }
    }

    public void GetSkinTransforms(Matrix4 world, Matrix4[] skinTransform)
    {
      if (InverseAbsoluteBindPose == null)
        throw new NotSupportedException("Skeleton does not support skinning.");

      Matrix4[] bones = new Matrix4[_bones.Count];
      CopyAbsoluteBoneTransformsTo(bones);
      for (int i = 0; i < InverseAbsoluteBindPose.Count; i++)
      {
        skinTransform[i] = InverseAbsoluteBindPose[i] * bones[SkeletonRoot + i] * world;
      }
    }
  }
}
