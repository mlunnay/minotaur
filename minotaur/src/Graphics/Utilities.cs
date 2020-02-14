using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Minotaur.Graphics
{
  public class Utilities
  {
    [System.Diagnostics.Conditional("DEBUG")]
    public static void CheckGLError()
    {
      ErrorCode error = GL.GetError();
      if (error != ErrorCode.NoError)
        throw new Core.GLException("GL.GetError() returned " + error.ToString());
    }
  }
}
