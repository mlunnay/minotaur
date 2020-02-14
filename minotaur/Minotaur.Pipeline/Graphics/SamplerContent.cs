using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Minotaur.Pipeline.Graphics
{
  public class SamplerContent : ContentItem
  {
    public static Color4 DefaultBorderColor = new Color4(0, 0, 0, 0);
    public static TextureMinFilter DefaultMinFilter = TextureMinFilter.NearestMipmapLinear;
    public static TextureMagFilter DefaultMagFilter = TextureMagFilter.Linear;
    public static float DefaultLodMin = -1000;
    public static float DefaultLodMax = 1000;
    public static float DefaultLodBias = 0;
    public static TextureWrapMode DefaultWrapS = TextureWrapMode.Repeat;
    public static TextureWrapMode DefaultWrapT = TextureWrapMode.Repeat;
    public static TextureWrapMode DefaultWrapR = TextureWrapMode.Repeat;

    public ExternalReferenceContent<TextureContent> Texture { get; set; }
    public byte Slot { get; set; }
    public Color4 BorderColor { get; set; }
    public TextureMinFilter MinFilter { get; set; }
    public TextureMagFilter MagFilter { get; set; }
    public float LodMin { get; set; }
    public float LodMax { get; set; }
    public float LodBias { get; set; }
    public TextureWrapMode WrapS { get; set; }
    public TextureWrapMode WrapT { get; set; }
    public TextureWrapMode WrapR { get; set; }

    public SamplerContent()
    {
      BorderColor = DefaultBorderColor;
      MinFilter = DefaultMinFilter;
      MagFilter = DefaultMagFilter;
      LodMin = DefaultLodMin;
      LodMax = DefaultLodMax;
      LodBias = DefaultLodBias;
      WrapS = DefaultWrapS;
      WrapT = DefaultWrapT;
      WrapR = DefaultWrapR;
    }

    /// <summary>
    /// Returns a dictionary of parameters that are different from the default values
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> GetParameters()
    {
      Dictionary<string, object> dict = new Dictionary<string, object>();
      if (BorderColor != DefaultBorderColor)
        dict["BorderColor"] = BorderColor;
      if (MinFilter != DefaultMinFilter)
        dict["MinFilter"] = MinFilter;
      if (MagFilter != DefaultMagFilter)
        dict["MagFilter"] = MagFilter;
      if (LodMin != DefaultLodMin)
        dict["LodMin"] = LodMin;
      if (LodMax != DefaultLodMax)
        dict["LodMax"] = LodMax;
      if (LodBias != DefaultLodBias)
        dict["LodBias"] = LodBias;
      if (WrapS != DefaultWrapS)
        dict["WrapS"] = WrapS;
      if (WrapT != DefaultWrapT)
        dict["WrapT"] = WrapT;
      if (WrapR != DefaultWrapR)
        dict["WrapR"] = WrapR;
      return dict;
    }
  }
}
