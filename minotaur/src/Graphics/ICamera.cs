using OpenTK;

namespace Minotaur.Graphics
{
  /// <summary>
  /// Interface for game camera
  /// </summary>
  public interface ICamera
  {
    /// <summary>
    /// Gets the camera view matrix
    /// </summary>
    Matrix4 View { get; }

    /// <summary>
    /// Gets the camera projection matrix
    /// </summary>
    Matrix4 Projection { get; }

    /// <summary>
    /// The combined View and Projection matrices
    /// </summary>
    Matrix4 Matrix { get; }

    Viewport Viewport { get; set; }

    float Near { get; set; }

    float Far { get; set; }
  }
}
