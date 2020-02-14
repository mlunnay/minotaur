using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Minotaur.Graphics
{
  public class DrawQuadCommand : IPipelineCommand
  {
    static VertexArray _fsQuad;

    #region IPipelineCommand Members

    public void Execute(GraphicsDevice device, RenderPipelineStage stage, Dictionary<string, object> parameters)
    {
      // TODO: implement
      throw new NotImplementedException();

      // set projection to orthographic
      Dictionary<string, IUniformValue> uniforms = stage.GetShaderUniforms(parameters["material"] as string);
      uniforms["projection"] = UniformValue.Create(Matrix4.CreateOrthographic(1f, 1f, -1f, 1f));
      uniforms["modelView"] = UniformValue.Create(Matrix4.CreateTranslation(0, 0, 0));

      // draw a full screen quad.
      if (_fsQuad == null)
      {
        // create the quad VertexArray.
        VertexBuffer vbo = VertexBuffer.Create(VertexFormat.PositionTexture, new VertexPositionTexture[]{
           new VertexPositionTexture(new Vector3(0f, 0f,  0f), new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( 1.0f, 0f,  0f), new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(new Vector3( 1.0f,  0f,  0f), new Vector2(1.0f, 0.0f)),
            new VertexPositionTexture(new Vector3(0f,  1.0f,  0f), new Vector2(0.0f, 0.0f))
        });
        IndexBuffer ibo = IndexBuffer.Create(new byte[] { 0, 1, 2, 2, 3, 0 });
        _fsQuad = new VertexArray(vbo, ibo);
      }

      // get material from pipeline cache

      // set materials uniforms

      // draw full screen quad with material
    }

    #endregion
  }
}
