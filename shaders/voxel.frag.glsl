#version 450

layout(location = 0) in vec2 texcoords;
layout(location = 1) in vec4 normal;
layout(location = 2) flat in uint textureID;

layout(location=0) out vec4 outColour;

layout(binding=0) uniform sampler2DArray texSampler;

void main()
{
    outColour=texture(texSampler,vec3(texcoords.x,-texcoords.y,textureID));
    //outColour=vec4((normal.z/normal.w));
    gl_FragDepth=(normal.z/normal.w);
}