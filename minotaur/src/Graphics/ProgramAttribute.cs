using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class ProgramAttribute
  {
    public string Name { get; private set; }
    public int Slot { get; private set; }
    public int Size { get; private set; }
    public ActiveAttribType Type { get; private set; }

    public ProgramAttribute(string name,
      int slot, int size, ActiveAttribType type)
    {
      Name = name;
      Slot = slot;
      Size = size;
      Type = type;
    }
  }
}
