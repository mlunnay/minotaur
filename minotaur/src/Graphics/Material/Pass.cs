using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Graphics
{
  public class Pass : ICloneable
  {
    public string Name { get; private set; }

    public Program Program { get; set; }

    public RenderState State { get; set; }

    public Dictionary<string, IUniformValue> Parameters { get; private set; }

    public List<string> ComparisonParameters { get; internal set; }

    public int ProgramHash
    {
      get { return Program.ShaderHash; }
    }

    private Pass(Program program)
    {
      Program = program;
      Parameters = new Dictionary<string, IUniformValue>();
      State = new RenderState();
      ComparisonParameters = new List<string>();
    }

    public Pass(Program program, string name)
      :this(program)
    {
      Name = name;
    }

    public Pass(Pass pass)
    {
      Program = pass.Program;
      Name = pass.Name;
      Parameters = new Dictionary<string, IUniformValue>(pass.Parameters);
      State = (RenderState)pass.State.Clone();
    }

    public void BindParameters()
    {
      Program.BindUniforms(Parameters);
    }

    public void Apply(GraphicsDevice graphicsDevice)
    {
      Program.Bind();
      BindParameters();
      graphicsDevice.RenderState = State;
    }

    public int Compare(Pass other)
    {
      int result = ProgramHash.CompareTo(other.ProgramHash);
      if (result == 0)
      {
        foreach (string p in ComparisonParameters)
        {
          IComparable<IUniformValue> comparer = Parameters[p] as IComparable<IUniformValue>;
          if (comparer != null)
          {
            result = comparer.CompareTo(other.Parameters[p]);
            if (result != 0)
              return result;
          }
          else
            return -1;
        }
      }

      return result;
    }

    #region ICloneable Members

    public object Clone()
    {
      Pass pass = new Pass(Program);
      pass.Name = Name;
      pass.Parameters = Parameters.ToDictionary(k => k.Key, e => (IUniformValue)e.Value.Clone());
      pass.State = (RenderState)State.Clone();
      return pass;
    }

    #endregion
  }
}
