using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Pipeline.Graphics
{
  public class ModelMeshPartContent : ContentItem
  {
    public MaterialContent Material { get; set; }

    public IndexCollection IndexBuffer { get; set; }

    public uint NumVertices { get; set; }

    public uint NumIndicies { get; set; }

    public uint PrimitiveCount { get; set; }

    public object Tag { get; set; }

    public VertexBufferContent VertexBuffer { get; set; }

    internal ModelMeshContent Parent { get; set; }

    public uint StartIndex { get; set; }

    public uint StartVertex { get; set; }

    public ModelMeshPartContent(VertexBufferContent vb, IndexCollection ic, uint startVertex, uint numVerticies, uint startIndex, uint numIndicies, uint primitiveCount)
    {
      VertexBuffer = vb;
      IndexBuffer = ic;
      StartVertex = startVertex;
      NumVertices = numVerticies;
      StartIndex = startIndex;
      NumIndicies = numVerticies;
      PrimitiveCount = primitiveCount;
    }
  }
}
