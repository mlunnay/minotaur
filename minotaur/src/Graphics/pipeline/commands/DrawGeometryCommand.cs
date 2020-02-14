using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
  public class DrawGeometryCommand : IPipelineCommand
  {
    #region IPipelineCommand Members

    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      // TODO: implement
      throw new NotImplementedException();
    }

    #endregion
  }
}
