using Minotaur.Helpers;
using OpenTK;

namespace Minotaur.Graphics.Animation
{
  /// <summary>
  /// Compresses the vector values by converting them to UInt16. this makes the vector take up 48 bits instead of 96 bits.
  /// </summary>
  public class CompressedVectorKey : IVectorKey
  {
    private float _time;
    private ushort _x;
    private ushort _y;
    private ushort _z;

    public float Time
    {
      get { return _time; }
    }

    public Vector3 Vector
    {
      get
      {
        return new Vector3(QuantizationHelper.DecompressTranslationChannel(_x),
          QuantizationHelper.DecompressTranslationChannel(_y),
          QuantizationHelper.DecompressTranslationChannel(_z));
      }
    }

    public CompressedVectorKey(ushort frame, ushort x, ushort y, ushort z)
    {
      _time = frame;
      _x = x;
      _y = y;
      _z = z;
    }
  }
}
