using System;
using Minotaur.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MinotaurTests.Common
{
  public static class Shaders
  {
    private static Program _spriteShader;

    public static Program SpriteShader
    {
      get
      {
        if (_spriteShader == null)
        {
          string vsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord;
void main() {
  fragTexCoord = TexCoord;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

          string fsSrc = @"#version 150
uniform sampler2D Texture;
uniform vec4 Diffuse = vec4(1);
      
noperspective in vec2 fragTexCoord;
      
out vec4 finalColor;
      
void main() {
    finalColor = texture(Texture, fragTexCoord);
}
";
          _spriteShader = new Program(vsSrc, fsSrc);
        }
        return _spriteShader;
      }
    }

    private static Program _wireframeShader;

    public static Program WireframeShader
    {
      get
      {
        if (_wireframeShader == null)
        {
          string wireframeVsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord_;
void main() {
  fragTexCoord_ = TexCoord;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

          string wireframeGeomSrc = @"#version 150
noperspective in vec2 fragTexCoord_[3];
uniform vec2 WIN_SCALE;
noperspective out vec3 dist;
noperspective out vec2 fragTexCoord;
layout(triangles) in;
layout(triangle_strip, max_vertices = 3) out;
void main(void)
{
// taken from 'Single-Pass Wireframe Rendering'
vec2 p0 = WIN_SCALE * gl_PositionIn[0].xy/gl_PositionIn[0].w;
vec2 p1 = WIN_SCALE * gl_PositionIn[1].xy/gl_PositionIn[1].w;
vec2 p2 = WIN_SCALE * gl_PositionIn[2].xy/gl_PositionIn[2].w;
vec2 v0 = p2-p1;
vec2 v1 = p2-p0;
vec2 v2 = p1-p0;
float area = abs(v1.x*v2.y - v1.y * v2.x);

dist = vec3(area/length(v0),0,0);
fragTexCoord = fragTexCoord_[0];
gl_Position = gl_PositionIn[0];
EmitVertex();
dist = vec3(0,area/length(v1),0);
fragTexCoord = fragTexCoord_[1];
gl_Position = gl_PositionIn[1];
EmitVertex();
dist = vec3(0,0,area/length(v2));
fragTexCoord = fragTexCoord_[2];
gl_Position = gl_PositionIn[2];
EmitVertex();
EndPrimitive();
}
";

          string wireframeFsSrc = @"#version 150
//uniform sampler2D Texture;
uniform vec4 Diffuse = vec4(0.1,0.1,0.1,1);
noperspective in vec3 dist;
noperspective in vec2 fragTexCoord;
      
out vec4 finalColor;
      
void main() {
  // determine frag distance to closest edge
float nearD = min(min(dist[0],dist[1]),dist[2]);
float edgeIntensity = exp2(-1.0*nearD*nearD);
    finalColor = edgeIntensity * Diffuse;
}
";
          _wireframeShader = new Program(new Shader[] {
        new Shader(wireframeFsSrc, ShaderType.FragmentShader),
        new Shader(wireframeGeomSrc, ShaderType.GeometryShader),
        new Shader(wireframeVsSrc, ShaderType.VertexShader)
      });
        }
        return _wireframeShader;
      }
    }

    private static Program _fontShader;

    public static Program FontShader
    {
      get
      {
        if (_fontShader == null)
        {
          string vsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
in vec4 Color;
in vec2 TexCoord;
noperspective out vec2 fragTexCoord;
out vec4 fragColor;
void main() {
  fragTexCoord = TexCoord;
  fragColor = Color;
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

          string fsSrc = @"#version 150
uniform sampler2D Texture;
      
noperspective in vec2 fragTexCoord;
in vec4 fragColor;
      
out vec4 finalColor;
      
void main() {
    finalColor = texture(Texture, fragTexCoord).rrrr * fragColor;
}
";
          _fontShader = new Program(vsSrc, fsSrc);
        }
        return _fontShader;
      }
    }

    private static Program _plainShader;

    public static Program PlainShader
    {
      get
      {
        if (_plainShader == null)
        {
          string vsSrc = @"#version 150
uniform mat4 WorldViewProj;
in vec3 Position;
void main() {
  gl_Position = WorldViewProj * vec4(Position, 1);
}
";

          string fsSrc = @"#version 150
uniform sampler2D Texture;
uniform vec4 Diffuse = vec4(1);
      
out vec4 finalColor;
      
void main() {
    finalColor = Diffuse;
}
";
          _plainShader = new Program(vsSrc, fsSrc);
        }
        return _plainShader;
      }
    }
  }
}
