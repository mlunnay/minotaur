using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Minotaur.Core;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class Program : GraphicsResource
  {
    #region Declarations

    private int _id;
    private List<ProgramAttribute> _attributes = new List<ProgramAttribute>();
    private List<ProgramUniform> _uniforms = new List<ProgramUniform>();
    private int _hashCode;

    // the below are needed to remove an autocast to Vector4 problem
    private static MethodInfo VertexAttrib4UInt32Array;
    private static MethodInfo VertexAttrib4UInt16Array;
    private static MethodInfo VertexAttrib4SByteArray;

    public UniformMappings UniformMappings;

    #endregion

    #region Properties

    public int ID { get { return _id; } }

    public bool InUse
    {
      get
      {
        int currentProgam = 0;
        GL.GetInteger(GetPName.CurrentProgram, out currentProgam);
        return currentProgam == _id;
      }
    }

    public List<ProgramAttribute> Attributes
    {
      get { return new List<ProgramAttribute>(_attributes); }
    }

    public List<ProgramUniform> Uniforms
    {
      get { return new List<ProgramUniform>(_uniforms); }
    }

    /// <summary>
    /// This returns a hash based on the shaders of this program.
    /// This should allow matching of programs with the same shaders.
    /// </summary>
    public int ShaderHash
    {
      get { return _hashCode; }
    }

    #endregion

    #region Constructor

    public Program(IEnumerable<Shader> shaders, IEnumerable<KeyValuePair<int, string>> fragDataLocations = null)
    {
      _id = GL.CreateProgram();
      _hashCode = 32;
      shaders = shaders.ToList();
      ((List<Shader>)shaders).Sort((a, b) => a._id.CompareTo(b._id));
      foreach (Shader shader in shaders)
      {
        GL.AttachShader(_id, shader.ID);
        _hashCode = _hashCode * 37 + shader.GetHashCode();
      }
      if (fragDataLocations != null)
      {
        foreach (var item in fragDataLocations)
          GL.BindFragDataLocation(_id, item.Key, item.Value);
      }
      GL.LinkProgram(_id);
      int status;
      GL.GetProgram(_id, ProgramParameter.LinkStatus, out status);
      if (status == 0)
      {
        string log = GL.GetProgramInfoLog(_id);
        GL.DeleteProgram(_id);
        _id = 0;
        throw new LinkFailedException(string.Format("Error linking program:\n{0}", log));
      }
      MapAttributes();
      MapUniforms();
      
    }

    /// <summary>
    /// This constructor creates a new program from code strings for a vertex shader, and a fragment shader
    /// </summary>
    /// <param name="vertexCode"></param>
    /// <param name="fragmentCode"></param>
    public Program(string vertexCode, string fragmentCode, IEnumerable<KeyValuePair<int, string>> fragDataLocations = null)
      : this(new Shader[] { new Shader(vertexCode, ShaderType.VertexShader), new Shader(fragmentCode, ShaderType.FragmentShader) }, fragDataLocations)
    { }

    internal Program()
    {
      _id = GL.CreateProgram();
    }

    static Program()
    {
      // this is solving an error with auto casting to Vector4
      VertexAttrib4UInt32Array = typeof(GL).GetMethod("VertexAttrib4", new Type[] { typeof(UInt32), typeof(UInt32[]) });
      VertexAttrib4UInt16Array = typeof(GL).GetMethod("VertexAttrib4", new Type[] { typeof(UInt32), typeof(UInt16[]) });
      VertexAttrib4SByteArray = typeof(GL).GetMethod("VertexAttrib4", new Type[] { typeof(UInt32), typeof(SByte[]) });
    }

    #endregion

    #region Destructor

    ~Program()
    {
      Dispose(false);
    }

    #endregion

    #region Public Methods

    public void Bind()
    {
      GL.UseProgram(_id);
    }

    public void Unbind()
    {
      GL.UseProgram(0);
    }

    public void BindUniforms(Dictionary<string, IUniformValue> values)
    {
      foreach (ProgramUniform uniform in _uniforms)
      {
        IUniformValue value;
        if (values.TryGetValue(uniform.Name, out value))
        {
          if (uniform.Cache == null)
          {
            uniform.Cache = value.Default();
          }
          
          if(!value.Equals(uniform.Cache))
          {
            value.Apply(uniform.Slot);
            uniform.Cache.Set(value);
          }
        }
      }
    }

    public void BindUniformSource(IUniformSource source)
    {
      foreach (ProgramUniform uniform in _uniforms)
      {
        UniformUsage usage;
        if ((usage = UniformMappings.GetUsage(uniform.Name)) != UniformUsage.GenericUniform)
        {
          IUniformValue value;
          if (source.GetUniformValue(usage, out value))
          {
            if (uniform.Cache == null)
              uniform.Cache = value.Default();

            if (!value.Equals(uniform.Cache))
            {
              value.Apply(uniform.Slot);
              uniform.Cache.Set(value);
            }
          }
        }
      }
    }

    public ProgramAttribute VertexAttribute(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      ProgramAttribute attrib = _attributes.FirstOrDefault(a => a.Name == name);
      if (attrib == null)
      {
        throw new ArgumentException(string.Format("Attribute not found: {0}", name), "name");
      }

      return attrib;
    }

    public int VertexAttributeSlot(string name)
    {
      return VertexAttribute(name).Slot;
    }

    public ProgramUniform Uniform(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      ProgramUniform uni = _uniforms.FirstOrDefault(a => a.Name == name);
      if (uni == null)
      {
        throw new ArgumentException(string.Format("Uniform not found: {0}", name), "name");
      }

      return uni;
    }

    public int UniformSlot(string name)
    {
      return Uniform(name).Slot;
    }

    // methods to set vertex attributes by name

    public void VertexAttrib1(string name, Double x)
    {
      GL.VertexAttrib1(VertexAttributeSlot(name), x);
    }

    public void VertexAttrib1(string name, Single x)
    {
      GL.VertexAttrib1(VertexAttributeSlot(name), x);
    }

    public void VertexAttrib1(string name, Int16 x)
    {
      GL.VertexAttrib1(VertexAttributeSlot(name), x);
    }

    public void VertexAttrib2(string name, Int16 x, Int16 y)
    {
      GL.VertexAttrib2(VertexAttributeSlot(name), x, y);
    }

    public void VertexAttrib2(string name, Int16[] v)
    {
      GL.VertexAttrib2(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib2(string name, Double x, Double y)
    {
      GL.VertexAttrib2(VertexAttributeSlot(name), x, y);
    }

    public void VertexAttrib2(string name, Double[] v)
    {
      GL.VertexAttrib2(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib2(string name, Single x, Single y)
    {
      GL.VertexAttrib2(VertexAttributeSlot(name), x, y);
    }

    public void VertexAttrib2(string name, Single[] v)
    {
      GL.VertexAttrib2(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib3(string name, Double x, Double y, Double z)
    {
      GL.VertexAttrib3(VertexAttributeSlot(name), x, y, z);
    }

    public void VertexAttrib3(string name, Double[] v)
    {
      GL.VertexAttrib3(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib3(string name, Single x, Single y, Single z)
    {
      GL.VertexAttrib3(VertexAttributeSlot(name), x, y, z);
    }

    public void VertexAttrib3(string name, Single[] v)
    {
      GL.VertexAttrib3(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib3(string name, Int16 x, Int16 y, Int16 z)
    {
      GL.VertexAttrib3(VertexAttributeSlot(name), x, y, z);
    }

    public void VertexAttrib3(string name, Int16[] v)
    {
      GL.VertexAttrib3(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib4(string name, Int16 x, Int16 y, Int16 z, Int16 w)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), x, y, z, w);
    }

    public void VertexAttrib4(string name, Int16[] v)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib4(string name, Byte[] v)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib4(string name, UInt32[] v)
    {
      //GL.VertexAttrib4(VertexAttribute(name), v as UInt32[]);
      // this is solving an error with auto casting to Vector4
      VertexAttrib4UInt32Array.Invoke(null, new object[] { v });
    }

    public void VertexAttrib4(string name, UInt16[] v)
    {
      //GL.VertexAttrib4(VertexAttribute(name), v);
      // this is solving an error with auto casting to Vector4
      VertexAttrib4UInt16Array.Invoke(null, new object[] { v });
    }

    public void VertexAttrib4(string name, SByte[] v)
    {
      //GL.VertexAttrib4(VertexAttribute(name), v);
      // this is solving an error with auto casting to Vector4
      VertexAttrib4SByteArray.Invoke(null, new object[] { v });
    }

    public void VertexAttrib4(string name, Double x, Double y, Double z, Double w)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), x, y, z, w);
    }

    public void VertexAttrib4(string name, Double[] v)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib4(string name, Single x, Single y, Single z, Single w)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), x, y, z, w);
    }

    public void VertexAttrib4(string name, Single[] v)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), v);
    }

    public void VertexAttrib4(string name, Int32[] v)
    {
      GL.VertexAttrib4(VertexAttributeSlot(name), v);
    }

    public void UniformMatrix4(string name, bool transpose, ref Matrix4 matrix)
    {
      GL.UniformMatrix4(UniformSlot(name), transpose, ref matrix);
    }

    public void UniformMatrix4x2(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix4x2(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix4x3(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix4x3(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix2(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix2(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix2x3(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix2x3(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix2x4(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix2x4(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix3(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix3(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix3x2(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix3x2(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix3x4(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix3x4(UniformSlot(name), count, transpose, value);
    }

    public void UniformMatrix4(string name, Int32 count, bool transpose, Single[] value)
    {
      GL.UniformMatrix4(UniformSlot(name), count, transpose, value);
    }

    public void VertexAttribPointer(string name, int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
    {
      GL.VertexAttribPointer(VertexAttributeSlot(name), size, type, normalized, stride, offset);
    }

    // methods to set uniforms by name

    public void Uniform1(string name, Single v0)
    {
      GL.Uniform1(UniformSlot(name), v0);
    }

    public void Uniform1(string name, Int32 count, Single[] value)
    {
      GL.Uniform1(UniformSlot(name), count, value);
    }

    public void Uniform1(string name, Int32 v0)
    {
      GL.Uniform1(UniformSlot(name), v0);
    }

    public void Uniform1(string name, Int32 count, Int32[] value)
    {
      GL.Uniform1(UniformSlot(name), count, value);
    }

    public void Uniform1(string name, UInt32 v0)
    {
      GL.Uniform1(UniformSlot(name), v0);
    }

    public void Uniform1(string name, Int32 count, UInt32[] value)
    {
      GL.Uniform1(UniformSlot(name), count, value);
    }

    public void Uniform2(string name, Single v0, Single v1)
    {
      GL.Uniform2(UniformSlot(name), v0, v1);
    }

    public void Uniform2(string name, Int32 count, Single[] value)
    {
      GL.Uniform2(UniformSlot(name), count, value);
    }

    public void Uniform2(string name, Int32 v0, Int32 v1)
    {
      GL.Uniform2(UniformSlot(name), v0, v1);
    }

    public void Uniform2(string name, Int32 count, Int32[] value)
    {
      GL.Uniform2(UniformSlot(name), count, value);
    }

    public void Uniform2(string name, UInt32 v0, UInt32 v1)
    {
      GL.Uniform2(UniformSlot(name), v0, v1);
    }

    public void Uniform2(string name, Int32 count, UInt32[] value)
    {
      GL.Uniform2(UniformSlot(name), count, value);
    }

    public void Uniform3(string name, Single v0, Single v1, Single v2)
    {
      GL.Uniform3(UniformSlot(name), v0, v1, v2);
    }

    public void Uniform3(string name, Int32 count, Single[] value)
    {
      GL.Uniform3(UniformSlot(name), count, value);
    }

    public void Uniform3(string name, Int32 v0, Int32 v1, Int32 v2)
    {
      GL.Uniform3(UniformSlot(name), v0, v1, v2);
    }

    public void Uniform3(string name, Int32 count, Int32[] value)
    {
      GL.Uniform3(UniformSlot(name), count, value);
    }

    public void Uniform3(string name, UInt32 v0, UInt32 v1, UInt32 v2)
    {
      GL.Uniform3(UniformSlot(name), v0, v1, v2);
    }

    public void Uniform3(string name, Int32 count, UInt32[] value)
    {
      GL.Uniform3(UniformSlot(name), count, value);
    }

    public void Uniform4(string name, Single v0, Single v1, Single v2, Single v3)
    {
      GL.Uniform4(UniformSlot(name), v0, v1, v2, v3);
    }

    public void Uniform4(string name, Int32 count, Single[] value)
    {
      GL.Uniform4(UniformSlot(name), count, value);
    }

    public void Uniform4(string name, Int32 v0, Int32 v1, Int32 v2, Int32 v3)
    {
      GL.Uniform4(UniformSlot(name), v0, v1, v2, v3);
    }

    public void Uniform4(string name, Int32 count, Int32[] value)
    {
      GL.Uniform4(UniformSlot(name), count, value);
    }

    public void Uniform4(string name, UInt32 v0, UInt32 v1, UInt32 v2, UInt32 v3)
    {
      GL.Uniform4(UniformSlot(name), v0, v1, v2, v3);
    }

    public void Uniform4(string name, Int32 count, UInt32[] value)
    {
      GL.Uniform4(UniformSlot(name), count, value);
    }

    public void Uniform2(string name, Vector2 vector)
    {
      GL.Uniform2(UniformSlot(name), vector.X, vector.Y);
    }

    public void Uniform3(string name, Vector3 vector)
    {
      GL.Uniform3(UniformSlot(name), vector.X, vector.Y, vector.Z);
    }

    public void Uniform4(string name, Vector4 vector)
    {
      GL.Uniform4(UniformSlot(name), vector.X, vector.Y, vector.Z, vector.W);
    }

    public void Uniform4(string name, Color4 color)
    {
      GL.Uniform4(UniformSlot(name), color.R, color.G, color.B, color.A);
    }

    public void Uniform4(string name, Quaternion quaternion)
    {
      GL.Uniform4(UniformSlot(name), quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
    }

    public bool HasAttribute(string name)
    {
      return _attributes.Any(a => a.Name == name);
    }

    public bool HasUniform(string name)
    {
      return _uniforms.Any(a => a.Name == name);
    }

    public override bool Equals(object obj)
    {
      Program o = obj as Program;
      if (o == null)
        return false;
      return _id == o._id;
    }

    public bool Equals(Program other)
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

    internal void MapAttributes()
    {
      int count = 0;

      GL.GetProgram(_id, ProgramParameter.ActiveAttributes, out count);

      _attributes.Clear();
      for (int i = 0; i < count; i++)
      {
        int size;
        ActiveAttribType type;

        string name = GL.GetActiveAttrib(_id, i, out size, out type);
        int slot = GL.GetAttribLocation(_id, name);

        ProgramAttribute attribute = new ProgramAttribute(name, slot, size, type);
        _attributes.Add(attribute);
      }
    }

    internal void MapUniforms()
    {
      int count = 0;

      GL.GetProgram(_id, ProgramParameter.ActiveUniforms, out count);

      _uniforms.Clear();
      for (int i = 0; i < count; i++)
      {
        int size;
        ActiveUniformType  type;

        string name = GL.GetActiveUniform(_id, i, out size, out type);
        int slot = GL.GetUniformLocation(_id, name);

        ProgramUniform attribute = new ProgramUniform(name, slot, size, type);
        _uniforms.Add(attribute);
      }
    }

    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        // skipping disposing as no managed resources to free
        DisposalManager.Add(() => GL.DeleteProgram(_id));
        _id = 0;
      }
      IsDisposed = true;
    }

    #endregion
  }
}
