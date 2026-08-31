#version 300 es
precision mediump float;


float log10(float x)
{
    float f = log(x) / log(10.0);
    return f;
}


float satf(float x)
{
    float f = clamp(x, 0.0, 1.0);
    return f;
}


vec2 satv(vec2 x)
{
    vec2 v = clamp(x, vec2(0.0), vec2(1.0));
    return v;
}


float max2(vec2 v)
{
    float f = max(v.x, v.y);
    return f;
}


in vec3 WorldPos;

out vec4 fragColor;

const float gGridCellSize = 0.025;
const vec4 gGridColorThin = vec4(0.5, 0.5, 0.5, 1.0);
const vec4 gGridColorThick = vec4(0.0, 0.0, 0.0, 1.);

void main() {

   // float GridCellSizeLod0 = gGridCellSize * pow(10.0, gGridCellSize);
    
    vec2 dvx = vec2(dFdx(WorldPos.x), dFdy(WorldPos.x));
    vec2 dvy = vec2(dFdx(WorldPos.z), dFdy(WorldPos.z));
    
    
    float lx = length(dvx);
    float ly = length(dvy);
    
    vec2 dudv = vec2(lx, ly);

    float l = length(dudv);

    //float LOD = max(0.0, log10(l * 2. / gGridCellSize) + 1.0);

    //float GridCellSizeLod0 = gGridCellSize * pow(10.0, floor(LOD));
    
    dudv *= 4.;
    
    vec2 mod_div_dudv = mod(WorldPos.xz, gGridCellSize) / dudv;
    
    float Lod0a = max2(vec2(1.0) - abs(satv(mod_div_dudv) * 2.0 - vec2(1.0)));
    
    vec4 Color = gGridColorThick;
    Color.a *= Lod0a;

    fragColor = Color;
        
}
