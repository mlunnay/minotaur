using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueTexture : IUniformValue, IComparable<IUniformValue>
  {
    public int TextureSlot;
    internal Texture Texture;

    private UniformValueTexture()
    {

    }

    public UniformValueTexture(Texture texture)
    {
      Texture = texture;
    }

    public override bool Equals(object obj)
    {
      UniformValueTexture o = obj as UniformValueTexture;
      if (o == null) return false;
      return Texture == o.Texture;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + Texture.GetHashCode();
      return hash;
    }

    #region IUniformValue Members

    public IUniformValue Default()
    {
      return new UniformValueTexture();
    }

    public void Apply(int location)
    {
      if(Texture == null)
        return;
      GL.Uniform1(location, TextureSlot);
      GL.ActiveTexture(TextureUnit.Texture0 + TextureSlot);
      Texture.Bind();
    }

    public void Set(IUniformValue source)
    {
      UniformValueTexture other = source as UniformValueTexture;
      if (other == null) return;
      Texture = other.Texture; ;
    }

    public object Clone()
    {
      return new UniformValueTexture(Texture) { TextureSlot = TextureSlot };
    }

    public int CompareTo(IUniformValue other)
    {
      UniformValueTexture oTex = other as UniformValueTexture;
      if (oTex != null)
        return Texture.ID.CompareTo(oTex.Texture.ID);
      UniformValueSampler oSampler = other as UniformValueSampler;
      if (oSampler != null)
        return Texture.ID.CompareTo(oSampler.Sampler.Texture.ID);
      return -1;
    }

    #endregion
  }
}
