using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Minotaur.Core;

namespace Minotaur.Graphics
{
  public class ModelMesh : GraphicsResource
  {
    private List<ModelMeshPart> _parts = new List<ModelMeshPart>();

    public ReadOnlyCollection<ModelMeshPart> MeshParts { get; set; }

    public String Name { get; set; }

    public BoundingSphere BoundingSphere { get; set; }

    public Bone ParentBone { get; set; }

    public object Tag { get; set; }

    public ModelMesh(List<ModelMeshPart> parts)
    {
      _parts = parts;
      foreach (ModelMeshPart part in parts)
      {
        part.Parent = this;
      }
      MeshParts = new ReadOnlyCollection<ModelMeshPart>(_parts);
    }
  }
}
