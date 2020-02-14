using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public abstract class BoneAnimationControlerBase : IBoneAnimationController
  {
    public abstract bool TryGetBoneTransforms(int bone, out Vector3 translation, out Quaternion rotation, out Vector3 scale, out float blendWeight);

    public bool TryGetGoneTransforms(int bone, out Matrix4 transform, out float blendWeight)
    {
      Vector3 translation;
      Quaternion rotation;
      Vector3 scale;

      bool result = TryGetBoneTransforms(bone, out translation, out rotation, out scale, out blendWeight);

      transform = Matrix4.Rotate(rotation) * Matrix4.Scale(scale) * Matrix4.Translation(translation);

      return result;
    }
  }
}
