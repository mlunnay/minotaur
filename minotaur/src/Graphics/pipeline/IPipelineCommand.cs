using System.Collections.Generic;

namespace Minotaur.Graphics
{
  public interface IPipelineCommand
  {
    void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters);
  }
}
