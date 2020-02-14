
namespace Minotaur.Pipeline.Graphics
{
  public class ShaderSourceContent : ContentItem
  {
    /// <summary>
    /// Original source filename for debuging purposes.
    /// </summary>
    public string FileName { get; set; }
    public OpenTK.Graphics.OpenGL.ShaderType ShaderType { get; set; }
    public string Source { get; set; }
  }
}
