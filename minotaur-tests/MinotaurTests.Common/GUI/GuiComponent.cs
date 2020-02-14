using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;
using OpenTK;
using Minotaur.Graphics;
using OpenTK.Input;

namespace MinotaurTests.Common.GUI
{
  public class GuiComponent : GraphicsResource, IGuiComponent
  {
    protected Vector2 _position;
    protected Vector2 _size;
    protected Vector2 _absPosition;
    protected Vector2 _absSize;
    private Reference _sizeReferenceX;
    private Reference _sizeReferenceY;
    private Reference _positionReferenceX;
    private Reference _positionReferenceY;
    private Alignment _horizontalAlignment;
    private Alignment _verticalAlignment;

    public bool Enabled { get; set; }
    public bool Visible { get; set; }
    public Color4 Color { get; set; }
    public IGuiComponent Parent { get; set; }
    public IGuiComponent FocusChild { get; set; }

    public Reference SizeReferenceX
    {
      get { return _sizeReferenceX; }
      set
      {
        _sizeReferenceX = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Reference SizeReferenceY
    {
      get { return _sizeReferenceY; }
      set
      {
        _sizeReferenceY = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Reference PositionReferenceX
    {
      get { return _positionReferenceX; }
      set
      {
        _positionReferenceX = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Reference PositionReferenceY
    {
      get { return _positionReferenceY; }
      set
      {
        _positionReferenceY = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Alignment HorizontalAlignment
    {
      get { return _horizontalAlignment; }
      set
      {
        _horizontalAlignment = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Alignment VerticalAlignment
    {
      get { return _verticalAlignment; }
      set
      {
        _verticalAlignment = value;
        ComputeAbsoluteSizePosition();
      }
    }


    public List<IGuiComponent> Children { get; private set; }

    public Vector2 Position
    {
      get { return _position; }
      set
      {
        _position = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Vector2 AbsolutePosition
    {
      get { return _absPosition; }
      set { _absPosition = value; }
    }

    public Vector2 Size
    {
      get { return _size; }
      set
      {
        _size = value;
        ComputeAbsoluteSizePosition();
      }
    }

    public Vector2 AbsoluteSize
    {
      get { return _absSize; }
      set { _absSize = value; }
    }

    protected GuiComponent()
    {
      Children = new List<IGuiComponent>();
      Enabled = true;
      Visible = true;
      Color = Color4.White;
      _sizeReferenceX = Reference.Absolute;
      _sizeReferenceY = Reference.Absolute;
      _positionReferenceX = Reference.Relative;
      _positionReferenceY = Reference.Relative;
      _horizontalAlignment = Alignment.Left;
      _verticalAlignment = Alignment.Top;
    }

    public GuiComponent(int width, int height)
      : this()
    {
      _size = new Vector2(width, height);
    }

    public virtual void ComputeAbsoluteSizePosition()
    {
      if (Parent == null)
      {
        _absSize = _size;
        _absPosition = _position;
      }

      foreach (IGuiComponent child in Children)
      {
        Vector2 absoluteSize = child.Size;
        Vector2 absolutePosition = child.Position;

        if (child.SizeReferenceX == Reference.Relative)
          absoluteSize.X = _absSize.X * child.Size.X / 100f;
        if (child.SizeReferenceY == Reference.Relative)
          absoluteSize.Y = _absSize.Y * child.Size.Y / 100f;
        if (child.PositionReferenceX == Reference.Relative)
          absolutePosition.X = _absPosition.X * child.Position.X / 100f;
        if (child.PositionReferenceY == Reference.Relative)
          absolutePosition.Y = _absPosition.Y * child.Position.Y / 100f;

        // align the child
        if (child.HorizontalAlignment == Alignment.Left)
          absolutePosition.X = _absPosition.X + child.Position.X;
        else if (child.HorizontalAlignment == Alignment.Right)
          absolutePosition.X = _absPosition.X + _absSize.X - child.Position.X - _absPosition.X;
        else
          absolutePosition.X = _absPosition.X + _absSize.X / 2 + child.Position.X - absoluteSize.X / 2;

        if (child.VerticalAlignment == Alignment.Top)
          absolutePosition.Y = _absPosition.Y + child.Position.Y;
        else if (child.VerticalAlignment == Alignment.Bottom)
          absolutePosition.Y = _absPosition.Y + _absSize.Y - child.Position.Y - _absPosition.Y;
        else
          absolutePosition.Y = _absPosition.Y + _absSize.Y / 2 + child.Position.Y - absoluteSize.Y / 2;

        child.AbsoluteSize = absoluteSize;
        child.AbsolutePosition = absolutePosition;
        child.ComputeAbsoluteSizePosition();
      }
    }

    public void AddChild(IGuiComponent child)
    {
      if (child.Parent != null)
      {
        child.Parent.Children.Remove(child);
      }
      child.Parent = this;
      Children.Add(child);
      ComputeAbsoluteSizePosition();
    }

    public virtual void Render(GraphicsDevice graphicsDevice)
    {
      foreach (IGuiComponent child in Children)
        child.Render(graphicsDevice);
    }

    public void Focus()
    {
      if (Parent != null)
      {
        if (Parent.FocusChild != null)
          Parent.FocusChild.Unfocus(this);  // unfocus current, focus this.
        else
          Parent.FocusChild = this;

        Parent.Focus();
      }
    }

    public void Unfocus(IGuiComponent newFocus = null)
    {
      if (Parent != null)
        Parent.FocusChild = newFocus;
    }

    public bool IsInside(Vector2 point)
    {
      Vector2 topLeft = _absPosition;
      Vector2 bottomRight = topLeft + _absSize;
      return (point.X >= topLeft.X && point.X <= bottomRight.X &&
        point.Y >= topLeft.Y && point.Y <= bottomRight.Y);
    }

    public virtual bool MouseDown(Vector2 position, MouseButton button)
    {
      if (!(Visible && Enabled))
        return false;

      // pass to focus first
      if (FocusChild != null)
        if (FocusChild.MouseDown(position, button))
          return true;

      if (!IsInside(position))
        return false;

      foreach (IGuiComponent child in Children)
      {
        if (child.MouseDown(position, button))
          return true;
      }

      return false; // not handled
    }

    public virtual bool MouseUp(Vector2 position, MouseButton button)
    {
      if (!(Visible && Enabled))
        return false;

      // pass to focus first
      if (FocusChild != null)
        if (FocusChild.MouseUp(position, button))
          return true;

      if (!IsInside(position))
        return false;

      foreach (IGuiComponent child in Children)
      {
        if (child.MouseUp(position, button))
          return true;
      }

      return false; // not handled
    }

    public virtual bool MouseMove(Vector2 position)
    {
      if (!(Visible && Enabled))
        return false;

      // pass to focus first
      if (FocusChild != null)
        if (FocusChild.MouseMove(position))
          return true;

      if (!IsInside(position))
        return false;

      foreach (IGuiComponent child in Children)
      {
        if (child.MouseMove(position))
          return true;
      }

      return false; // not handled
    }

    public virtual void Update(float dt)
    {
      foreach (IGuiComponent child in Children)
      {
        child.Update(dt);
      }
    }
  }
}
