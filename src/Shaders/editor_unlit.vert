#version 300 es
precision mediump float;

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vUv;
layout (location = 3) in vec4 vTangent;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

//out vec2 frag_textCoords;

void main()
{
    // order matters!
    gl_Position =  uProjection * uView * uModel * vec4(vPos, 1.0);
    //frag_textCoords = vUv;
}