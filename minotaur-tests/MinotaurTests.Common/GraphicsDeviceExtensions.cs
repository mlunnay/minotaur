using Minotaur.Graphics;
using OpenTK;

namespace MinotaurTests.Common
{
  public static class GraphicsDeviceExtensions
  {
    public static void DrawFPS(this GraphicsDevice graphicsDevice, int x = 0, int y = 0)
    {
      graphicsDevice.DebugBatch.DrawString(
        string.Format("Current FPS: {0:0.00}\nOverall FPS: {1:0.00}", graphicsDevice.FPS.FramesPerSecond, graphicsDevice.FPS.OverallFramesPerSecond),
        world: Matrix4.CreateTranslation(x, y, 0));
    }

    public static void DrawStatitics(this GraphicsDevice graphicsDevice, int x = 0, int y = 0)
    {
      graphicsDevice.DebugBatch.DrawString(
        string.Format("Draw Calls: {0}\nPrimitive Count: {1}\nVertices Count: {2}", graphicsDevice.Statistics.DrawCount, graphicsDevice.Statistics.PrimitiveCount, graphicsDevice.Statistics.VertexCount),
        world: Matrix4.CreateTranslation(x, y, 0));
    }
  }
}
