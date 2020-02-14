using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Minotaur.Pipeline.Graphics;
using OpenTK.Graphics;

namespace Minotaur.Pipeline.MaterialFactories
{
  public class MaterialFactoryManager
  {
    private Dictionary<string, IMaterialFactory> _materialFactories;
    private List<string> _assemblies = new List<string>() { null };

    public MaterialFactoryManager()
    {
    }
    
    public void AddMaterialDefinitionFactory(string name, IEnumerable<PassTemplate> passTemplates)
    {
      _materialFactories[name] = new MaterialDefinitionFactory(name, passTemplates);
    }

    public void AddAssembly(string assemblyPath)
    {
      if (_materialFactories == null)
        _assemblies.Add(assemblyPath);
      else
        ProcessAssembly(assemblyPath);
    }

    public void AddMaterialFactory(IMaterialFactory factory)
    {
      if (_materialFactories == null)
        ProcessAssemblies();
      string name = factory.GetType().Name;
      _materialFactories[name] = factory;
    }

    public Func<ContentProcessorContext, Dictionary<string, object>, MaterialContent> GetMaterialFactory(string typeName)
    {
      if (_materialFactories == null)
        ProcessAssemblies();
      IMaterialFactory factory;
      if (!_materialFactories.TryGetValue(typeName, out factory))
        throw new KeyNotFoundException(string.Format("Material factory {0} is not registered.", typeName));
      return factory.CreateMaterial;
    }

    private void ProcessAssemblies()
    {
      _materialFactories = new Dictionary<string, IMaterialFactory>();

      foreach (string assemblyPath in _assemblies)
      {
        ProcessAssembly(assemblyPath);
      }

      AddBuiltinMaterialDefinitionFactories();
    }

    private void AddBuiltinMaterialDefinitionFactories()
    {
      AddMaterialDefinitionFactory("BasicMaterial", new PassTemplate[] {
        new PassTemplate(new[] { "builtin://basic_vert.meb" }, new[] { "builtin://basic_frag.meb" })
        .AddVariable("Texture", "builtin://white.meb", new List<string>(){ "DiffuseMap" })
        .AddVariable("Diffuse", Color4.White, new List<string>(){ "DiffuseColor" })
        .AddComparisonParameter("Texture")
      });
    }

    private void ProcessAssembly(string assemblyPath)
    {
        Type[] exportedTypes;
        try
        {
          if (assemblyPath == null)
          {
            Assembly a = Assembly.GetExecutingAssembly();
            // We only look at public types for external importers, processors
            // and type writers.
            exportedTypes = a.GetExportedTypes();
          }
          else
          {
            Assembly a = Assembly.LoadFrom(assemblyPath);
            // We only look at public types for external importers, processors
            // and type writers.
            exportedTypes = a.GetExportedTypes();
          }
        }
        catch (Exception)
        {
          throw;
        }

        foreach (Type type in exportedTypes)
        {
          // only instanciate types with parameterless constructors.
          if (type.GetConstructor(Type.EmptyTypes) != null && type.GetInterface("IMaterialFactory") != null)
          {
            IMaterialFactory factory = (IMaterialFactory)Activator.CreateInstance(type);
            if (factory == null)
              continue;
            _materialFactories[type.Name] = factory;
          }
      }
    }
  }
}
