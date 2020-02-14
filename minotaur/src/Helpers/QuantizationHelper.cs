using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Helpers
{
  /// <summary>
  /// This class provides helper methods for Quantizing of floating point numbers into unsigned integrals.
  /// </summary>
  public static class QuantizationHelper
  {
    /// <summary>
    /// The Max translation allowed, this defaults to 2 meters and needs to be the same as the value used in the pipeline building process
    /// </summary>
    public static float MaxTranslation = 2.0f;

    public static UInt32 CompressUnitFloatRL(float unitFloat, int nBits)
    {
      // Determine the number of intervals based on the
      // number of output bits we’ve been asked to produce.
      int nIntervals = 1 << nBits;
      // Scale the input value from the range [0, 1] into
      // the range [0, nIntervals – 1]. We subtract one
      // interval because we want the largest output value
      // to fit into nBits bits.
      float scaled = unitFloat * (float)(nIntervals - 1);
      // Finally, round to the nearest interval center. We
      // do this by adding 0.5f, and then truncating to the
      // next-lowest interval index (by casting to U32).
      UInt32 rounded = (UInt32)(scaled + 0.5f);
      // Guard against invalid input values.
      if (rounded > nIntervals - 1)
        rounded = (UInt32)nIntervals - 1;
      return rounded;
    }

    public static float DecompressUnitFloatRL(UInt32 quantized, int nBits)
    {
      // Determine the number of intervals based on the
      // number of output bits we’ve been asked to produce.
      int intervals = 1 << nBits;

      // Decode by simply converting the uint to a float, and
      // scaling by the interval size.
      float intervalSize = 1.0f / (float)(intervals - 1);

      return (float)quantized * intervalSize;
    }

    public static UInt32 CompressFloatRL(float value, float min, float max, int nBits)
    {
      float unitFloat = (value - min) / (max - min);
      return CompressUnitFloatRL(unitFloat, nBits);
    }

    public static float DecompressFloatRL(UInt32 quantized, float min, float max, int nBits)
    {
      float unitFloat = DecompressUnitFloatRL(quantized, nBits);
      return min + (unitFloat * (max - min));
    }

    public static UInt16 CompressRotationChannel(float qx)
    {
      return (UInt16)CompressFloatRL(qx, -1.0f, 1.0f, 16);
    }

    public static float DecompressRotationChannel(UInt16 qx)
    {
      return DecompressFloatRL((UInt32)qx, -1.0f, 1.0f, 16);
    }

    public static UInt16 CompressTranslationChannel(float vx)
    {
      // clamp to valid range
      if (vx < -MaxTranslation)
        vx = -MaxTranslation;
      else if (vx > MaxTranslation)
        vx = MaxTranslation;

      return (UInt16)CompressFloatRL(vx, -MaxTranslation, MaxTranslation, 16);
    }

    public static float DecompressTranslationChannel(UInt16 vx)
    {
      return DecompressFloatRL((UInt32)vx, -MaxTranslation, MaxTranslation, 16);
    }
  }
}
