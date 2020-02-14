using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class ProgramUniform
  {
    public string Name { get; private set; }
    public int Slot { get; private set; }
    public int Size { get; private set; }
    public ActiveUniformType Type { get; private set; }
    internal IUniformValue Cache;

    public ProgramUniform(string name,
      int slot, int size, ActiveUniformType type)
    {
      Name = name;
      Slot = slot;
      Size = size;
      Type = type;
    }
  }
}
