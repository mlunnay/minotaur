using System;
using System.Linq;
using log4net;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;
using Minotaur.Content;
using System.Text.RegularExpressions;
using Minotaur.Pipeline.MaterialFactories;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline
{
  public class ContentManager
  {
    private struct ImporterInfo
    {
      public ContentImporterAttribute attribue;
      public Type type;
    };

    private struct ProcessorInfo
    {
      public ContentProcessorAttribute attribue;
      public Type type;
    };

    private List<ImporterInfo> _importers;
    private List<ProcessorInfo> _processors;
    private Dictionary<Type, Type> typeWriterMap = new Dictionary<Type, Type>();
    private List<string> _assemblies = new List<string>() { null };
    private ContentProcessorContext _processorContext;
    private ContentTypeWriterManager _typeWriterManager;
    private MaterialFactoryManager _materialFactoryManager;

    private IContentSaver _contentSaver;
    private IBuildOptions _buildOptions;
    private IBuildSource _buildSource;

    private Regex _camelCaseRegex = new Regex("([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))");

    public ILog Log { get; private set; }

    public ContentProcessorContext ProcessorContext { get { return _processorContext; } }

    public MaterialFactoryManager MaterialFactoryManager { get { return _materialFactoryManager; } }

    public ContentManager(ILog log, IBuildOptions buildOptions, IContentSaver contentSaver, IBuildSource buildSource)
    {
      Log = log;
      _buildOptions = buildOptions;
      _processorContext = new ContentProcessorContext();
      _processorContext.ContentManager = this;
      _contentSaver = contentSaver;
      _materialFactoryManager = new MaterialFactoryManager();
      _buildSource = buildSource;
    }

    public void AddAssembly(string assemblyPath)
    {
      if (assemblyPath == null)
        throw new ArgumentNullException("assemblyPath");
      _assemblies.Add(assemblyPath);
      _materialFactoryManager.AddAssembly(assemblyPath);
    }

    public ContentWriter CreateWriter(ContentTypeWriterManager typeWriterManager, Stream stream, bool compressOutput = false, string identifierString = "MEB")
    {
      return new ContentWriter(typeWriterManager, this, stream, compressOutput, identifierString);
    }

    public ExternalReferenceContent<T> BuildContent<T>(string filename,
      string importerName = null,
      Dictionary<string, object> importerData = null,
      string processorName = null,
      Dictionary<string, object> processorData = null,
      string writerName = null,
      Dictionary<string, object> writerData = null,
      bool ignoreBuildItem = false)
    {
      object content;
      return BuildContent<T>(filename, out content,
        importerName, importerData, processorName, processorData,
        writerName, writerData, ignoreBuildItem);
    }

    public ExternalReferenceContent<T> BuildContent<T>(string filename,
      out object content,
      string importerName = null,
      Dictionary<string, object> importerData = null,
      string processorName = null,
      Dictionary<string, object> processorData = null,
      string writerName = null,
      Dictionary<string, object> writerData = null,
      bool ignoreBuildItem = false)
    {
      if(IsBuiltin(filename))
      {
        content = null;
        string outputname = _buildOptions.GetOutputFilename(typeof(T), filename);
        return new ExternalReferenceContent<T>(filename);
      }

      if (!Path.IsPathRooted(filename))
        filename = _processorContext.GetFilenamePath(filename);

      if (!ignoreBuildItem && _buildSource.HasBuildItem(filename))
      {
        // if there is a build item for this item leave it up to the build source to generate the actual build, and just return a ExternalReferenceContent item.
        content = null;
        string outputname = _buildOptions.GetOutputFilename(typeof(T), filename);
        outputname = _contentSaver.GetPath(outputname, typeof(T));
        return new ExternalReferenceContent<T>(outputname);
      }

      // first find out the output type of the processor
      IContentImporter importer;
      if ((importer = CreateImporter(importerName ?? FindImporterByExtension(Path.GetExtension(filename)), importerData ?? new Dictionary<string, object>())) == null)
        throw new ContentLoadException(importerName != null ?
          string.Format("Importer {0} not found.", importerName) :
          string.Format("Importer not found that handles extension {0}", Path.GetExtension(filename)));
      IContentProcessor processor;
      if ((processor = CreateProcessor(processorName ?? FindDefaultProcessor(importer.GetType()), processorData ?? new Dictionary<string, object>())) == null)
        throw new ContentLoadException(processorName != null ?
          string.Format("Processor {0} not found.", processorName) :
          string.Format("Processor not found that handles type {0}", importer.GetType().Name));

      _processorContext.PushDirectory(Path.GetDirectoryName(filename));

      using (FileStream stream = File.OpenRead(filename))
      {
        content = importer.Import(stream, this); 
      }

      content = processor.Process(content, _processorContext);

      _processorContext.PopDirectory();

      object outName;
      if (!(content is ContentItem) || !((ContentItem)content).OpaqueData.TryGetValue("OutputFileName", out outName)) outName = filename;
      string outputFilename = _buildOptions.GetOutputFilename(content.GetType(), (string)outName);
      if (!_buildOptions.ForceRebuild && _contentSaver.GetLastModified(outputFilename, content.GetType()) > File.GetLastWriteTime(filename))
        return new ExternalReferenceContent<T>(_contentSaver.GetPath(outputFilename, processor.OutputType));

      ContentTypeWriter typeWriter = GetTypeWriter(writerName, content.GetType(), writerData);
      if (typeWriter == null && (typeWriter = GetTypeWriter(content.GetType(), writerData ?? new Dictionary<string, object>())) == null)
        throw new ContentLoadException(string.Format("ContentTypeWriter not found for content type {0}", content.GetType()));

      if(_typeWriterManager == null)
        _typeWriterManager = new ContentTypeWriterManager();
      using (MemoryStream stream = new MemoryStream())
      {
        ContentWriter writer = CreateWriter(_typeWriterManager, stream, _buildOptions.CompressOutput, _buildOptions.GetIdentifierString(content.GetType()));
        writer.WriteObject(content, typeWriter);
        writer.Flush();

        string path = _contentSaver.Save(stream, outputFilename, content.GetType());
        return new ExternalReferenceContent<T>(path);
      }
    }

    public void LoadContentStream(string filename,
      Stream outStream,
      string importerName = null,
      Dictionary<string, object> importerData = null,
      string processorName = null,
      Dictionary<string, object> processorData = null,
      string writerName = null,
      Dictionary<string, object> writerData = null)
    {
      _typeWriterManager = new ContentTypeWriterManager();

      IContentImporter importer;
      if((importer = CreateImporter(importerName ?? FindImporterByExtension(Path.GetExtension(filename)), importerData ?? new Dictionary<string, object>())) == null)
        throw new ContentLoadException(importerName != null ? 
          string.Format("Importer {0} not found.", importerName) : 
          string.Format("Importer not found that handles extension {0}", Path.GetExtension(filename)));

      object content;
      using (FileStream stream = File.OpenRead(filename))
      {
        content = importer.Import(stream, this);
      }

      IContentProcessor processor;
      if((processor = CreateProcessor(processorName ?? FindDefaultProcessor(importer.GetType()), processorData ?? new Dictionary<string,object>())) == null)
        throw new ContentLoadException(processorName != null ?
          string.Format("Processor {0} not found.", processorName) :
          string.Format("Processor not found that handles type {0}", importer.GetType().Name));

      _processorContext.PushDirectory(Path.GetDirectoryName(filename));
      content = processor.Process(content, _processorContext);
      _processorContext.PopDirectory();

      ContentTypeWriter typeWriter = GetTypeWriter(writerName, content.GetType(), writerData);
      if(typeWriter == null && (typeWriter = GetTypeWriter(content.GetType(), writerData ?? new Dictionary<string, object>())) == null)
        throw new ContentLoadException(string.Format("ContentTypeWriter not found for content type {0}", content.GetType()));

      ContentWriter writer = CreateWriter(_typeWriterManager, outStream, _buildOptions.CompressOutput, _buildOptions.GetIdentifierString(content.GetType()));

      //typeWriterManager.RegisterTypeWriter(content.GetType(), typeWriter);
      writer.WriteObject(content, typeWriter);
      writer.Flush();
    }

    public ContentItem LoadContentItem(string filename, 
      IContentImporter importer = null,
      IContentProcessor processor = null,
      Dictionary<string, object> opaqueData = null)
    {
      if (opaqueData == null)
        opaqueData = new Dictionary<string, object>();
      if (importer == null)
        importer = CreateImporter(FindImporterByExtension(Path.GetExtension(filename)), opaqueData);
      if (processor == null)
        processor = CreateProcessor(FindDefaultProcessor(importer.GetType()), opaqueData);

      object item;

      using (FileStream stream = File.OpenRead(filename))
      {
        item = importer.Import(stream, this);
      }
      _processorContext.PushDirectory(Path.GetDirectoryName(filename));
      item = processor.Process(item, _processorContext);
      _processorContext.PopDirectory();

      return (ContentItem)item;
    }

    public bool NeedsToBuild(string filename, string outputDir = "", string importerName = null, string processorName = null)
    {
      IContentImporter importer;
      if ((importer = CreateImporter(importerName ?? FindImporterByExtension(Path.GetExtension(filename)), new Dictionary<string, object>())) == null)
        throw new ContentLoadException(importerName != null ?
          string.Format("Importer {0} not found.", importerName) :
          string.Format("Importer not found that handles extension {0}", Path.GetExtension(filename)));
      IContentProcessor processor;
      if ((processor = CreateProcessor(processorName ?? FindDefaultProcessor(importer.GetType()), new Dictionary<string, object>())) == null)
        throw new ContentLoadException(processorName != null ?
          string.Format("Processor {0} not found.", processorName) :
          string.Format("Processor not found that handles type {0}", importer.GetType().Name));
      string outputFilename = Path.Combine(outputDir, _buildOptions.GetOutputFilename(processor.OutputType != typeof(object) ? processor.OutputType : importer.OutputType, filename));
      return _contentSaver.GetLastModified(_contentSaver.GetPath(outputFilename, processor.OutputType != typeof(object) ? processor.OutputType : importer.OutputType), processor.OutputType != typeof(object) ? processor.OutputType : importer.OutputType) < File.GetLastWriteTime(filename);
    }

    public string GetOutputName(string filename, string outputDir = "", string importerName = null, string processorName = null)
    {
      IContentImporter importer;
      if ((importer = CreateImporter(importerName ?? FindImporterByExtension(Path.GetExtension(filename)), new Dictionary<string, object>())) == null)
        throw new ContentLoadException(importerName != null ?
          string.Format("Importer {0} not found.", importerName) :
          string.Format("Importer not found that handles extension {0}", Path.GetExtension(filename)));
      IContentProcessor processor;
      if ((processor = CreateProcessor(processorName ?? FindDefaultProcessor(importer.GetType()), new Dictionary<string, object>())) == null)
        throw new ContentLoadException(processorName != null ?
          string.Format("Processor {0} not found.", processorName) :
          string.Format("Processor not found that handles type {0}", importer.GetType().Name));
      return _contentSaver.GetPath(Path.Combine(outputDir, _buildOptions.GetOutputFilename(processor.OutputType != typeof(object) ? processor.OutputType : importer.OutputType, filename)), processor.OutputType != typeof(object) ? processor.OutputType : importer.OutputType);
    }

    public IContentImporter CreateImporter(string name, Dictionary<string, object> importerData)
    {
      if (_importers == null)
        ProcessAssemblies();

      var i = _importers.FirstOrDefault(a => a.type.Name == name);
      if (i.type == null)
        return null;
      return CreateImporter(i.type, importerData);
    }

    public IContentImporter CreateImporter(Type type, Dictionary<string, object> importerData)
    {
      if (_importers == null)
        ProcessAssemblies();

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

    public string FindImporterByExtension(string ext)
    {
      if (_importers == null)
        ProcessAssemblies();

      foreach (ImporterInfo info in _importers)
      {
        if(info.attribue.FileExtensions.Any(e => e.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
          return info.type.Name;
      }

      return null;
    }

    public string FindDefaultProcessor(Type type)
    {
      if (_importers == null)
        ProcessAssemblies();

      foreach (var info in _importers)
      {
        if (info.type == type)
          return info.attribue.DefaultProcessor;
      }
      return null;
    }

    public IContentProcessor CreateProcessor(string name, Dictionary<string, object> processorData)
    {
      if (_processors == null)
        ProcessAssemblies();

      var p = _processors.FirstOrDefault(a => a.type.Name == name);
      if (p.type == null)
        return null;
      return CreateProcessor(p.type, processorData);
    }

    public IContentProcessor CreateProcessor(Type type, Dictionary<string, object> processorData)
    {
      if (_processors == null)
        ProcessAssemblies();

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

    /// <summary>
    /// Retrieves the worker writer for the specified type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The worker writer.</returns>
    /// <remarks>This should be called from the ContentTypeWriter.Initialize method.</remarks>
    public ContentTypeWriter GetTypeWriter(Type type, Dictionary<string, object> writerData)
    {
      if (typeWriterMap.Count == 0)
        ProcessAssemblies();
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
        foreach (var param in writerData)
        {
          var propInfo = typeWriterType.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
          if (propInfo == null || propInfo.GetSetMethod(false) == null)
            continue;

          if (propInfo.PropertyType.IsInstanceOfType(param.Value))
            propInfo.SetValue(result, param.Value, null);
          else
          {
            // find a type converter for this property
            var typeConverter = TypeDescriptor.GetConverter(propInfo.PropertyType);
            if (typeConverter.CanConvertFrom(param.Value.GetType()))
            {
              var propValue = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
              propInfo.SetValue(result, propValue, null);
            }
          }
        }

        MethodInfo dynMethod = result.GetType().GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
        dynMethod.Invoke(result, new object[] { _typeWriterManager });
      }
      return result;
    }

    public ContentTypeWriter GetTypeWriter(string name, Type contentType, Dictionary<string, object> writerData)
    {
      if (typeWriterMap.Count == 0)
        ProcessAssemblies();
      Type writerType = typeWriterMap.FirstOrDefault(t => t.Value.Name == name).Value;
      if (writerType == null)
        return null;

      var args = writerType.BaseType.GetGenericArguments();
      if (args.Length == 0 || !contentType.Equals(args[0]))
        return null;

      ContentTypeWriter result = (ContentTypeWriter)Activator.CreateInstance(writerType);
      if (result != null)
      {
        foreach (var param in writerData)
        {
          var propInfo = writerType.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
          if (propInfo == null || propInfo.GetSetMethod(false) == null)
            continue;

          if (propInfo.PropertyType.IsInstanceOfType(param.Value))
            propInfo.SetValue(result, param.Value, null);
          else
          {
            // find a type converter for this property
            var typeConverter = TypeDescriptor.GetConverter(propInfo.PropertyType);
            if (typeConverter.CanConvertFrom(param.Value.GetType()))
            {
              var propValue = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
              propInfo.SetValue(result, propValue, null);
            }
          }
        }

        MethodInfo dynMethod = result.GetType().GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
        dynMethod.Invoke(result, new object[] { _typeWriterManager });
      }
      return result;
    }

    public string DisplayNameFromTypeName(string name, string postRemove = null)
    {
      if (!string.IsNullOrEmpty(postRemove) && name.EndsWith(postRemove))
        name = name.Substring(0, name.Length - postRemove.Length);
      return _camelCaseRegex.Replace(name, "$1 ");
    }

    private void ProcessAssemblies()
    {
      _importers = new List<ImporterInfo>();
      _processors = new List<ProcessorInfo>();

      foreach (string assemblyPath in _assemblies)
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
              if(string.IsNullOrEmpty(importerAttribute.DisplayName))
                importerAttribute.DisplayName = DisplayNameFromTypeName(t.Name);
              _importers.Add(new ImporterInfo { attribue = importerAttribute, type = t });
            }
            else
            {
              // If no attribute specify default one
              var importerAttribute = new ContentImporterAttribute(".*");
              importerAttribute.DefaultProcessor = "";
              importerAttribute.DisplayName = DisplayNameFromTypeName(t.Name);
              _importers.Add(new ImporterInfo { attribue = importerAttribute, type = t });
            }
          }
          else if (t.GetInterface("IContentProcessor") != null)
          {
            var attributes = t.GetCustomAttributes(typeof(ContentProcessorAttribute), false);
            if (attributes.Length != 0)
            {
              var processorAttribute = attributes[0] as ContentProcessorAttribute;
              if (string.IsNullOrEmpty(processorAttribute.DisplayName))
                processorAttribute.DisplayName = DisplayNameFromTypeName(t.Name);
              _processors.Add(new ProcessorInfo { attribue = processorAttribute, type = t });
            }
            else
            {
              ContentProcessorAttribute processorAttribute = new ContentProcessorAttribute();
              processorAttribute.DisplayName = DisplayNameFromTypeName(t.Name);
              _processors.Add(new ProcessorInfo { attribue = processorAttribute, type = t });
            }
          }
          else if (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == contentTypeWriterType)
          {
            Type baseType = t.BaseType;
            while ((baseType != null) && baseType.IsGenericType && (baseType.GetGenericTypeDefinition() != contentTypeWriterType))
              baseType = baseType.BaseType;
            if (baseType != null)
            {
              typeWriterMap.Add(baseType, t);
            }
          }
        }
      }
    }

    public Stream GetStream(string path)
    {
      if (path.StartsWith("builtin://"))
      {
        Uri uri = new Uri(path);
        path = uri.GetLeftPart(UriPartial.Path).Replace("builtin://", "");
        if (path.EndsWith("/"))
          path = path.Substring(0, path.Length - 1);
        return typeof(BuiltinContentLoader).Assembly.GetManifestResourceStream(string.Format("Minotaur.Content.Resources.{0}", path.Replace('/', '.')));
      }
      return null;
    }

    private bool IsBuiltin(string path)
    {
      return path.StartsWith("builtin://");
    }
  }
}
