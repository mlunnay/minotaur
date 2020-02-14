using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using Minotaur.Core;

namespace Minotaur.Graphics.Primitives
{
  public class Skeleton : IPrimitive
  {
    private Minotaur.Graphics.Skeleton _skeleton;
    private Dictionary<string, Vector4> _boneHighlights;
    private List<string> _ignoreBones;

    public BeginMode BeginMode
    {
      get { return BeginMode.Lines; }
    }

    public Dictionary<string, Vector4> BoneHighlights { get { return _boneHighlights; } }

    public Skeleton(Minotaur.Graphics.Skeleton skeleton, Dictionary<string, Vector4> boneHighlights = null, List<string> ignoreBones = null)
    {
      _skeleton = skeleton;
      _boneHighlights = boneHighlights ?? new Dictionary<string, Vector4>();
      _ignoreBones = ignoreBones ?? new List<string>();
    }

    public Skeleton(Minotaur.Graphics.Skeleton skeleton, Dictionary<string, Color4> boneHighlights, List<string> ignoreBones = null)
    {
      _skeleton = skeleton;
      _boneHighlights = boneHighlights.ToDictionary(p => p.Key, k => new Vector4(k.Value.R, k.Value.G, k.Value.B, k.Value.A));
      _ignoreBones = ignoreBones ?? new List<string>();
    }

    public VertexPositionColor[] GetVertices(GraphicsDevice graphicsDevice, Matrix4 world, Color4 color, out ushort[] indices)
    {
      List<ushort> indicesList = new List<ushort>();
      List<VertexPositionColor> vertices = new List<VertexPositionColor>();

      AddBone(_skeleton.Bones[_skeleton.SkeletonRoot], indicesList, vertices, world, new Vector4(color.R, color.G, color.B, color.A), Matrix4.Identity);
      indices = indicesList.ToArray();
      return vertices.ToArray();
    }

    private void AddBone(Bone bone, List<ushort> indices, List<VertexPositionColor> vertices, Matrix4 world, Vector4 color, Matrix4 parentTransform)
    {
      Vector4 boneColor;
      if (!_boneHighlights.TryGetValue(bone.Name, out boneColor))
        boneColor = color;

      Vector3 start = Vector3.Transform(parentTransform.Translation(), world);
      Vector3 end = Vector3.Transform((_skeleton.BoneTransforms[bone.Index] * parentTransform).Translation(), world);

      if (bone.Parent != null && Vector3.Subtract(end, start).LengthSquared > 0f && !_ignoreBones.Contains(bone.Name))
      { // only add if the bone has actual length
        indices.Add((ushort)vertices.Count);
        indices.Add((ushort)(vertices.Count + 1));

        vertices.Add(new VertexPositionColor(start, boneColor));
        vertices.Add(new VertexPositionColor(end, boneColor)); 
      }

      foreach (Bone child in bone.Children)
        AddBone(child, indices, vertices, world, color, _skeleton.BoneTransforms[bone.Index] * parentTransform);
    }
  }
}
