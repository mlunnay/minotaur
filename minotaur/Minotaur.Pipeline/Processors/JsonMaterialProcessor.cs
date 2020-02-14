using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Minotaur.Graphics;
using Minotaur.Pipeline.Graphics;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Pipeline.Processors
{
  [ContentProcessor(DisplayName = "Json Material Processor")]
  public class JsonMaterialProcessor : ContentProcessor<object, MaterialContent>
  {
    public static int _materialID = 0;

    public virtual bool GenerateMipmaps { get; set; }

    [DefaultValueAttribute(true)]
    public virtual bool PremultiplyAlpha { get; set; }

    [DefaultValue(ImageType.RGB)]
    public ImageType OutputType { get; set; }

    public override MaterialContent Process(object input, ContentProcessorContext context)
    {
      Dictionary<string, object> materialInput = (Dictionary<string, object>)input;
      MaterialContent material = new MaterialContent();
      if (materialInput.ContainsKey("name"))
        material.Name = (string)materialInput["name"];
      else
        material.Name = string.Format("Material_{0}", _materialID++);

      if (!materialInput.ContainsKey("passes"))
        throw new ContentException("Material json definition is missing the required key passes.");

      Dictionary<string, object> globalParameters = new Dictionary<string,object>();
      if (materialInput.ContainsKey("parameteres"))
        globalParameters = (Dictionary<string, object>)materialInput["parameters"];

      Dictionary<string, object> globalState = new Dictionary<string, object>();
      if (materialInput.ContainsKey("state"))
        globalState = (Dictionary<string, object>)materialInput["state"];

      List<ShaderSourceContent> shaderSources = new List<ShaderSourceContent>();

      int passCount = 0;
      foreach (Dictionary<string, object> item in ((List<object>)materialInput["passes"]).Cast<Dictionary<string, object>>())
      {
        MaterialContent.Pass pass = new MaterialContent.Pass();
        if (item.ContainsKey("name"))
          pass.Name = (string)item["name"];
        
        if (!item.ContainsKey("vs"))
          throw new ContentException(string.Format("Pass {0} is missing the required key vs.", passCount));
        if (!item.ContainsKey("fs"))
          throw new ContentException(string.Format("Pass {0} is missing the required key fs.", passCount));

        if (item["vs"] is string)
        {
          object content;
          pass.VertexShaderSources.Add(
            context.ContentManager.BuildContent<ShaderSourceContent>((string)item["vs"], out content,
            importerData: new Dictionary<string, object>() { { "ShaderType", "VertexShader" } }));
          shaderSources.Add((ShaderSourceContent)content);
        }
        else if (item["vs"] is List<string>)
          pass.VertexShaderSources.AddRange(((List<string>)item["vs"]).Select(
            f =>
            {
              object content;
              ExternalReferenceContent<ShaderSourceContent> result = context.ContentManager.BuildContent<
                ShaderSourceContent>(f, out content, importerData: new Dictionary<string, object>() { { "ShaderType", "VertexShader" } });
              shaderSources.Add((ShaderSourceContent)content);
              return result;
            }));
        else
          throw new ContentException(string.Format("In Pass {0} vs must be either a string or a list of strings.", passCount));

        if (item["fs"] is string)
        {
          object content;
          pass.VertexShaderSources.Add(
            context.ContentManager.BuildContent<ShaderSourceContent>((string)item["fs"], out content,
            importerData: new Dictionary<string, object>() { { "ShaderType", "FragmentShader" } }));
          shaderSources.Add((ShaderSourceContent)content);
        }
        else if (item["fs"] is List<string>)
          pass.VertexShaderSources.AddRange(((List<string>)item["fs"]).Select(
            f => 
            {
              object content;
              ExternalReferenceContent<ShaderSourceContent> result = context.ContentManager.BuildContent<ShaderSourceContent>(f, out content, importerData: new Dictionary<string, object>() { { "ShaderType", "FragmentShader" } });
              shaderSources.Add((ShaderSourceContent)content);
              return result;
            }
            ));
        else
          throw new ContentException(string.Format("In Pass {0} fs must be either a string or a list of strings.", passCount));

        Dictionary<string, object> parameters = new Dictionary<string, object>();
        if (item.ContainsKey("parameters"))
          parameters = (Dictionary<string, object>)item["parameters"];

        (new[] { globalParameters, parameters }).SelectMany(x => x).ToList().ForEach(x => pass.Parameters[x.Key] = ParseMaterialParameter(x.Value, context.ContentManager));

        Dictionary<string, object> stateData = new Dictionary<string, object>(globalState);
        if (item.ContainsKey("state"))
          ((Dictionary<string, object>)item["state"]).ToList().ForEach(x => stateData[x.Key] = x.Value);


        pass.State = CreateState(stateData);

        material.Passes.Add(pass);

        passCount++;
      }

      return material;
    }

    public RenderState CreateState(Dictionary<string, object> data)
    {
      RenderState state = new RenderState();
      if (data.ContainsKey("Cull"))
      {
        string p = (string)data["Cull"];
        if (p.Equals("front", StringComparison.InvariantCultureIgnoreCase))
        {
          state.RasterizerState = RasterizerState.CullClockwise;
        }
        else if (p.Equals("back", StringComparison.InvariantCultureIgnoreCase))
        {
          state.RasterizerState = RasterizerState.CullCounterClockwise;
        }
        else if (!p.Equals("off", StringComparison.InvariantCultureIgnoreCase))
          throw new ContentException("Cull state parameter must be one of front, back or off.");
      }

      if (data.ContainsKey("DepthWrite"))
      {
        string p = (string)data["DepthWrite"];
        if (p.Equals("on", StringComparison.InvariantCultureIgnoreCase))
        {
          state.DepthWrite = true;
        }
        else if (!p.Equals("off", StringComparison.InvariantCultureIgnoreCase))
          throw new ContentException("DepthWrite state parameter must be one of on or off.");
      }

      if (data.ContainsKey("DepthFunc"))
      {
        DepthFunction func;
        if (!Enum.TryParse<DepthFunction>((string)data["DepthWrite"], true, out func))
          throw new ContentException(string.Format("DepthFunc state parameter must be one of {0}.", EnumToStringList(typeof(DepthFunction))));
        state.DepthFunction = func;
      }

      if (data.ContainsKey("DepthOffset"))
      {
        List<object> args = (List<object>)data["DepthOffset"];
        if(args == null || args.Count != 2 || !args.All(a => a is long || a is double))
          throw new ContentException("DepthOffset state parameter must be an array of 2 integers.");
        state.DepthOffsetFactor = Convert.ToSingle(args[0]);
        state.DepthOffsetUnits = Convert.ToSingle((float)args[1]);
      }

      if (data.ContainsKey("BlendEnable"))
      {
        string p = (string)data["BlendEnable"];
        if (p.Equals("on", StringComparison.InvariantCultureIgnoreCase))
        {
          state.BlendState = BlendState.AlphaBlend;
        }
        else if (p.Equals("off", StringComparison.InvariantCultureIgnoreCase))
        {
          state.ColorSourceBlend = BlendingFactorSrc.One;
          state.AlphaSourceBlend = BlendingFactorSrc.One;
          state.ColorDestinationBlend = BlendingFactorDest.Zero;
          state.AlphaDestinationBlend = BlendingFactorDest.Zero;
        }
        else
          throw new ContentException("BlendEnable state parameter must be one of on or off.");
      }

      if (data.ContainsKey("BlendFunc"))
      {
        List<object> args = (List<object>)data["BlendFunc"];
        if (args == null || args.Count != 2 || args.Count != 4 || !args.All(a => a is string))
          throw new ContentException("BlendFunc state parameter must be an array of 2 blend factors.");
        
        BlendingFactorSrc src;
        BlendingFactorDest dst;
        if (!Enum.TryParse<BlendingFactorSrc>((string)args[0], true, out src))
          throw new ContentException(string.Format("BlendFunc states first parameter must be one of {0}.", EnumToStringList(typeof(BlendingFactorSrc))));
        state.ColorSourceBlend = src;
        if (!Enum.TryParse<BlendingFactorDest>((string)args[1], true, out dst))
          throw new ContentException(string.Format("BlendFunc states second parameter must be one of {0}.", EnumToStringList(typeof(BlendingFactorDest))));
        state.ColorDestinationBlend = dst;

        if (args.Count == 4)
        {
          if (!Enum.TryParse<BlendingFactorSrc>((string)args[3], true, out src))
            throw new ContentException(string.Format("BlendFunc states third parameter must be one of {0}.", EnumToStringList(typeof(BlendingFactorSrc))));
          state.AlphaSourceBlend = src;
          if (!Enum.TryParse<BlendingFactorDest>((string)args[4], true, out dst))
            throw new ContentException(string.Format("BlendFunc states fourth parameter must be one of {0}.", EnumToStringList(typeof(BlendingFactorDest))));
          state.AlphaDestinationBlend = dst;
        }
      }

      if (data.ContainsKey("ColorBlendMode"))
      {
        BlendEquationMode func;
        if (!Enum.TryParse<BlendEquationMode>((string)data["ColorBlendMode"], true, out func))
          throw new ContentException(string.Format("ColorBlendMode state parameter must be one of {0}.", EnumToStringList(typeof(BlendEquationMode))));
        state.ColorBlendFunction = func;
      }

      if (data.ContainsKey("AlphaBlendMode"))
      {
        BlendEquationMode func;
        if (!Enum.TryParse<BlendEquationMode>((string)data["AlphaBlendMode"], true, out func))
          throw new ContentException(string.Format("AlphaBlendMode state parameter must be one of {0}.", EnumToStringList(typeof(BlendEquationMode))));
        state.AlphaBlendFunction = func;
      }

      if (data.ContainsKey("StencilFunc"))
      {
        StencilFunction func;
        if (!Enum.TryParse<StencilFunction>((string)data["StencilFunc"], true, out func))
          throw new ContentException(string.Format("StencilFunc state parameter must be one of {0}.", EnumToStringList(typeof(StencilFunction))));
        state.StencilEnable = true;
        state.StencilFunction = func;
      }

      if (data.ContainsKey("StencilPass"))
      {
        StencilOp func;
        if (!Enum.TryParse<StencilOp>((string)data["StencilPass"], true, out func))
          throw new ContentException(string.Format("StencilPass state parameter must be one of {0}.", EnumToStringList(typeof(StencilOp))));
        state.StencilEnable = true;
        state.StencilPass = func;
      }

      if (data.ContainsKey("StencilFail"))
      {
        StencilOp func;
        if (!Enum.TryParse<StencilOp>((string)data["StencilFail"], true, out func))
          throw new ContentException(string.Format("StencilFail state parameter must be one of {0}.", EnumToStringList(typeof(StencilOp))));
        state.StencilEnable = true;
        state.StencilFail = func;
      }

      if (data.ContainsKey("StencilZFail"))
      {
        StencilOp func;
        if (!Enum.TryParse<StencilOp>((string)data["StencilZFail"], true, out func))
          throw new ContentException(string.Format("StencilZFail state parameter must be one of {0}.", EnumToStringList(typeof(StencilOp))));
        state.StencilEnable = true;
        state.StencilZFail = func;
      }

      if (data.ContainsKey("StencilWriteMask"))
      {
        long p = (long)data["StencilWriteMask"];
        if (!(data["StencilWriteMask"] is long) || p < 0 || p > 255)
          throw new ContentException("StencilWriteMask state parameter must be an integer.");
        state.StencilEnable = true;
        state.StencilWriteMask = (int)p;
      }

      if (data.ContainsKey("StencilReadMask"))
      {
        long p = (long)data["StencilReadMask"];
        if (!(data["StencilReadMask"] is long) || p < 0 || p > 255)
          throw new ContentException("StencilReadMask state parameter must be an integer.");
        state.StencilEnable = true;
        state.StencilReadMask = (int)p;
      }

      return state;
    }

    private string EnumToStringList(Type type)
    {
      return string.Join(", ", Enum.GetNames(type));
    }

    public UniformValueContent ParseMaterialParameter(object parameter, ContentManager manager)
    {
      if (parameter is long)
        return new UniformValueContent(UniformValueType.Int, new object[] { (int)parameter });
      else if (parameter is double)
        return new UniformValueContent(UniformValueType.Float, new object[] { (float)parameter });
      else if (parameter is string)
      {
        string p = (string)parameter;
        if (p.StartsWith("int(") && p.EndsWith(")"))
        {
          string[] args = p.Remove(0, 4).TrimEnd(new[] { ')' }).Split(',');
          if (args.Length > 0 && args.Length <= 4 && args.All(s => s.IsInt()))
            return new UniformValueContent(UniformValueType.Int, args.Select(s => int.Parse(s)).Cast<object>().ToArray());
          else
            throw new ContentException("Material parameter int() must contain 1-4 integers");
        }

        if (p.StartsWith("float(") && p.EndsWith(")"))
        {
          string[] args = p.Remove(0, 6).TrimEnd(new[] { ')' }).Split(',');
          if (args.Length > 0 && args.Length <= 4 && args.All(s => s.IsFloat()))
            return new UniformValueContent(UniformValueType.Float, args.Select(s => float.Parse(s)).Cast<object>().ToArray());
          else
            throw new ContentException("Material parameter float() must contain 1-4 floats");
        }

        if (p.StartsWith("uint(") && p.EndsWith(")"))
        {
          string[] args = p.Remove(0, 5).TrimEnd(new[] { ')' }).Split(',');
          if (args.Length > 0 && args.Length <= 4 && args.All(s => s.IsUInt()))
            return new UniformValueContent(UniformValueType.UInt, args.Select(s => uint.Parse(s)).Cast<object>().ToArray());
          else
            throw new ContentException("Material parameter uint() must contain 1-4 unsigned integers");
        }

        if (p.StartsWith("matrix(") && p.EndsWith(")"))
        {
          string[] args = p.Remove(0, 7).TrimEnd(new[] { ')' }).Split(',');
          if (args.Length == 16 && args.All(s => s.IsFloat()))
          {
            float[] m = args.Select(s => float.Parse(s)).ToArray();
            return new UniformValueContent(UniformValueType.Matrix,
              new object[] { new Matrix4(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]) });
          }
          else
            throw new ContentException("Material parameter matrix() must contain 16 floats");
        }

        // if previous not matched it is a texture reference so create a default sampler and add the texture.
        SamplerContent content = new SamplerContent();
        content.Texture = new ExternalReferenceContent<TextureContent>(manager.ProcessorContext.GetFilenamePath(p), manager)
        {
          ImporterData = new Dictionary<string, object>() { { "GenerateMipmaps", GenerateMipmaps }, { "PremultiplyAlpha", PremultiplyAlpha }, { "OutputType", OutputType } }
        };

        return new UniformValueContent(UniformValueType.Sampler, new object[] { content});
      }
      else if (parameter is Dictionary<string, object>)
      {
        Dictionary<string, object> p = (Dictionary<string, object>)parameter;
        SamplerContent content = new SamplerContent();

        if (!p.ContainsKey("texture"))
          throw new ContentException("Sampler is missing the required texture parameter");
        content.Texture = new ExternalReferenceContent<TextureContent>(manager.ProcessorContext.GetFilenamePath((string)p["texture"]), manager)
        {
          ImporterData = new Dictionary<string, object>() { { "GenerateMipmaps", GenerateMipmaps }, { "PremultiplyAlpha", PremultiplyAlpha }, { "OutputType", OutputType } }
        };

        object value;
        if(p.TryGetValue("minfilter", out value))
          content.MinFilter = ParseEnum<TextureMinFilter>((string)value, "minfilter");
        if (p.TryGetValue("magfilter", out value))
          content.MagFilter = ParseEnum<TextureMagFilter>((string)value, "magfilter");
        if (p.TryGetValue("lodmin", out value))
        {
          float f;
          if (!float.TryParse((string)value, out f))
            throw new ContentException("Parameter lodmin must be a float.");
            content.LodMin = f;
        }
        if (p.TryGetValue("lodmax", out value))
        {
          float f;
          if (!float.TryParse((string)value, out f))
            throw new ContentException("Parameter lodmax must be a float.");
          content.LodMax = f;
        }
        if (p.TryGetValue("lodbias", out value))
        {
          float f;
          if (!float.TryParse((string)value, out f))
            throw new ContentException("Parameter lodbias must be a float.");
          content.LodBias = f;
        }
        if (p.TryGetValue("wraps", out value))
          content.WrapS = ParseEnum<TextureWrapMode>((string)value, "wraps");
        if (p.TryGetValue("wrapt", out value))
          content.WrapT = ParseEnum<TextureWrapMode>((string)value, "wrapt");
        if (p.TryGetValue("wrapr", out value))
          content.WrapR = ParseEnum<TextureWrapMode>((string)value, "wrapr");
        if (p.TryGetValue("slot", out value))
        {
          int i;
          if (!int.TryParse((string)value, out i))
            throw new ContentException("Parameter slot must be an integer.");
          content.LodMax = i;
        }
        return new UniformValueContent(UniformValueType.Sampler, new object[] { content });
      }

      throw new ContentException(string.Format("Unknown Material parameter {0}", parameter));
    }

    private T ParseEnum<T>(string value, string parameter) where T : struct
    {
      T e;
      if (!Enum.TryParse(value, out e))
        throw new ContentException(string.Format("Unknown value for {0} parameter.", parameter));
      return e;
    }
  }
}
