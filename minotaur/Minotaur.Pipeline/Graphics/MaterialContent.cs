using System.Collections.Generic;
using Minotaur.Graphics;

namespace Minotaur.Pipeline.Graphics
{
  public class MaterialContent : ContentItem
  {
    public class Pass
    {
      public string Name { get; set; }
      public List<ExternalReferenceContent<ShaderSourceContent>> VertexShaderSources { get; private set; }
      public List<ExternalReferenceContent<ShaderSourceContent>> FragmentShaderSources { get; private set; }
      public Dictionary<string, UniformValueContent> Parameters { get; set; }
      public RenderState State { get; set; }
      public List<string> ComparisonParameters { get; set; }

      public Pass(string name = "")
      {
        Name = name;
        VertexShaderSources = new List<ExternalReferenceContent<ShaderSourceContent>>();
        FragmentShaderSources = new List<ExternalReferenceContent<ShaderSourceContent>>();
        Parameters = new Dictionary<string, UniformValueContent>();
        State = new RenderState();
        ComparisonParameters = new List<string>();
      }
    }

    public List<Pass> Passes { get; private set; }

    public MaterialContent()
    {
      Passes = new List<Pass>();
    }
  }
}
