#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec2 aTexCoords;
layout (location = 2) in vec4 aColor;

out vec2 fTexCoords;
out vec4 fColor;

uniform mat4 uProjection;

void main()
{
    fTexCoords = aTexCoords;
    fColor = aColor;
    gl_Position = uProjection * vec4(aPosition, 1.0);
} 