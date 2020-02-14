using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using Minotaur.Pipeline.Graphics;
using Minotaur.Graphics;

namespace Minotaur.Pipeline.MaterialFactories
{
  public class PassTemplate
  {
    public string Name;
    public List<string> VertexShaders = new List<string>();
    public List<string> FragmentShaders = new List<string>();
    public RenderState State;
    public List<UniformDefinition> Variables = new List<UniformDefinition>();
    public List<UniformDefinition> Constants = new List<UniformDefinition>();
    public List<string> ComparisonParameters = new List<string>();

    public PassTemplate(string name, IEnumerable<string> vertexShaders, IEnumerable<string> fragmentShaders)
    {
      Name = name;
      State = new RenderState();
      VertexShaders.AddRange(vertexShaders);
      FragmentShaders.AddRange(fragmentShaders);
    }

    public PassTemplate(IEnumerable<string> vertexShaders, IEnumerable<string> fragmentShaders)
      : this(null, vertexShaders, fragmentShaders) { }

    public PassTemplate AddVariable(string name, Color4 c, List<string> aliases = null)
    {
      Variables.Add(UniformDefinition.Create(name, c, aliases));
      return this;
    }

    public PassTemplate AddVariable(string name, string path, List<string> aliases = null)
    {
      Variables.Add(UniformDefinition.Create(name, path, aliases));
      return this;
    }

    public PassTemplate AddVariable(string name, int[] values, List<string> aliases = null)
    {
      Variables.Add(UniformDefinition.Create(name, values, aliases));
      return this;
    }

    public PassTemplate AddVariable(string name, float[] values, List<string> aliases = null)
    {
      Variables.Add(UniformDefinition.Create(name, values, aliases));
      return this;
    }

    public PassTemplate AddVariable(string name, uint[] values, List<string> aliases = null)
    {
      Variables.Add(UniformDefinition.Create(name, values, aliases));
      return this;
    }

    public PassTemplate AddVariable(string name, SamplerContent sampler, List<string> aliases = null)
    {
      Variables.Add(UniformDefinition.Create(name, sampler, aliases));
      return this;
    }

    public PassTemplate AddConstant(string name, Color4 color)
    {
      Constants.Add(UniformDefinition.Create(name, color));
      return this;
    }

    public PassTemplate AddConstant(string name, string path)
    {
      Constants.Add(UniformDefinition.Create(name, path));
      return this;
    }

    public PassTemplate AddConstant(string name, int[] values)
    {
      Constants.Add(UniformDefinition.Create(name, values));
      return this;
    }

    public PassTemplate AddConstant(string name, float[] values)
    {
      Constants.Add(UniformDefinition.Create(name, values));
      return this;
    }

    public PassTemplate AddConstant(string name, uint[] values)
    {
      Constants.Add(UniformDefinition.Create(name, values));
      return this;
    }

    public PassTemplate AddConstant(string name, SamplerContent sampler)
    {
      Constants.Add(UniformDefinition.Create(name, sampler));
      return this;
    }

    public PassTemplate SetState(RenderState state)
    {
      State = state;
      return this;
    }

    public PassTemplate AddComparisonParameter(string parameterName)
    {
      ComparisonParameters.Add(parameterName);
      return this;
    }
  }
}
