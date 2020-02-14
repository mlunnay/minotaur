using System.Collections.Generic;
using Minotaur.Graphics.Animation;

namespace Minotaur.Pipeline.Graphics
{
  public class BoneAnimationsContent : ContentItem
  {
      public ushort TotalFrames;
      public float FramesPerSecond;
      public KeyframeEnding preferedEnding = KeyframeEnding.Clamp;
      public List<List<VectorKey>> TranslationKeys = new List<List<VectorKey>>();
      public List<List<QuaternionKey>> QuaternionKeys = new List<List<QuaternionKey>>();
      public List<List<VectorKey>> ScaleKeys = new List<List<VectorKey>>();
  }
}
