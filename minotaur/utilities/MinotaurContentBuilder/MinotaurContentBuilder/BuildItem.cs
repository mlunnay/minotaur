using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinotaurContentBuilder
{
  public class BuildItem
  {
    public string SourceFile { get; set; }
    public string DestinationFile { get; set; }
    public string Importer { get; set; }
    public string Processor { get; set; }
    public string Writer { get; set; }

    public Dictionary<string, object> ImporterParameters { get; set; }
    public Dictionary<string, object> ProcessorParameters { get; set; }
    public Dictionary<string, object> WriterParameters { get; set; }
    public Dictionary<string, object> GeneralParameters { get; set; }

    public BuildItem()
    {
      ImporterParameters = new Dictionary<string, object>();
      ProcessorParameters = new Dictionary<string, object>();
      WriterParameters = new Dictionary<string, object>();
      GeneralParameters = new Dictionary<string, object>();
    }
  }
}
