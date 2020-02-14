using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public interface IBoneAnimationController
  {
    bool TryGetBoneTransforms(int bone, out Vector3 translation, out Quaternion rotation, out Vector3 scale, out float blendWeight);
    bool TryGetGoneTransforms(int bone, out Matrix4 transform, out float blendWeight);
  }
}
