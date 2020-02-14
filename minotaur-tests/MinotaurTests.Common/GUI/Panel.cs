using System.Collections.Generic;
using Minotaur.Graphics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MinotaurTests.Common.GUI
{
  public class Panel : GuiComponent
  {
    private VertexArray _backgroundVao;
    private VertexArray _borderVao;

    public Color4 BorderColor { get; set; }
    public float BorderThickness { get; set; }
    public float Margin { get; set; }

    public Panel(Color4? borderColor = null, float borderThickness = 0)
    {
      BorderColor = borderColor ?? Color4.White;
      BorderThickness = borderThickness;
      Margin = 1f;

      VertexBuffer vbo = new VertexBuffer(new VertexFormat(new List<VertexAttribute>() {new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 3)}), 0, BufferUsageHint.StreamDraw, BeginMode.Triangles);
      _backgroundVao = new VertexArray(vbo, IndexBuffer.Create(new ushort[] { 0, 1, 2, 3, 2, 1 }));

      vbo = new VertexBuffer(new VertexFormat(new List<VertexAttribute>() { new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 3) }), 0, BufferUsageHint.StreamDraw, BeginMode.Lines);
      _borderVao = new VertexArray(vbo, IndexBuffer.Create(new ushort[] { 0, 1, 1, 2, 2, 3, 3, 0 }));
    }

    public override void ComputeAbsoluteSizePosition()
    {
      base.ComputeAbsoluteSizePosition();

      if (BorderThickness == 0f)
        return;

      float maxWidth = _size.X;
      float maxHeight = _size.Y;

      foreach (IGuiComponent child in Children)
      {
        child.PositionReferenceX = Reference.Absolute;
        child.PositionReferenceY = Reference.Absolute;
        child.Position = new Vector2(BorderThickness + Margin, BorderThickness + Margin);

        child.ComputeAbsoluteSizePosition();

        if (child.AbsoluteSize.X > maxWidth)
          maxWidth = child.AbsoluteSize.X;
        if (child.AbsoluteSize.Y > maxHeight)
          maxHeight = child.AbsoluteSize.Y;
      }

      _absSize = new Vector2(maxWidth + (BorderThickness + Margin) * 2, maxHeight + (BorderThickness + Margin) * 2);

      //foreach (IGuiComponent child in Children)
      //{
      //  child.ComputeAbsoluteSizePosition();
      //}
    }

    public override void Render(GraphicsDevice graphicsDevice)
    {
      float[] vertices = new float[] {
        _absPosition.X + BorderThickness, _absPosition.Y + BorderThickness, 0,
        _absPosition.X + BorderThickness, _absPosition.Y + _absSize.Y - BorderThickness, 0,
        _absPosition.X + _absSize.X - BorderThickness, _absPosition.Y + BorderThickness, 0,
        _absPosition.X + _absSize.X - BorderThickness, _absPosition.Y + _absSize.Y - BorderThickness, 0,
      };
      _backgroundVao.VertexBuffer.SetData(vertices, true);
      Shaders.PlainShader.Bind();
      Shaders.PlainShader.BindUniforms(new Dictionary<string, IUniformValue>() {
        {"Diffuse", UniformValue.Create(Color)}
      });
      graphicsDevice.World = Matrix4.Identity;
      graphicsDevice.DrawVertexArray(_backgroundVao, Shaders.PlainShader);
      if (BorderThickness > 0f)
      {
        float halfBorder = BorderThickness * 0.5f;
        vertices = new float[] {
          _absPosition.X + halfBorder, _absPosition.Y + halfBorder, 0,
          _absPosition.X + halfBorder, _absPosition.Y + _absSize.Y - halfBorder, 0,
          _absPosition.X + _absSize.X - halfBorder, _absPosition.Y + _absSize.Y - halfBorder, 0,
          _absPosition.X + _absSize.X - halfBorder, _absPosition.Y + halfBorder, 0,
        };
        _borderVao.VertexBuffer.SetData(vertices, true);

        float oldLineWidth;
        GL.GetFloat(GetPName.LineWidth, out oldLineWidth);
        GL.LineWidth(BorderThickness);
        Shaders.PlainShader.Bind();
        Shaders.PlainShader.BindUniforms(new Dictionary<string, IUniformValue>() {
          {"Diffuse", UniformValue.Create(BorderColor)}
        });
        graphicsDevice.World = Matrix4.Identity;
        graphicsDevice.DrawVertexArray(_borderVao, Shaders.PlainShader);
        GL.LineWidth(oldLineWidth);
      }

      base.Render(graphicsDevice);
    }
  }
}
