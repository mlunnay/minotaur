using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Minotaur.Pipeline.Graphics
{
  public class ModelContent : ContentItem
  {
    public List<ModelMeshContent> Meshes { get; private set; }
    public List<BoneContent> Bones { get; private set; }
    public BoneContent RootBone { get; private set; }
    public object Tag { get; set; }
    public List<ExternalReferenceContent<BoneAnimationsContent>> Animations { get; private set; }

    public ModelContent(BoneContent rootBone, IEnumerable<BoneContent> bones, IEnumerable<ModelMeshContent> meshes, IEnumerable<ExternalReferenceContent<BoneAnimationsContent>> animations)
    {
      Meshes = new List<ModelMeshContent>(meshes);
      RootBone = rootBone;
      Bones = new List<BoneContent>(bones);
      Animations = new List<ExternalReferenceContent<BoneAnimationsContent>>(animations);
    }
  }
}
