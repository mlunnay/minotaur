using Minotaur.Helpers;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  /// <summary>
  /// Compresses the quarternion value by converting them to UInt16. this makes the vector take up 64 bits instead of 128 bits.
  /// </summary>
  public class CompressedQuarternionKey : IQuaternionKey
  {
    private float _time;
    private ushort _x;
    private ushort _y;
    private ushort _z;
    private ushort _w;

    public float Time
    {
      get { return _time; }
    }

    public Quaternion Rotation
    {
      get
      {
        return new Quaternion(QuantizationHelper.DecompressRotationChannel(_x),
          QuantizationHelper.DecompressRotationChannel(_y),
          QuantizationHelper.DecompressRotationChannel(_z),
          QuantizationHelper.DecompressRotationChannel(_w));
      }
    }

    public CompressedQuarternionKey(ushort frame, ushort x, ushort y, ushort z, ushort w)
    {
      _time = frame;
      _x = x;
      _y = y;
      _z = z;
      _w = w;
    }
  }
}
