using System;
using System.Linq;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Pipeline.Graphics
{
  public class VertexBufferContent : ContentItem
  {
    internal List<VertexAttribute> Attributes = new List<VertexAttribute>();

    public int Count { get { return Attributes.Count; } }

    public int Stride { get { return Attributes.Sum(a => a.Stride); } }

    public VertexBufferContent() { }

    public VertexBufferContent(IEnumerable<VertexAttribute> attributes)
    {
      foreach (VertexAttribute attribute in attributes)
      {
        Attributes.Add(attribute);
      }
    }

    public void AddAttribute(VertexAttribute attribute)
    {
      Attributes.Add(attribute);
    }

    public VertexAttribute this[Minotaur.Graphics.VertexUsage usage]
    {
      get
      {
        VertexAttribute attribute = Attributes.FirstOrDefault(a => a.Usage == usage);
        if (attribute == null)
          throw new KeyNotFoundException(string.Format("Vertex for VertexUsage {0} not found.", usage));
        return attribute;
      }
    }

    public byte[] GetBytes()
    {
      List<byte> bytes = new List<byte>();
      for (int i = 0; i < ((PositionAttribute)this[Minotaur.Graphics.VertexUsage.Position]).Values.Count; i++)
      {
        foreach (VertexAttribute attribute in Attributes)
        {
          bytes.AddRange(attribute.GetBytes(i));
        }
      }
      return bytes.ToArray();
    }
  }
}
