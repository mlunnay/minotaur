using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public interface IVectorKey
  {
    float Time { get; }
    Vector3 Vector { get; }
  }
}
