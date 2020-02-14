using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
  public class SetUniformCommand : IPipelineCommand
  {
    #region IPipelineCommand Members

    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      float[] values = parameters["value"].ToString().Split(new char[] { ',' }).Select(f => float.Parse(f)).ToArray();
      IUniformValue value;
      if(values.Length == 1)
        value = UniformValue.Create(values[0]);
      else if(values.Length == 2)
        value = UniformValue.Create(values[0], values[1]);
      else if(values.Length == 3)
        value = UniformValue.Create(values[0], values[1], values[2]);
      else if(values.Length == 4)
        value = UniformValue.Create(values[0], values[1], values[2], values[3]);
      else
        throw new ArgumentException("value parameter must contain 1 - 4 float values.");
      stage.AddShaderUniform(parameters["material"].ToString(), parameters["uniform"].ToString(), value);
    }

    #endregion
  }
}
