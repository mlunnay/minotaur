using System.Collections.Generic;

namespace Minotaur.Graphics
{
  public class UnbindBufferCommand : IPipelineCommand
  {
    #region IPipelineCommand Members

    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      stage.Pipeline.ClearSamplerBindings();
    }

    #endregion
  }
}
