using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class VertexAttributeMappings
  {
    #region Declarations

    private List<AttributeMapping> _mappings = new List<AttributeMapping>();
    private Dictionary<Program, List<VertexArray>> _programBindings = new Dictionary<Program,List<VertexArray>>();

    public class AttributeMapping
    {
      public string Name { get; private set; }
      public VertexUsage Usage { get; private set; }
      public int Index { get; private set; }

      public AttributeMapping(string name, VertexUsage usage, int index)
      {
        Name = name;
        Usage = usage;
        Index = index;
      }
    }

    #endregion

    #region Properties

    public List<AttributeMapping> Mappings
    {
      get { return new List<AttributeMapping>(_mappings); }
    }

    #endregion

    #region Constructor



    #endregion

    #region Public Methods

    public void Add(string name, VertexUsage usage, int index = 0)
    {
      _mappings.Add(new AttributeMapping(name, usage, index));
    }

    public void BindAttributes(
      VertexArray vao,
      Program program)
    {
      List<VertexArray> vaList;
      if (!_programBindings.TryGetValue(program, out vaList) || !vaList.Contains(vao))
      {
        if (vaList == null)
          _programBindings[program] = new List<VertexArray>();
        _programBindings[program].Add(vao);

        foreach (AttributeMapping mapping in _mappings)
        {
          if (program.HasAttribute(mapping.Name) &&
            vao.VertexBuffer.VertexFormat.HasAttribute(mapping.Usage, mapping.Index))
          {
            vao.AddBinding(program.VertexAttribute(mapping.Name).Slot, mapping.Usage, mapping.Index);
          }
        }
      }
    }

    #endregion
  }
}
