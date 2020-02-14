
namespace Minotaur.Graphics
{
  public class GraphicsStatistics
  {
    public int VisibleLightCount { get; internal set; }
    public int VisibleObjectCount { get; internal set; }
    public int VisibleDrawableCount { get; internal set; }

    public int VertexCount { get; internal set; }
    public int PrimitiveCount { get; internal set; }
    public int DrawCount { get; internal set; }

    public GraphicsStatistics()
    {
      Reset();
    }

    public void Reset()
    {
      VisibleLightCount = 0;
      VisibleDrawableCount = 0;
      VisibleObjectCount = 0;
      VertexCount = 0;
      PrimitiveCount = 0;
      DrawCount = 0;
    }
  }
}
