#version 300 es
precision mediump float;

uniform mat4 uView;
uniform mat4 uProjection;
uniform vec3 uCameraPosition;


out vec3 WorldPos;

const vec3 Pos[4] = vec3[4](
vec3(-100., 0., -100.),
vec3(100., 0., -100.),
vec3(100., 0., 100.),
vec3(-100., 0., 100.)
);

const int Indices[6] = int[6](0, 2, 1, 2, 0, 3);

void main()
{
    int index = Indices[gl_VertexID];
    
    vec3 vPos3 = Pos[index];

    vPos3.x += uCameraPosition.x;
    vPos3.z += uCameraPosition.z;
    
    vec4 vPos =  vec4(vPos3, 1.0);
    gl_Position = uProjection * uView * vPos;
    
    WorldPos = vPos3;
}