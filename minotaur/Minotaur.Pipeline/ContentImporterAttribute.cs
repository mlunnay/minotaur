using System;
using System.Collections.Generic;

namespace Minotaur.Pipeline
{
  public class ContentImporterAttribute : Attribute
  {
    private List<string> extensions = new List<string>();

    public bool CacheImportedData { get; set; }

    public string DefaultProcessor { get; set; }

    public string DisplayName { get; set; }

    public IEnumerable<string> FileExtensions { get { return extensions; } }

    public ContentImporterAttribute(string fileExtension)
    {
      extensions.Add(fileExtension);
    }

    public ContentImporterAttribute(params string[] fileExtensions)
    {
      extensions.AddRange(fileExtensions);
    }
  }
}
