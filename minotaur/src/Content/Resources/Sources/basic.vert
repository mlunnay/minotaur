#version 150

uniform mat4 WorldViewProj;

in vec3 Position;
in vec2 TexCoord;

out vec2 fragTexCoord;

void main() {
    // Pass the tex coord straight through to the fragment shader
    fragTexCoord = TexCoord;
    
    // Apply all matrix transformations to vert
    gl_Position = WorldViewProj * vec4(Position, 1);
}