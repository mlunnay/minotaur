#version 150

uniform sampler2D Texture;
uniform vec4 Diffuse;

in vec2 fragTexCoord;

out vec4 finalColor;

void main() {
    finalColor = texture(Texture, fragTexCoord) * Diffuse;
}