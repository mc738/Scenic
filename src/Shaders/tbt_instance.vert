#version 300 es
precision mediump float;
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aUv;
layout (location = 3) in mat4 uModelMatrix;
layout (location = 7) in vec3 aColor;

out vec2 fUv;
out vec3 fColor;

uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    gl_Position = uProjection * uView * uModelMatrix * vec4(aPos, 1.0);
    fUv = aUv;
    fColor = aColor;
}