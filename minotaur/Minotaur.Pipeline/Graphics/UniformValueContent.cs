
namespace Minotaur.Pipeline.Graphics
{
  public class UniformValueContent : ContentItem
  {
    public Minotaur.Graphics.UniformValueType ValueType { get; private set; }

    public object[] Values { get; private set; }

    public UniformValueContent(Minotaur.Graphics.UniformValueType valueType, object[] values)
    {
      ValueType = valueType;
      Values = values;
    }
  }
}
