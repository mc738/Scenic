#version 300 es
precision mediump float;

//in vec2 frag_textCoords;

//uniform sampler2D uTexture0;

out vec4 out_color;

void main()
{
    out_color = vec4(1., 0., 1., 1.); //texture(uTexture0,  frag_textCoords);
}