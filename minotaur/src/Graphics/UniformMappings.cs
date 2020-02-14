using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
  public class UniformMappings
  {
    private Dictionary<string, UniformUsage> _uniformToUsage = new Dictionary<string, UniformUsage>();
    private Dictionary<UniformUsage, string> _usageToUniform = new Dictionary<UniformUsage, string>();

    public void Add(string uniformName, UniformUsage usage)
    {
      _uniformToUsage.Add(uniformName, usage);
      _usageToUniform.Add(usage, uniformName);
    }

    public UniformUsage this[string uniformName]
    {
      get
      {
        UniformUsage usage;
        if (!_uniformToUsage.TryGetValue(uniformName, out usage))
          usage = UniformUsage.GenericUniform;
        return usage;
      }
    }

    public UniformUsage GetUsage(string uniformName)
    {
      UniformUsage usage;
      if (!_uniformToUsage.TryGetValue(uniformName, out usage))
        usage = UniformUsage.GenericUniform;
      return usage;
    }

    public string GetUniformForUsage(UniformUsage usage)
    {
      string uniform;
      _usageToUniform.TryGetValue(usage, out uniform);
      return uniform;
    }
  }
}
