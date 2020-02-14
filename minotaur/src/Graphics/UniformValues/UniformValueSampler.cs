using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class UniformValueSampler : IUniformValue, IComparable<IUniformValue>
  {
    private int _slot;
    private Sampler _sampler;

    public Sampler Sampler { get { return _sampler; } }

    public UniformValueSampler() { }

    public UniformValueSampler(Sampler sampler,
      int textureSlot)
    {
      _sampler = sampler;
      _slot = textureSlot;
    }

    public override bool Equals(object obj)
    {
      UniformValueSampler o = obj as UniformValueSampler;
      if (o == null) return false;
      return _sampler == o._sampler &&
        _sampler.Texture == o._sampler.Texture &&
        _slot == o._slot;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + _sampler.GetHashCode();
      hash = hash * 37 + _sampler.Texture.GetHashCode();
      hash = hash * 37 + _slot;
      return hash;
    }

    public bool Equals(UniformValueSampler o)
    {
      return _sampler == o._sampler &&
        _sampler.Texture == o._sampler.Texture &&
        _slot == o._slot;
    }

    #region IUniformValue Members

    public IUniformValue Default()
    {
      return new UniformValueSampler();
    }

    public void Apply(int location)
    {
      if (_sampler == null)
        return;
      GL.Uniform1(location, _slot);
      GL.ActiveTexture(TextureUnit.Texture0 + _slot);
      _sampler.Bind();
    }

    public void Set(IUniformValue source)
    {
      UniformValueSampler other = source as UniformValueSampler;
      if (other == null) return;
      _sampler = other._sampler;
      _slot = other._slot;
    }

    public object Clone()
    {
      return new UniformValueSampler((Sampler)_sampler.Clone(), _slot);
    }

    public int CompareTo(IUniformValue other)
    {
      UniformValueTexture oTex = other as UniformValueTexture;
      if (oTex != null)
        return _sampler.Texture.ID.CompareTo(oTex.Texture.ID);
      UniformValueSampler oSampler = other as UniformValueSampler;
      if (oSampler != null)
        return _sampler.Texture.ID.CompareTo(oSampler._sampler.Texture.ID);
      return -1;
    }

    #endregion
  }
}
