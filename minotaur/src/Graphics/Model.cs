using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Minotaur.Graphics.Animation;

namespace Minotaur.Graphics
{
  public class Model : GraphicsResource
  {
    private List<ModelMesh> _meshes;
    
    public Skeleton Skeleton { get; private set; }
    
    public ReadOnlyCollection<ModelMesh> Meshes { get; private set; }

    public Bone RootBont { get; set; }

    public List<BoneAnimationClip> Animations { get; set; }

    public object Tag { get; set; }

    public Model() 
    {
      Animations = new List<BoneAnimationClip>();
    }

    public Model(List<ModelMesh> meshes)
      : this(null, meshes) { }

    public Model(Skeleton skeleton, List<ModelMesh> meshes)
    {
      Skeleton = skeleton;
      _meshes = meshes;
      Meshes = new ReadOnlyCollection<ModelMesh>(meshes);
    }
  }
}
