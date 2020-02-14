using System;
using System.Collections.Generic;
using System.Linq;
using Minotaur.Graphics;
using Minotaur.Pipeline.Graphics;
using Minotaur.Pipeline.Utils;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Minotaur.Pipeline.Processors;
using fastJSON;
using System.IO;

namespace Minotaur.Pipeline.MaterialFactories
{
  public class MaterialDefinitionFactory : IMaterialFactory
  {
    private string _name;
    private List<PassTemplate> _passes;

    public MaterialDefinitionFactory(string name, IEnumerable<PassTemplate> passes)
    {
      _name = name;
      _passes = new List<PassTemplate>(passes);
    }

    public MaterialContent CreateMaterial(ContentProcessorContext context, Dictionary<string, object> parameters)
    {
      MaterialContent content = new MaterialContent() { Name = _name };

      int i = 0;
      foreach (PassTemplate template in _passes)
      {
        MaterialContent.Pass pass = new MaterialContent.Pass(string.IsNullOrEmpty(template.Name) ? string.Format("{0}_Pass_{1}", _name, i) : template.Name);
        pass.VertexShaderSources.AddRange(template.VertexShaders.Select(s => new ExternalReferenceContent<ShaderSourceContent>(s)));
        pass.FragmentShaderSources.AddRange(template.FragmentShaders.Select(s => new ExternalReferenceContent<ShaderSourceContent>(s)));

        foreach (UniformDefinition def in template.Variables)
        {
          UniformValueContent value = null;
          object o;
          if (!parameters.TryGetValue(def.Name, out o))
          {
            foreach (string name in def.Aliases)
            {
              if (parameters.TryGetValue(name, out o))
                break;
            }
          }
          if (o != null)
            value = GetAsUniformValueContent(def.Type, def.Name, o, context);

          if (value == null) // no match, or not able to convert to uniform type
            value = def.Value;

          pass.Parameters[def.Name] = value;
        }

        foreach (UniformDefinition def in template.Constants)
        {
          pass.Parameters[def.Name] = def.Value;
        }

        pass.ComparisonParameters = template.ComparisonParameters;

        i++;
        content.Passes.Add(pass);
      }

      return content;
    }

    private UniformValueContent GetAsUniformValueContent(ActiveUniformType type, string name, object value, ContentProcessorContext context)
    {
      if (Enum.GetName(typeof(ActiveUniformType), type).StartsWith("Sampler"))
      {
        if (!(value is UniformValueContent))
        {
          context.ContentManager.Log.Warn(string.Format("Uniform {0} passed a non UniformValueContent.", name));
          return null;
        }
        return (UniformValueContent)value;
      }
      else if (value is Color4)
      {
        if (type != ActiveUniformType.FloatVec4)
        {
          context.ContentManager.Log.Warn(string.Format("Uniform {0} passed a Color4 object but is not ActiveUniformType.FloatVec4.", name));
          return null;
        }
        Color4 c = (Color4)value;
        return new UniformValueContent(UniformValueType.Float, new object[] { c.R, c.G, c.B, c.A });
      }
      else if (value is float || value is double)
      {
        if (type != ActiveUniformType.Float)
        {
          context.ContentManager.Log.Warn(string.Format("Uniform {0} passed a float but is not ActiveUniformType.Float.", name));
          return null;
        }

        float v = (float)value;
        return new UniformValueContent(UniformValueType.Float, new object[] { v });
      }
      else if (value is bool)
      {
        if (type != ActiveUniformType.Int)
        {
          context.ContentManager.Log.Warn(string.Format("Uniform {0} passed a boolean but is not ActiveUniformType.Int.", name));
          return null;
        }
        return new UniformValueContent(UniformValueType.Int, new object[] { (bool)value ? 1 : 0 });
      }

      context.ContentManager.Log.Warn(string.Format("Uniform {0} passed an unknown type {1}.", name, value.GetType()));
      return null;
    }
  }
}
