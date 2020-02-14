using System.Collections.Generic;

namespace Minotaur.Graphics
{
  public class RenderPipelineStage
  {
    public class PipelineCommand
    {
      public IPipelineCommand Command;
      public Dictionary<string, object> Parameters;
    }

    public string Name { get; set; }
    public Dictionary<string, object> Parameters { get; private set; }
    public List<PipelineCommand> Commands { get; private set; }
    public bool Enabled { get; set; }
    public RenderPipeline Pipeline { get; private set; }
    public Dictionary<string, Dictionary<string, IUniformValue>> ShaderUniforms;

    public RenderPipelineStage(RenderPipeline pipeline,
      string name)
    {
      Pipeline = pipeline;
      Name = name;
      Parameters = new Dictionary<string, object>();
      Commands = new List<PipelineCommand>();
      Enabled = true;
      ShaderUniforms = new Dictionary<string, Dictionary<string, IUniformValue>>();
    }

    public void AddShaderUniform(string shader, string uniform, IUniformValue value)
    {
      Dictionary<string, IUniformValue> shaderDict;
      if (!ShaderUniforms.TryGetValue(shader, out shaderDict))
      {
        shaderDict = new Dictionary<string, IUniformValue>();
        ShaderUniforms[shader] = shaderDict;
      }
      shaderDict[uniform] = value;
    }

    public void ResetShaderUniforms()
    {
      ShaderUniforms.Clear();
    }

    public Dictionary<string, IUniformValue> GetShaderUniforms(string shader)
    {
      Dictionary<string, IUniformValue> shaderDict;
      if (!ShaderUniforms.TryGetValue(shader, out shaderDict))
        return new Dictionary<string, IUniformValue>();
      return new Dictionary<string,IUniformValue>(shaderDict);
    }
  }
}
