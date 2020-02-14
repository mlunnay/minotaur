using System;
using System.Collections.Generic;
using Minotaur.Graphics;
using System.Text;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Linq.Expressions;

namespace Minotaur.Content
{
  public class MaterialReader : ContentTypeReader<Material>
  {
    private static Dictionary<string, Program> _programMap = new Dictionary<string, Program>();

    public MaterialReader()
      : base(new Guid("6f1be25e-7f37-4faa-b551-7a58c8d91824")) { }

    public override void Initialize(ContentTypeReaderManager manager)
    {
      manager.RegisterTypeReader<Shader>(new ShaderSourceReader());
      manager.RegisterTypeReader<IUniformValue>(new UniformValueReader());
      manager.RegisterTypeReader<Color4>(new ColorReader());
    }

    public override object Read(ContentReader reader)
    {
      string name = reader.ReadString();
      Material material = new Material(name);
      int passes = reader.ReadInt32();
      for (int i = 0; i < passes; i++)
      {
        name = reader.ReadString();
        int sourceCount = reader.ReadInt32();
        List<Shader> shaders = new List<Shader>();
        for (int j = 0; j < sourceCount; j++)
        {
          shaders.Add((Shader)reader.ReadObjectRaw<object>());
        }
        Program program = GetOrCreateProgram(shaders);
        Pass pass = new Pass(program, name);

        // read in render state
        pass.State.CullMode = reader.ReadEnum<CullMode>();
        pass.State.FillMode = reader.ReadEnum<PolygonMode>();
        pass.State.DepthBias = reader.ReadSingle();
        pass.State.SlopeScaleDepthBias = reader.ReadSingle();
        pass.State.ScissorTestEnable = reader.ReadBoolean();
        pass.State.DepthEnabled = reader.ReadBoolean();
        pass.State.DepthWrite = reader.ReadBoolean();
        pass.State.DepthFunction = reader.ReadEnum<DepthFunction>();
        pass.State.DepthOffsetFactor = reader.ReadSingle();
        pass.State.DepthOffsetUnits = reader.ReadSingle();
        pass.State.BlendColor = reader.ReadObjectRaw<Color4>();
        pass.State.ColorBlendFunction = reader.ReadEnum<BlendEquationMode>();
        pass.State.AlphaBlendFunction = reader.ReadEnum<BlendEquationMode>();
        pass.State.ColorSourceBlend = reader.ReadEnum<BlendingFactorSrc>();
        pass.State.ColorDestinationBlend = reader.ReadEnum<BlendingFactorDest>();
        pass.State.AlphaSourceBlend = reader.ReadEnum<BlendingFactorSrc>();
        pass.State.AlphaDestinationBlend = reader.ReadEnum<BlendingFactorDest>();
        pass.State.StencilEnable = reader.ReadBoolean();
        pass.State.StencilFunction = reader.ReadEnum<StencilFunction>();
        pass.State.StencilReference = reader.ReadInt32();
        pass.State.StencilPass = reader.ReadEnum<StencilOp>();
        pass.State.StencilFail = reader.ReadEnum<StencilOp>();
        pass.State.StencilZFail = reader.ReadEnum<StencilOp>();
        pass.State.StencilWriteMask = reader.ReadInt32();
        pass.State.StencilReadMask = reader.ReadInt32();

        reader.ReadObjectRaw<Dictionary<string, IUniformValue>>().ToList().ForEach(x => pass.Parameters[x.Key] = x.Value);
        
        pass.ComparisonParameters = reader.ReadObjectRaw<List<string>>();

        material.Passes.Add(pass);
      }
      return material;
    }

    /// <summary>
    /// Gets a program if it already exists based on its shaders, or creates it if it dosen't exist.
    /// </summary>
    /// <param name="shaders"></param>
    /// <returns></returns>
    public static Program GetOrCreateProgram(IEnumerable<Shader> shaders)
    {
      StringBuilder sb = new StringBuilder();
      foreach (Shader shader in shaders.OrderBy(s => s._id))
      {
        sb.Append(shader._id);
      }
      string key = sb.ToString();
      Program program;
      if (!_programMap.TryGetValue(key, out program))
      {
        program = new Program(shaders);
        _programMap[key] = program;
      }

      return program;
    }
  }
}
