using System.Collections.Generic;

namespace Minotaur.Pipeline.Graphics
{
  public class ExternalReferenceContent<T> : ContentItem
  {
    public string FileName { get; set; }
    public ContentManager Manager { get; internal set; }
    public bool HasBeenBuilt { get; set; }
    public string ImporterName { get; set; }
    public string ProcessorName { get; set; }
    public string WriterName { get; set; }
    public Dictionary<string, object> ImporterData { get; set; }
    public Dictionary<string, object> ProcessorData { get; set; }
    public Dictionary<string, object> WriterData { get; set; }

    /// <summary>
    /// Create an ExternalReferenceContent instance that has already been built.
    /// </summary>
    /// <param name="fileName"></param>
    public ExternalReferenceContent(string fileName)
    {
      FileName = fileName;
      HasBeenBuilt = true;
    }

    // Create an ExternalReferenceCountent instance that has not yet been built.
    public ExternalReferenceContent(string fileName, ContentManager manager)
    {
      FileName = fileName;
      Manager = manager;
      HasBeenBuilt = false;
    }

    public void Build()
    {
      FileName = Manager.BuildContent<T>(FileName, ImporterName, ImporterData,
        ProcessorName, ProcessorData, WriterName, WriterData).FileName;
    }
  }
}
