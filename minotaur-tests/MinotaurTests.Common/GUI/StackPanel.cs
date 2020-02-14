using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace MinotaurTests.Common.GUI
{
  public class StackPanel : GuiComponent
  {
    public Dimension Direction { get; set; }

    public StackPanel(Dimension direction = Dimension.Vertical)
    {
      Direction = direction;
    }

    public override void ComputeAbsoluteSizePosition()
    {
      base.ComputeAbsoluteSizePosition();

      float sumWidth = 0;
      float sumHeight = 0;
      float maxWidth = 0;
      float maxHeight = 0;

      foreach (IGuiComponent child in Children)
      {
        if (Direction == Dimension.Vertical)
        {
          child.PositionReferenceY = Reference.Absolute;
          child.Position = new Vector2(child.Position.X, sumHeight);
        }
        else
        {
          child.PositionReferenceX = Reference.Absolute;
          child.Position = new Vector2(AbsolutePosition.X + sumWidth, child.Position.Y);
        }

        child.ComputeAbsoluteSizePosition();

        if (child.AbsoluteSize.X > maxWidth)
          maxWidth = child.AbsoluteSize.X;
        if (child.AbsoluteSize.Y > maxHeight)
          maxHeight = child.AbsoluteSize.Y;
        sumWidth += child.AbsoluteSize.X;
        sumHeight += child.AbsoluteSize.Y;
      }

      if (Direction == Dimension.Vertical)
        _absSize = new Vector2(maxWidth, sumHeight);
      else
        _absSize = new Vector2(sumWidth, maxHeight);
    }
  }
}
