using System;

namespace Minotaur.Graphics
{
  [AttributeUsage(AttributeTargets.Class)]
  public class PipelineCommandAttribute : Attribute
  {
    public string Name { get; set; }

    public PipelineCommandAttribute(string name)
    {
      Name = name;
    }
  }
}
