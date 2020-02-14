using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.ObjectModel;

namespace Minotaur.Graphics
{
  public class VertexFormat
  {
    private List<VertexAttribute> _attributes = new List<VertexAttribute>();

    #region Static VertexFormats

    public static VertexFormat PositionColor;
    public static VertexFormat PositionColorTexture;
    public static VertexFormat PositionNormalTexture;
    public static VertexFormat PositionTexture;

    static VertexFormat()
    {
      PositionColor = new VertexFormat(new List<VertexAttribute>()
      {
        new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3),
        new VertexAttribute(VertexUsage.Color, VertexAttribPointerType.Float, 0, 4)
      });

      PositionColorTexture = new VertexFormat(new List<VertexAttribute>()
      {
        new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3),
        new VertexAttribute(VertexUsage.Color, VertexAttribPointerType.Float, 0, 4),
        new VertexAttribute(VertexUsage.TextureCoordinate, VertexAttribPointerType.Float, 0, 2, true)
      });

      PositionNormalTexture = new VertexFormat(new List<VertexAttribute>()
      {
        new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3),
        new VertexAttribute(VertexUsage.Normal, VertexAttribPointerType.Float, 0, 3),
        new VertexAttribute(VertexUsage.TextureCoordinate, VertexAttribPointerType.Float, 0, 2, true)
      });

      PositionTexture = new VertexFormat(new List<VertexAttribute>()
      {
        new VertexAttribute(VertexUsage.Position, VertexAttribPointerType.Float, 0, 3),
        new VertexAttribute(VertexUsage.TextureCoordinate, VertexAttribPointerType.Float, 0, 2, true)
      });
    }

    #endregion

    public int Stride { get; private set; }

    public ReadOnlyCollection<VertexAttribute> Attributes { get { return new ReadOnlyCollection<VertexAttribute>(_attributes); } }

    public VertexFormat() { }

    public VertexFormat(List<VertexAttribute> attributes)
    {
      foreach (VertexAttribute attribute in attributes)
      {
        Add(attribute);
      }
    }

    public void Clear()
    {
      _attributes.Clear();
      Stride = 0;
    }

    public VertexAttribute Add(VertexAttribute attribute)
    {
      attribute.Offset = Stride;
      Stride += attribute.Stride();
      _attributes.Add(attribute);
      return attribute;
    }

    public VertexAttribute Add(VertexUsage usage,
      VertexAttribPointerType type,
      int index,
      int dimension,
      int offset,
      bool normalized)
    {
      VertexAttribute attribute = new VertexAttribute() { Usage = usage, Type = type, Index = index, Dimension = dimension, Offset = offset, Normalized = normalized };
      Stride += attribute.Stride();
      _attributes.Add(attribute);
      return attribute;
    }

    public bool HasAttribute(VertexUsage usage, int index)
    {
      return _attributes.Any(a => a.Usage == usage && a.Index == index);
    }

    public VertexAttribute GetAttribute(VertexUsage usage, int index=0)
    {
      VertexAttribute attribute = _attributes.FirstOrDefault(a => a.Usage == usage && a.Index == index);
      if (attribute == null)
        throw new InvalidOperationException("VertexAttrubute not found");
      return attribute;
    }
  }
}
