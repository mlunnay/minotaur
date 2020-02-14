using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
using Minotaur.Core;
using OpenTK.Graphics;

namespace Minotaur.Graphics
{
  /// <summary>
  /// A texture sampler.
  /// </summary>
  public class Sampler : GraphicsResource, ICloneable
  {
    private static List<Sampler> _samplerInstances = new List<Sampler>();
    private int _id;

    public Texture Texture { get; set; }

    public Color4 BorderColor
    {
      get
      {
        float[] rgba = new float[4];
        GL.GetSamplerParameter(_id, SamplerParameter.TextureBorderColor, rgba);
        return new Color4(rgba[0], rgba[1], rgba[2], rgba[3]);
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureBorderColor, new float[] { value.R, value.G, value.B, value.A });
      }
    }

    public TextureMinFilter MinFilter
    {
      get
      {
        int value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureMinFilter, out value);
        return (TextureMinFilter)value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureMinFilter, (int)value);
      }
    }

    public TextureMagFilter MagFilter
    {
      get
      {
        int value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureMagFilter, out value);
        return (TextureMagFilter)value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureMagFilter, (int)value);
      }
    }

    public float LodMin
    {
      get
      {
        float value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureMinLod, out value);
        return value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureMinLod, value);
      }
    }

    public float LodMax
    {
      get
      {
        float value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureMaxLod, out value);
        return value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureMaxLod, value);
      }
    }

    public float LodBias
    {
      get
      {
        float value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureLodBias, out value);
        return value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureLodBias, value);
      }
    }

    public TextureWrapMode WrapS
    {
      get
      {
        int value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureWrapS, out value);
        return (TextureWrapMode)value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureWrapS, (int)value);
      }
    }

    public TextureWrapMode WrapT
    {
      get
      {
        int value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureWrapT, out value);
        return (TextureWrapMode)value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureWrapT, (int)value);
      }
    }

    public TextureWrapMode WrapR
    {
      get
      {
        int value;
        GL.GetSamplerParameter(_id, SamplerParameter.TextureWrapR, out value);
        return (TextureWrapMode)value;
      }
      set
      {
        GL.SamplerParameter(_id, SamplerParameter.TextureWrapR, (int)value);
      }
    }

    private Sampler()
    {
      GL.GenSamplers(1, out _id);
    }

    private Sampler(int id)
    {
      _id = id;
    }

    /// <summary>
    /// Creates a copy of this sampler but with a new opengl sampler id.
    /// This will mean that the copy is no longer part of the static creation and should be used before calling any of the property setters except for Texture.
    /// </summary>
    /// <returns></returns>
    public Sampler Copy()
    {
      return new Sampler()
      {
        BorderColor = BorderColor,
        MinFilter = MinFilter,
        MagFilter = MagFilter,
        LodMin = LodMin,
        LodMax = LodMax,
        LodBias = LodBias,
        WrapS = WrapS,
        WrapT = WrapT,
        WrapR = WrapR,
        Texture = Texture
      };
    }

    /// <summary>
    /// This method ensures that only 1 sampler id is created for each needed combination of parameters.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="borderColor"></param>
    /// <param name="minFilter"></param>
    /// <param name="magFilter"></param>
    /// <param name="lodMin"></param>
    /// <param name="lodMax"></param>
    /// <param name="lodBias"></param>
    /// <param name="wrapS"></param>
    /// <param name="wrapT"></param>
    /// <param name="wrapR"></param>
    /// <returns></returns>
    public static Sampler Create(Texture texture = null,
      Color4 borderColor = default(Color4),
      TextureMinFilter minFilter = TextureMinFilter.NearestMipmapLinear,
      TextureMagFilter magFilter = TextureMagFilter.Linear,
      float lodMin = -1000,
      float lodMax = 1000,
      float lodBias = 0,
      TextureWrapMode wrapS = TextureWrapMode.Repeat,
      TextureWrapMode wrapT = TextureWrapMode.Repeat,
      TextureWrapMode wrapR = TextureWrapMode.Repeat)
    {
      Sampler sampler = FindOrCreateSampler(borderColor,
        minFilter,
        magFilter,
        lodMin,
        lodMax,
        lodBias,
        wrapS,
        wrapT,
        wrapR);
      sampler.Texture = texture;

      return sampler;
    }

    private static Sampler FindOrCreateSampler(Color4 borderColor = default(Color4),
      TextureMinFilter minFilter = TextureMinFilter.NearestMipmapLinear,
      TextureMagFilter magFilter = TextureMagFilter.Linear,
      float lodMin = -1000,
      float lodMax = 1000,
      float lodBias = 0,
      TextureWrapMode wrapS = TextureWrapMode.Repeat,
      TextureWrapMode wrapT = TextureWrapMode.Repeat,
      TextureWrapMode wrapR = TextureWrapMode.Repeat)
    {
      Sampler sampler = _samplerInstances.FirstOrDefault(s => s.BorderColor == borderColor &&
        s.MinFilter == minFilter &&
        s.MagFilter == magFilter &&
        s.LodMin == lodMin &&
        s.LodMax == lodMax &&
        s.LodBias == lodBias &&
        s.WrapS == wrapS &&
        s.WrapT == wrapT &&
        s.WrapR == wrapR);

      if (sampler == null)
      {
        sampler = new Sampler()
        {
          BorderColor = borderColor,
          MinFilter = minFilter,
          MagFilter = magFilter,
          LodMin = lodMin,
          LodMax = lodMax,
          LodBias = lodBias,
          WrapS = wrapS,
          WrapT = wrapT,
          WrapR = wrapR
        };
        _samplerInstances.Add(sampler);
      }

      return new Sampler(sampler._id);
    }

    public void Bind()
    {
      Texture.Bind();
      GL.BindSampler(Texture.ID, _id);
    }

    public void Unbind()
    {
      GL.BindSampler(Texture.ID, 0);
      Texture.Unbind();
    }

    public override bool Equals(object obj)
    {
      Sampler o = (Sampler)obj;
      if (o == null)
        return false;

      return BorderColor == o.BorderColor
        && MinFilter == o.MinFilter
        && MagFilter == o.MagFilter
        && LodMin == o.LodMin
        && LodMax == o.LodMax
        && LodBias == o.LodBias
        && WrapS == o.WrapS
        && WrapT == o.WrapT
        && WrapR == o.WrapR;
    }

    public bool Equals(Sampler o)
    {
      return BorderColor == o.BorderColor
        && MinFilter == o.MinFilter
        && MagFilter == o.MagFilter
        && LodMin == o.LodMin
        && LodMax == o.LodMax
        && LodBias == o.LodBias
        && WrapS == o.WrapS
        && WrapT == o.WrapT
        && WrapR == o.WrapR;
    }

    public override int GetHashCode()
    {
      int hash = 23;
      hash = hash * 37 + BorderColor.GetHashCode();
      hash = hash * 37 + MinFilter.GetHashCode();
      hash = hash * 37 + MagFilter.GetHashCode();
      hash = hash * 37 + LodMin.GetHashCode();
      hash = hash * 37 + LodMax.GetHashCode();
      hash = hash * 37 + LodBias.GetHashCode();
      hash = hash * 37 + WrapS.GetHashCode();
      hash = hash * 37 + WrapT.GetHashCode();
      hash = hash * 37 + WrapR.GetHashCode();
      return hash;
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        // no managed object to dispose so skipping if disposing
        DisposalManager.Add(() => GL.DeleteSamplers(1, new[] { _id }));
        _id = 0;
      }
      IsDisposed = true;
    }

    public object Clone()
    {
      return new Sampler(_id) { Texture = Texture };
    }
  }
}
