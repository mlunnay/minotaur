
namespace Minotaur.Graphics
{
  public class ModelMeshPart : GraphicsResource
  {
    public Material Material { get; set; }

    public IndexBuffer IndexBuffer { get { return VertexArray.IndexBuffer; } }

    public uint NumVertices { get; set; }

    public uint NumIndicies { get; set; }

    public uint PrimitiveCount { get; set; }

    public object Tag { get; set; }

    public VertexBuffer VertexBuffer { get { return VertexArray.VertexBuffer; } }

    internal ModelMesh Parent { get; set; }

    public VertexArray VertexArray { get; set; }

    public uint StartIndex { get; set; }

    public uint StartVertex { get; set; }

    public ModelMeshPart Clone()
    {
      ModelMeshPart part = new ModelMeshPart();
      part.Material = Material;
      part.NumVertices = NumVertices;
      part.PrimitiveCount = PrimitiveCount;
      part.Tag = Tag;
      part.VertexArray = VertexArray;
      part.Parent = Parent;
      part.StartIndex = StartIndex;
      part.StartVertex = StartVertex;
      return part;
    }
  }
}
