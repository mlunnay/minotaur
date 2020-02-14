using System;
using System.Collections.Generic;
using Minotaur.Graphics;
using OpenTK.Graphics;
using OpenTK;
using OpenTK.Input;

namespace MinotaurTests.Common.GUI
{
  public interface IGuiComponent : IDisposable
  {
    bool Enabled { get; set; }
    bool Visible { get; set; }
    Color4 Color { get; set; }
    IGuiComponent Parent { get; set; }
    IGuiComponent FocusChild { get; set; }
    List<IGuiComponent> Children { get; }

    Vector2 Position { get; set; }
    Vector2 AbsolutePosition { get; set; }
    Vector2 Size { get; set; }
    Vector2 AbsoluteSize { get; set; }

    Reference SizeReferenceX { get; set; }
    Reference SizeReferenceY { get; set; }
    Reference PositionReferenceX { get; set; }
    Reference PositionReferenceY { get; set; }
    Alignment HorizontalAlignment { get; set; }
    Alignment VerticalAlignment { get; set; }

    void AddChild(IGuiComponent child);
    void Render(GraphicsDevice graphicsDevice);
    void Focus();
    void Unfocus(IGuiComponent newFocus = null);
    bool IsInside(Vector2 point);

    void ComputeAbsoluteSizePosition();

    bool MouseDown(Vector2 position, MouseButton button);
    bool MouseUp(Vector2 position, MouseButton button);
    bool MouseMove(Vector2 position);

    void Update(float dt);
  }
}
