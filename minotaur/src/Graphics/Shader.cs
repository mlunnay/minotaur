using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using Minotaur.Core;

namespace Minotaur.Graphics
{
  public class Shader : GraphicsResource
  {
    #region Declarations

    internal int _id;
    private ShaderType _shaderType;

    #endregion

    #region Properties

    public int ID { get { return _id; } }
    public ShaderType ShaderType { get { return _shaderType; } }

    #endregion

    #region Constructor

    public Shader(string code, ShaderType type)
    {
      _shaderType = type;
      _id = GL.CreateShader(type);
      GL.ShaderSource(_id, code);
      GL.CompileShader(_id);
      int status;
      GL.GetShader(_id, ShaderParameter.CompileStatus, out status);
      if (status == 0)
      {
        string log = GL.GetShaderInfoLog(_id);
        GL.DeleteShader(_id);
        _id = 0;
        throw new CompileFailedException(string.Format("Compile failure in shader:\n{0}", log));
      }
    }

    internal Shader(ShaderType type)
    {
      _shaderType = type;
      _id = GL.CreateShader(type);
    }

    #endregion

    #region Destructor

    ~Shader()
    {
      Dispose(false);
    }

    #endregion

    #region Public Methods

    public override bool Equals(object obj)
    {
      Shader o = obj as Shader;
      if (o == null)
        return false;
      return _id == o._id;
    }

    public bool Equals(Shader other)
    {
      if (other == null)
        return false;
      return _id == other._id;
    }

    public override int GetHashCode()
    {
      return _id;
    }

    #endregion

    #region Private Methods

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed && _id != 0)
      {
        // skipping if disposing as no managed resources to dispose
        DisposalManager.Add(() => GL.DeleteShader(_id));
        _id = 0;
        IsDisposed = true;
      }
    }

    #endregion
  }
}
