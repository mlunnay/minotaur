using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Minotaur.Pipeline.Graphics;
using OpenTK.Graphics;

namespace Minotaur.Pipeline.Writers
{
  public class MaterialWriter : ContentTypeWriter<MaterialContent>
  {
    public MaterialWriter()
      : base(new Guid("6f1be25e-7f37-4faa-b551-7a58c8d91824")) { }

    public override void Initialize(ContentTypeWriterManager manager)
    {
      manager.RegisterTypeWriter<ShaderSourceContent>(new ShaderSourceWriter());
      manager.RegisterTypeWriter<UniformValueContent>(new UniformValueWriter());
      manager.RegisterTypeWriter<Color4>(new ColorWriter());
    }

    public override void Write(ContentWriter writer, MaterialContent value)
    {
      writer.Write(value.Name);
      writer.Write(value.Passes.Count);
      foreach (var pass in value.Passes)
      {
        writer.Write(pass.Name);
        writer.Write(pass.FragmentShaderSources.Count + pass.VertexShaderSources.Count);
        foreach (var source in (new[] {pass.VertexShaderSources, pass.FragmentShaderSources}).SelectMany(x => x))
        {
          writer.WriteRawObject(source);
        }
        writer.Write(pass.State.CullMode);
        writer.Write(pass.State.FillMode);
        writer.Write(pass.State.DepthBias);
        writer.Write(pass.State.SlopeScaleDepthBias);
        writer.Write(pass.State.ScissorTestEnable);
        writer.Write(pass.State.DepthEnabled);
        writer.Write(pass.State.DepthWrite);
        writer.Write(pass.State.DepthFunction);
        writer.Write(pass.State.DepthOffsetFactor);
        writer.Write(pass.State.DepthOffsetUnits);
        writer.WriteRawObject(pass.State.BlendColor);
        writer.Write(pass.State.ColorBlendFunction);
        writer.Write(pass.State.AlphaBlendFunction);
        writer.Write(pass.State.ColorSourceBlend);
        writer.Write(pass.State.ColorDestinationBlend);
        writer.Write(pass.State.AlphaSourceBlend);
        writer.Write(pass.State.AlphaDestinationBlend);
        writer.Write(pass.State.StencilEnable);
        writer.Write(pass.State.StencilFunction);
        writer.Write(pass.State.StencilReference);
        writer.Write(pass.State.StencilPass);
        writer.Write(pass.State.StencilFail);
        writer.Write(pass.State.StencilZFail);
        writer.Write(pass.State.StencilWriteMask);
        writer.Write(pass.State.StencilReadMask);
        writer.WriteRawObject(pass.Parameters);
        writer.WriteRawObject(pass.ComparisonParameters);
      }
    }
  }
}
