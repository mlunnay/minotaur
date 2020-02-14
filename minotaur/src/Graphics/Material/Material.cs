using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Collections.ObjectModel;

namespace Minotaur.Graphics
{
  public class Material : GraphicsResource, ICloneable
  {
    public string Name { get; private set; }

    public PassCollection Passes { get; private set; }

    private Material() { }

    public Material(string name)
    {
      Name = name;
      Passes = new PassCollection();
    }

    public Material(Material mat)
    {
      Name = mat.Name;
      Passes = new PassCollection(mat.Passes);
    }

    public int PassesHash()
    {
      int _hashCode = 32;
      foreach (Pass pass in Passes)
      {
        _hashCode = _hashCode * 37 + pass.ProgramHash;
      }
      return _hashCode;
    }

    #region ICloneable Members

    public object Clone()
    {
      Material mat = new Material();
      mat.Name = Name;
      mat.Passes = (PassCollection)Passes.Clone();
      return mat;
    }

    #endregion
  }
}
