using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class BindBufferCommand : IPipelineCommand
  {
    #region IPipelineCommand Members

    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      FrameBuffer fb = stage.Pipeline.GetRenderTarget(parameters["source"] as String);
      if (fb != null)
      {
        Texture2D texture;
        if ((int)(parameters["index"]) == -1)  // depth texture
          texture = fb.DepthTexture;
        else
          texture = fb.Textures[(int)(parameters["index"])];

        string sampler = parameters["sampler"] as string;

        stage.Pipeline.AddSamplerBinding(sampler, texture);
      }
    }

    #endregion
  }
}
