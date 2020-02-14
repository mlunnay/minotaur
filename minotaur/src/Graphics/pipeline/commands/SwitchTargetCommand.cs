using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class SwitchTargetCommand : IPipelineCommand
  {
    #region IPipelineCommand Members

    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      FrameBuffer fb = stage.Pipeline.GetRenderTarget(parameters["id"] as String);
      if (fb != null)
      {
        device.SetViewport(0, 0, fb.Width, fb.Height);
        fb.Bind();
        stage.Pipeline.CurrentRenderTarget = fb;
      }
      else
      {
        device.ViewPort = stage.Pipeline.Camera.Viewport;
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        stage.Pipeline.CurrentRenderTarget = null;
      }
    }

    #endregion
  }
}
