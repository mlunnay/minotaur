using Minotaur.Core;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public sealed class TextureCollection
  {
    private Texture[] _textures;
    private TextureTarget[] _targets;
    private int _dirty;

    internal TextureCollection(int maxTextures)
    {
      _textures = new Texture[maxTextures];
      _dirty = int.MaxValue;
      _targets = new TextureTarget[maxTextures];
    }

    public Texture this[int index]
    {
      get { return _textures[index]; }
      set
      {
        if (_textures[index] == value)
          return;

        _textures[index] = value;
        _dirty |= 1 << index;
      }
    }

    internal void Clear()
    {
      for (int i = 0; i < _textures.Length; i++)
      {
        _textures[i] = null;
        _targets[i] = 0;
      }
      _dirty = int.MaxValue;
    }

    internal void Dirty()
    {
      _dirty = int.MaxValue;
    }

    internal void SetTextures()
    {
      Threading.EnsureUIThread();

      if (_dirty == 0)
        return; // nothing has changed

      for (int i = 0; i < _textures.Length; i++)
      {
        int mask = 1 << i;
        if ((_dirty & mask) == 0)
          continue;

        Texture tex = _textures[i];

        GL.ActiveTexture(TextureUnit.Texture0 + i);
        Utilities.CheckGLError();

        // clear the previous binding if the tartet is different from the new one
        if (_targets[i] != 0 && (tex == null || _targets[i] != tex.Target))
        {
          GL.BindTexture(_targets[i], 0);
          Utilities.CheckGLError();
        }

        if (tex != null)
        {
          _targets[i] = tex.Target;
          GL.BindTexture(tex.Target, tex.ID);
          Utilities.CheckGLError();
        }

        _dirty &= ~mask;
        if (_dirty == 0)
          break;
      }

      _dirty = 0;
    }
  }
}
