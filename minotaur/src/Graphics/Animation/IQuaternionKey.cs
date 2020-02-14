using OpenTK;

namespace Minotaur.Graphics.Animation
{
  public interface IQuaternionKey
  {
    float Time { get; }
    Quaternion Rotation { get; }
  }
}
