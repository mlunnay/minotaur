using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Minotaur.Pipeline.Graphics
{
  public class BoneContent : ContentItem
  {
    public uint Index { get; private set; }
    public Matrix4 Transform { get; private set; }
    public Matrix4 InverseAbsoluteTransform { get; internal set; }
    public BoneContent Parent { get; private set; }
    public List<BoneContent> Children { get; private set; }

    public BoneContent(string name, uint index, Matrix4 transform, BoneContent parent)
    {
      Name = name;
      Index = index;
      Transform = transform;
      Parent = parent;
      Children = new List<BoneContent>();
    }
  }
}
