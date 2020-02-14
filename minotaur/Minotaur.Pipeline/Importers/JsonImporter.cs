using System.IO;
using fastJSON;

namespace Minotaur.Pipeline.Importers
{
  [ContentImporter(".json", DisplayName = "JSON Importer")]
  public class JsonImporter : ContentImporter<object>
  {
    public override object Import(FileStream stream, ContentManager manager)
    {
      return JSON.Instance.Parse(new StreamReader(stream).ReadToEnd());
    }
  }
}
