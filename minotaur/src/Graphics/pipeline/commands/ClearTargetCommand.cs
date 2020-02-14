using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  class ClearTargetCommand
  {
    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      Color4 color = (Color4)parameters["color"];
      if ((bool)parameters["depth"] == true)
        GL.ClearDepth(1.0);
      DrawBuffersEnum[] buffers = ((int[])parameters["buffers"]).Select(i => DrawBuffersEnum.ColorAttachment0 + i).ToArray();
      if (buffers.Length != 0)
      {
        GL.DrawBuffers(buffers.Length, buffers);
        GL.ClearColor(color);
        GL.DrawBuffers(stage.Pipeline.CurrentRenderTarget.DrawBuffers.Length, stage.Pipeline.CurrentRenderTarget.DrawBuffers);
      }
    }
  }
}
