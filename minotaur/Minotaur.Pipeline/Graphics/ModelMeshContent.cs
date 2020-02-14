using System;
using System.Collections.Generic;
using Minotaur.Core;

namespace Minotaur.Pipeline.Graphics
{
  public class ModelMeshContent
  {
    public List<ModelMeshPartContent> MeshParts { get; private set; }
    public string Name { get; set; }
    public BoundingSphere BoundingSphere { get; set; }
    public BoneContent ParentBone { get; set; }
    public object Tag { get; set; }

    public ModelMeshContent(string name, BoneContent parentBone, IEnumerable<ModelMeshPartContent> meshParts, BoundingSphere boundingSphere)
    {
      Name = name;
      ParentBone = parentBone;
      MeshParts = new List<ModelMeshPartContent>(meshParts);
      BoundingSphere = boundingSphere;
    }
  }
}
