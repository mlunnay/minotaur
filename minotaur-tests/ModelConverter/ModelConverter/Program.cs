using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using Mono.Options;
using Minotaur.Pipeline.Graphics;
using Minotaur.Pipeline.Importers;
using Minotaur.Pipeline;
using log4net;
using System.ComponentModel;
using System.Globalization;
using Minotaur.Pipeline.Writers;

namespace ModelConverter
{
  class Program
  {
    public static ILog _log = LogManager.GetLogger(typeof(Program));

    static string _outputName;
    static string _inputName;
    static Dictionary<string, object> _opaqueData = new Dictionary<string, object>();

    static Dictionary<Type, Type> typeWriterMap = new Dictionary<Type, Type>();

    private struct ImporterInfo
    {
      public ContentImporterAttribute attribue;
      public Type type;
    };

    private static List<ImporterInfo> _importers;

    private struct ProcessorInfo
    {
      public ContentProcessorAttribute attribue;
      public Type type;
    };

    private static List<ProcessorInfo> _processors;

    private static List<Type> _writers;

    private static List<string> _assemblies = new List<string>() { "Minotaur.Pipeline.dll" };

    static void Main(string[] args)
    {
      log4net.Config.BasicConfigurator.Configure(new log4net.Appender.DebugAppender());

      if (!ParseCommandLine(args))
        return;

      if (string.IsNullOrEmpty(_outputName))
      {
        _outputName = string.Format("{0}.meb", Path.GetFileNameWithoutExtension(_inputName));
      }



      //GetTypeWriters();
      ProcessAssemblies();

      string ext = Path.GetExtension(_inputName).ToLower();
      var imp = _importers.FirstOrDefault(i => i.attribue.FileExtensions.Contains(ext, StringComparer.InvariantCultureIgnoreCase));
      if (imp.type == null)
      {
        Console.WriteLine("file format is not handled by ModelImporter.");
        return;
      }

      ContentManager manager = new ContentManager(_log);
      ContentTypeWriterManager contentTypeWriterManager = new ContentTypeWriterManager();
      contentTypeWriterManager.RegisterTypeWriter<ModelContent>(new ModelWriter());

      IContentImporter importer = CreateImporter(imp.type, _opaqueData);

      var content = importer.Import(_inputName, manager);

      IContentProcessor processor = CreateProcessor(FindDefaultProcessor(importer.GetType()), _opaqueData);

      object processedContent = processor.Process(content, new ContentProcessorContext());

      ContentTypeWriter typeWriter = GetTypeWriter(processedContent.GetType());

      using (FileStream stream = new FileStream(_outputName, FileMode.Create))
      {
        ContentWriter writer = manager.CreateWriter(contentTypeWriterManager, stream);
        writer.WriteObject(processedContent, typeWriter);
        writer.Flush();
      }
    }

    private static void ProcessAssemblies()
    {
      _importers = new List<ImporterInfo>();
      _processors = new List<ProcessorInfo>();
      _writers = new List<Type>();

      foreach (string assemblyPath in _assemblies)
      {
        Type[] exportedTypes;
        try
        {
          Assembly a = Assembly.LoadFrom(assemblyPath);
          // We only look at public types for external importers, processors
          // and type writers.
          exportedTypes = a.GetExportedTypes();
        }
        catch (Exception)
        {
          throw;
        }

        Type contentTypeWriterType = typeof(ContentTypeWriter<>);
        foreach (Type t in exportedTypes)
        {
          if (!t.IsPublic || t.IsAbstract)
            continue;

          if (t.GetInterface("IContentImporter") != null)
          {
            var attributes = t.GetCustomAttributes(typeof(ContentImporterAttribute), false);
            if (attributes.Length != 0)
            {
              var importerAttribute = attributes[0] as ContentImporterAttribute;
              _importers.Add(new ImporterInfo { attribue = importerAttribute, type = t });
            }
            else
            {
              // If no attribute specify default one
              var importerAttribute = new ContentImporterAttribute(".*");
              importerAttribute.DefaultProcessor = "";
              importerAttribute.DisplayName = t.Name;
              _importers.Add(new ImporterInfo { attribue = importerAttribute, type = t });
            }
          }
          else if (t.GetInterface("IContentProcessor") != null)
          {
            var attributes = t.GetCustomAttributes(typeof(ContentProcessorAttribute), false);
            if (attributes.Length != 0)
            {
              var processorAttribute = attributes[0] as ContentProcessorAttribute;
              _processors.Add(new ProcessorInfo { attribue = processorAttribute, type = t });
            }
          }
          else if (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == contentTypeWriterType)
          {
            Type baseType = t.BaseType;
            while ((baseType != null) && baseType.IsGenericType && (baseType.GetGenericTypeDefinition() != contentTypeWriterType))
              baseType = baseType.BaseType;
            if (baseType != null)
              typeWriterMap.Add(baseType, t);
          }
        }
      }
    }

    private static string FindDefaultProcessor(Type type)
    {
      foreach (var info in _importers)
      {
        if (info.type == type)
          return info.attribue.DefaultProcessor;
      }
      return null;
    }

    private static IContentImporter CreateImporter(string name, Dictionary<string, object> importerData)
    {
      var i = _processors.FirstOrDefault(a => a.type.Name == name);
      if (i.type == null)
        return null;
      return CreateImporter(i.type, importerData);
    }

    private static IContentImporter CreateImporter(Type type, Dictionary<string, object> importerData)
    {
      if (type == null)
        return null;

      var importer = (IContentImporter)Activator.CreateInstance(type);

      foreach (var param in importerData)
      {
        var propInfo = type.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
        if (propInfo == null || propInfo.GetSetMethod(false) == null)
          continue;

        if (propInfo.PropertyType.IsInstanceOfType(param.Value))
          propInfo.SetValue(importer, param.Value, null);
        else
        {
          // find a type converter for this property
          var typeConverter = TypeDescriptor.GetConverter(propInfo.PropertyType);
          if (typeConverter.CanConvertFrom(param.Value.GetType()))
          {
            var propValue = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
            propInfo.SetValue(importer, propValue, null);
          }
        }
      }

      return importer;
    }

    private static IContentProcessor CreateProcessor(string name, Dictionary<string, object> processorData)
    {
      var p = _processors.FirstOrDefault(a => a.type.Name == name);
      if (p.type == null)
        return null;
      return CreateProcessor(p.type, processorData);
    }

    private static IContentProcessor CreateProcessor(Type type, Dictionary<string, object> processorData)
    {
      if (type == null)
        return null;

      var processor = (IContentProcessor)Activator.CreateInstance(type);

      foreach (var param in processorData)
      {
        var propInfo = type.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
        if (propInfo == null || propInfo.GetSetMethod(false) == null)
          continue;

        if (propInfo.PropertyType.IsInstanceOfType(param.Value))
          propInfo.SetValue(processor, param.Value, null);
        else
        {
          // find a type converter for this property
          var typeConverter = TypeDescriptor.GetConverter(propInfo.PropertyType);
          if (typeConverter.CanConvertFrom(param.Value.GetType()))
          {
            var propValue = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
            propInfo.SetValue(processor, propValue, null);
          }
        }
      }

      return processor;
    }

    private static void GetTypeWriters()
    {
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type[] exportedTypes;
        try
        {
          exportedTypes = assembly.GetTypes();
        }
        catch (Exception)
        {
          continue;
        }

        var contentTypeWriterType = typeof(ContentTypeWriter<>);
        foreach (var type in exportedTypes)
        {
          if (type.IsAbstract)
            continue;


          // Find the content type this writer implements
          Type baseType = type.BaseType;
          if (baseType != null && baseType.IsGenericType && baseType.GetGenericTypeDefinition() == contentTypeWriterType)
          {
            while ((baseType != null) && baseType.IsGenericType && (baseType.GetGenericTypeDefinition() != contentTypeWriterType))
              baseType = baseType.BaseType;
            if (baseType != null)
              typeWriterMap.Add(baseType, type);
          }
        }
      }
    }

    /// <summary>
    /// Retrieves the worker writer for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The worker writer.</returns>
    /// <remarks>This should be called from the ContentTypeWriter.Initialize method.</remarks>
    public static ContentTypeWriter GetTypeWriter(Type type)
    {
      ContentTypeWriter result = null;
      var contentTypeWriterType = typeof(ContentTypeWriter<>).MakeGenericType(type);
      Type typeWriterType;
      if (!typeWriterMap.TryGetValue(contentTypeWriterType, out typeWriterType))
      {
        var inputTypeDef = type.GetGenericTypeDefinition();

        Type chosen = null;
        foreach (var kvp in typeWriterMap)
        {
          var args = kvp.Key.GetGenericArguments();

          if (args.Length == 0)
            continue;

          if (!args[0].IsGenericType)
            continue;

          // Compare generic type definition
          var keyTypeDef = args[0].GetGenericTypeDefinition();
          if (inputTypeDef.Equals(keyTypeDef))
          {
            chosen = kvp.Value;
            break;
          }
        }

        try
        {
          var concreteType = type.GetGenericArguments();
          result = (ContentTypeWriter)Activator.CreateInstance(chosen.MakeGenericType(concreteType));

          // save it for next time.
          typeWriterMap.Add(contentTypeWriterType, result.GetType());
        }
        catch (Exception)
        {
          throw new ArgumentException(String.Format("Could not find ContentTypeWriter for type '{0}'", type.Name));
        }
      }
      else
      {
        result = (ContentTypeWriter)Activator.CreateInstance(typeWriterType);
      }

      if (result != null)
      {
        MethodInfo dynMethod = result.GetType().GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
        dynMethod.Invoke(result, new object[] { new ContentTypeWriterManager() });
      }
      return result;
    }

    private static bool ParseCommandLine(string[] args)
    {
      bool showHelp = false;
      OptionSet p = new OptionSet()
      {
        {"o=|output=", "The output file to write the model to.", a => _outputName = a},
        {"s|skin", "Include bone weight data for skinning of the model", a => _opaqueData["Skin"] = true},
        {"h|help|?", "Show this help mesage.", a => showHelp = true},
      };


      List<string> extra = p.Parse(args);

      if (showHelp)
      {
        ShowHelp(p);
        return false;
      }

      if (extra.Count != 1)
      {
        Console.WriteLine("Error: Model file was not given.");
        ShowHelp(p);
        return false;
      }

      _inputName = extra[0];

      return true;
    }

    private static void ShowHelp(OptionSet options)
    {
      Console.WriteLine("Usage: ModelConverter [OPTIONS] MODEL");
      Console.WriteLine("Converts an model to a Minotaur Model file.");
      Console.WriteLine();
      Console.WriteLine("Options:");
      options.WriteOptionDescriptions(Console.Out);
    }
  }
}
