
namespace Minotaur.Pipeline.Processors
{
  [ContentProcessor(DisplayName = "Pass Through Processor")]
  public class PassThroughProcessor : ContentProcessor<object, object>
  {
    public override object Process(object input, ContentProcessorContext context)
    {
      return input;
    }
  }
}
