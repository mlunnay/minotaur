using System.Collections.Generic;
using System.ComponentModel;
using Assimp;
using Minotaur.Pipeline.Graphics;

namespace Minotaur.Pipeline.Processors
{
  [ContentProcessor(DisplayName = "Bone Animation Processor")]
  public class BoneAnimProcessor : ContentProcessor<SceneContent, BoneAnimationsContent>
  {
    private List<BoneContent> _bones = new List<BoneContent>();
    private Dictionary<string, BoneContent> _namesToBones = new Dictionary<string, BoneContent>();

    public string AnimationName { get; set; }

    [Description("String to prepend to the animation name for output.")]
    public string NamePre { get; set; }

    [Description("String to append to the animation name for output.")]
    public string NamePost { get; set; }

    [DefaultValue(Minotaur.Graphics.Animation.KeyframeEnding.Clamp)]
    public Minotaur.Graphics.Animation.KeyframeEnding PreferedEnding { get; set; }

    public BoneAnimProcessor()
    {
      PreferedEnding = Minotaur.Graphics.Animation.KeyframeEnding.Clamp;
    }

    public override BoneAnimationsContent Process(SceneContent input, ContentProcessorContext context)
    {
      BoneAnimationsContent clip = new BoneAnimationsContent();
      BuildSkeleton(input.Scene.RootNode, null);
      for (int i = 0; i < _bones.Count; i++)
      {
        clip.TranslationKeys.Add(new List<Minotaur.Graphics.Animation.VectorKey>());
        clip.QuaternionKeys.Add(new List<Minotaur.Graphics.Animation.QuaternionKey>());
        clip.ScaleKeys.Add(new List<Minotaur.Graphics.Animation.VectorKey>());
      }
      foreach (Animation animation in input.Scene.Animations)
      {
        if (string.IsNullOrEmpty(AnimationName) || animation.Name.Equals(AnimationName))
        {
          clip.Name = animation.Name;
          clip.preferedEnding = PreferedEnding;
          clip.FramesPerSecond = animation.TicksPerSecond != 0 ? (float)animation.TicksPerSecond : 25.0f;
          clip.TotalFrames = (ushort)animation.DurationInTicks;
          float tickConversion = 1.0f / clip.FramesPerSecond;

          foreach (NodeAnimationChannel channels in animation.NodeAnimationChannels)
          {
            BoneContent bone;
            if (!_namesToBones.TryGetValue(channels.NodeName, out bone))
            {
              continue; // no bone by that name so skip it.
            }
            int boneIndex = (int)bone.Index;
            foreach (VectorKey t in channels.PositionKeys)
              clip.TranslationKeys[boneIndex].Add(new Minotaur.Graphics.Animation.VectorKey((float)t.Time * tickConversion, ConvertVector(t.Value)));
            foreach (QuaternionKey q in channels.RotationKeys)
              clip.QuaternionKeys[boneIndex].Add(new Minotaur.Graphics.Animation.QuaternionKey((float)q.Time * tickConversion, ConvertQuaternion(q.Value)));
            foreach (VectorKey s in channels.ScalingKeys)
              clip.ScaleKeys[boneIndex].Add(new Minotaur.Graphics.Animation.VectorKey((float)s.Time * tickConversion, ConvertVector(s.Value)));
          }

          clip.OpaqueData["OutputFileName"] = string.Format("{0}{1}{2}", NamePre, animation.Name, NamePost);
          return clip;
        }
      }

      return null;
    }

    private BoneContent BuildSkeleton(Node node, BoneContent parent)
    {
      BoneContent bone = new BoneContent(node.Name, (uint)_bones.Count, ConvertMatrix(node.Transform), parent);
      _bones.Add(bone);
      _namesToBones[bone.Name] = bone;

      bone.InverseAbsoluteTransform = OpenTK.Matrix4.Invert(bone.Transform);
      BoneContent p = bone.Parent;
      while (p != null)
      {
        bone.InverseAbsoluteTransform *= p.Transform;
        p = p.Parent;
      }

      if (node.HasChildren)
      {
        foreach (Node n in node.Children)
        {
          BoneContent child = BuildSkeleton(n, bone);
          bone.Children.Add(child);
        }
      }

      return bone;
    }

    private OpenTK.Vector3 ConvertVector(Vector3D v)
    {
      return new OpenTK.Vector3(v.X, v.Y, v.Z);
    }

    private OpenTK.Quaternion ConvertQuaternion(Quaternion q)
    {
      return new OpenTK.Quaternion(q.X, q.Y, q.Z, q.W);
    }

    private OpenTK.Matrix4 ConvertMatrix(Matrix4x4 m)
    {
      return new OpenTK.Matrix4(m.A1, m.A2, m.A3, m.A4, m.B1, m.B2, m.B3, m.B4, m.C1, m.C2, m.C3, m.C4, m.D1, m.D2, m.D3, m.D4);
    }
  }
}
