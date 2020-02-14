using System;

namespace Minotaur.Graphics
{
  public interface IUniformValue : ICloneable
  {
    IUniformValue Default();
    void Apply(int location);
    void Set(IUniformValue source);
  }
}
