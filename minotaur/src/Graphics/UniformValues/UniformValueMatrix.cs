using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueMatrix : IUniformValue
  {
    Matrix4 M;
    bool transpose;

    public UniformValueMatrix(Matrix4 m, bool transpose = false)
    {
      M = m;
    }

    public void Apply(int slot)
    {
      GL.UniformMatrix4(slot, transpose, ref M);
    }

    public override bool Equals(object obj)
    {
      UniformValueMatrix o = obj as UniformValueMatrix;
      if (o == null) return false;
      return M == o.M && transpose == o.transpose;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + M.GetHashCode();
      hash = hash * 37 + transpose.GetHashCode();
      return hash;
    }

    public IUniformValue Default()
    {
      return new UniformValueMatrix(Matrix4.Identity);
    }

    public void Set(IUniformValue source)
    {
      UniformValueMatrix other = source as UniformValueMatrix;
      if (other == null) return;
      M = other.M;
      transpose = other.transpose;
    }

    public object Clone()
    {
      return new UniformValueMatrix(M, transpose);
    }
  }
}
