#version 330 core
out vec4 FragColor;

in vec2 fTexCoords;
in vec4 fColor;

uniform sampler2D uTexture;

void main()
{
    FragColor = texture(uTexture, fTexCoords) * fColor;
} 