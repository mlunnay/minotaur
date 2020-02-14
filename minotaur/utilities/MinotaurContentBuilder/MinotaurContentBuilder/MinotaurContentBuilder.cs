using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;
using System.IO;
using Minotaur.Pipeline;
using log4net;
using OpenTK;
using OpenTK.Graphics;

namespace MinotaurContentBuilder
{
  public class MinotaurContentBuilder
  {
    private static ILog _log = LogManager.GetLogger(typeof(MinotaurContentBuilder));
    private static string _outputPath = "";
    private static string _inputName;
    private static ContentManager _contentManager;
    private static List<BuildItem> _buildItems = new List<BuildItem>();
    private static string _baseDirectory;
    private static bool _forceRebuild;
    private static string _asset;

    public class BuildSource : IBuildSource
    {
      private List<BuildItem> _buildItems;

      public BuildSource(List<BuildItem> buildItems)
      {
        _buildItems = buildItems;
      }

      public bool HasBuildItem(string path)
      {
        return _buildItems.Any(b => Path.GetFullPath(b.SourceFile) == Path.GetFullPath(path));
      }
    }

    static void Main(string[] args)
    {
      log4net.Config.BasicConfigurator.Configure(new log4net.Appender.DebugAppender());
      log4net.Config.BasicConfigurator.Configure(new log4net.Appender.ConsoleAppender());
      _log.Logger.Repository.Threshold = log4net.Core.Level.Warn;
      ((log4net.Repository.Hierarchy.Logger)_log.Logger).Level = log4net.Core.Level.Warn;

      if (!ParseCommandLine(args))
        return;

      _contentManager = new ContentManager(_log, new BasicBuildOptions(forceRebuild: _forceRebuild), new BasicFileContentSaver(_outputPath), new BuildSource(_buildItems));


      _baseDirectory = Path.GetDirectoryName(_asset ?? _inputName);
      if (string.IsNullOrEmpty(_outputPath))
        _outputPath = _baseDirectory;
      //_contentManager.ProcessorContext.PushDirectory(_baseDirectory);

      if (string.IsNullOrEmpty(_asset))
      {
        if (!ParseConfigFile())
          return;
      }
      else
        _buildItems.Add(new BuildItem() { SourceFile = _asset });

      // need to create a OpenGL context for shader compiling etc.
      NativeWindow window = new NativeWindow();
      GraphicsContext context = new GraphicsContext(GraphicsMode.Default, window.WindowInfo);
      context.MakeCurrent(window.WindowInfo);
      context.LoadAll();

      if (!string.IsNullOrEmpty(_outputPath) && !Directory.Exists(_outputPath))
        Directory.CreateDirectory(_outputPath);

      // process each of the build items
      foreach (BuildItem item in _buildItems)
      {
        if (string.IsNullOrEmpty(item.DestinationFile))
          item.DestinationFile = Path.GetFileName(item.SourceFile);
        //  item.DestinationFile = string.Format("{0}.meb", Path.GetFileNameWithoutExtension(Path.GetFileName(item.SourceFile)));

        string directory = Path.GetDirectoryName(item.DestinationFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(Path.Combine(_outputPath, directory)))
          Directory.CreateDirectory(Path.Combine(_outputPath, directory));

        Dictionary<string, object> importerParameters = new Dictionary<string, object>();
        Dictionary<string, object> processorParameters = new Dictionary<string, object>();
        Dictionary<string, object> writerParameters = new Dictionary<string, object>();

        (new[] { item.GeneralParameters, item.ImporterParameters }).SelectMany(x => x).ToList().ForEach(x => importerParameters[x.Key] = x.Value);
        (new[] { item.GeneralParameters, item.ProcessorParameters }).SelectMany(x => x).ToList().ForEach(x => processorParameters[x.Key] = x.Value);
        (new[] { item.GeneralParameters, item.WriterParameters }).SelectMany(x => x).ToList().ForEach(x => writerParameters[x.Key] = x.Value);

        if (_forceRebuild || _contentManager.NeedsToBuild(item.SourceFile, _outputPath, item.Importer, item.Processor))
        {
          string outputName = _contentManager.GetOutputName(item.DestinationFile, Path.GetFullPath(string.IsNullOrEmpty(_outputPath) ? "." : _outputPath), item.Importer, item.Processor);
          using (FileStream stream = new FileStream(outputName, FileMode.Create))
          {
            _contentManager.LoadContentStream(item.SourceFile, stream,
              item.Importer, importerParameters,
              item.Processor, processorParameters,
              item.Writer, writerParameters);
          }
        }
      }
    }

    // returns true for successfull parsing 
    private static bool ParseConfigFile()
    {
      using (StreamReader reader = new StreamReader(_inputName))
      {
        try
        {
          Dictionary<string, object> config = (Dictionary<string, object>)fastJSON.JSON.Instance.Parse(reader.ReadToEnd());

          if (!config.ContainsKey("builditems"))
          {
            Console.WriteLine("Invalid Config file: missing key builditems");
            return false;
          }

          if (config.ContainsKey("assemblies"))
          {
            foreach (string assembly in (List<string>)config["assemblies"])
            {
              _contentManager.AddAssembly(assembly);
            }
          }

          Dictionary<string, object> globalParameters = new Dictionary<string, object>(config);
          globalParameters.Remove("builditems");
          globalParameters.Remove("assemblies");

          int count = 1;
          foreach (Dictionary<string, object> item in ((List<object>)config["builditems"]).Cast<Dictionary<string, object>>())
          {
            BuildItem buildItem = new BuildItem();
            if (!item.ContainsKey("source"))
            {
              Console.WriteLine("Build Item {0} is missing the source parameter.", count);
              return false;
            }
            buildItem.SourceFile = (string)item["source"];
            if (!Path.IsPathRooted(buildItem.SourceFile))
              buildItem.SourceFile = Path.Combine(_baseDirectory, buildItem.SourceFile);

            if (item.ContainsKey("destination"))
            {
              buildItem.DestinationFile = (string)item["destination"];
            }
            if (item.ContainsKey("importer"))
            {
              List<object> list = (List<object>)item["importer"];
              if (list.Count == 1)
              {
                if (list[0] is string)
                  buildItem.Importer = (string)list[0];
                else
                  buildItem.ImporterParameters = (Dictionary<string, object>)list[0];
              }
              else if (list[0] is string)
              {
                buildItem.Importer = (string)list[0];
                buildItem.ImporterParameters = (Dictionary<string, object>)list[1];
              }
              else
              {
                buildItem.Importer = (string)list[1];
                buildItem.ImporterParameters = (Dictionary<string, object>)list[0];
              }
            }
            if (item.ContainsKey("processor"))
            {
              List<object> list = (List<object>)item["processor"];
              if (list.Count == 1)
              {
                if (list[0] is string)
                  buildItem.Processor = (string)list[0];
                else
                  buildItem.ProcessorParameters = (Dictionary<string, object>)list[0];
              }
              else if (list[0] is string)
              {
                buildItem.Processor = (string)list[0];
                buildItem.ProcessorParameters = (Dictionary<string, object>)list[1];
              }
              else
              {
                buildItem.Processor = (string)list[1];
                buildItem.ProcessorParameters = (Dictionary<string, object>)list[0];
              }
            }
            if (item.ContainsKey("exporter"))
            {
              List<object> list = (List<object>)item["exporter"];
              if (list.Count == 1)
              {
                if (list[0] is string)
                  buildItem.Writer = (string)list[0];
                else
                  buildItem.WriterParameters = (Dictionary<string, object>)list[0];
              }
              else if (list[0] is string)
              {
                buildItem.Writer = (string)list[0];
                buildItem.WriterParameters = (Dictionary<string, object>)list[1];
              }
              else
              {
                buildItem.Writer = (string)list[1];
                buildItem.WriterParameters = (Dictionary<string, object>)list[0];
              }
            }

            Dictionary<string, object> parameters = new Dictionary<string, object>(item);
            parameters.Remove("importer");
            parameters.Remove("processor");
            parameters.Remove("exporter");
            parameters.Remove("source");
            parameters.Remove("destination");
            
            (new[] {globalParameters, parameters}).SelectMany(x => x).ToList().ForEach(x => buildItem.GeneralParameters[x.Key] = x.Value);

            _buildItems.Add(buildItem);
            count++;
          }
        }
        catch (FileLoadException)
        {
          Console.WriteLine("Invalid config file.");
          return false;
        }
      }

      return true;
    }

    private static bool ParseCommandLine(string[] args)
    {
      bool showHelp = false;
      OptionSet p = new OptionSet()
      {
        {"a=|asset=", "Build a single asset with all default settings.", a => _asset = a},
        {"o=|output=", "The output directory to write the binary files to.", a => _outputPath = a},
        {"f|force", "Force rebuild of all assets.", a => _forceRebuild = true},
        {"h|help|?", "Show this help mesage.", a => showHelp = true},
      };


      List<string> extra = p.Parse(args);

      if (showHelp)
      {
        ShowHelp(p);
        return false;
      }

      if (_asset == null)
      {
        if (extra.Count != 1)
        {
          Console.WriteLine("Error: Config file was not given.");
          ShowHelp(p);
          return false;
        }

        _inputName = extra[0];
      }

      return true;
    }

    private static void ShowHelp(OptionSet options)
    {
      Console.WriteLine("Usage: MinotaurContentBuilder CONFIG");
      Console.WriteLine("Converts assets into Minotaur binary files.");
      Console.WriteLine();
      Console.WriteLine("Options:");
      options.WriteOptionDescriptions(Console.Out);
    }
  }
}
