#version 300 es
precision mediump float;

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec2 aUV;
layout(location = 3) in vec4 aTangent;
layout(location = 4) in ivec4 aBoneIds; 
layout(location = 5) in vec4 aWeights;

uniform mat4 uProjection;
uniform mat4 uView;
uniform mat4 uModel;
	
const int MAX_BONES = 100;
const int MAX_BONE_INFLUENCE = 4;

uniform mat4 uFinalBonesMatrices[MAX_BONES];
	
out vec2 TexCoords;
// Optional: If you need to pass the transformed normal to the fragment shader
// out vec3 TransformedNormal; 
	
void main()
{
    
    vec4 totalPosition = vec4(0.0);
    // Optional: vec3 totalNormal = vec3(0.0);

    for(int i = 0 ; i < MAX_BONE_INFLUENCE ; i++)
    {
        if(aBoneIds[i] == -1) 
            continue;
        if(aBoneIds[i] >= MAX_BONES) 
        {
            totalPosition = vec4(aPosition, 1.0);
            break;
        }
        
        // Fixed: changed 'pos' to 'aPosition'
        vec4 localPosition = uFinalBonesMatrices[aBoneIds[i]] * vec4(aPosition, 1.0);
        totalPosition += localPosition * aWeights[i];
        
        // Fixed: changed 'norm' to 'aNormal'. 
        // Note: If you want to use this normal later, accumulate it like the position:
        // vec3 localNormal = mat3(uFinalBonesMatrices[aBoneIds[i]]) * aNormal;
        // totalNormal += localNormal * aWeights[i];
    }

    if (totalPosition.w == 0.0 || (aWeights.x + aWeights.y + aWeights.z + aWeights.w) == 0.0) {
        totalPosition = vec4(aPosition, 1.0);
    }
		
    mat4 viewModel = uView * uModel;
    gl_Position = uProjection * viewModel * totalPosition;
    

    //gl_Position =  uProjection * uView * uModel * vec4(aPosition, 1.0);

    // Fixed: changed 'tex' to 'aUV'
    TexCoords = aUV; 
}